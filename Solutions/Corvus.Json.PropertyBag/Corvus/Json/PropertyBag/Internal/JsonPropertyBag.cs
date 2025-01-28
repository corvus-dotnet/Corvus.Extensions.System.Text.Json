// <copyright file="JsonPropertyBag.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.PropertyBag.Internal;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

using Corvus.Json;

/// <summary>
/// A property bag that serializes neatly, and which works well with <see cref="JsonElement"/>.
/// </summary>
internal class JsonPropertyBag : IPropertyBag, IEnumerable<(string Key, PropertyBagEntryType Type)>
{
    private readonly JsonSerializerOptions serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPropertyBag"/> class.
    /// </summary>
    /// <param name="json">The UTF8 json data from which to initialize the property bag.</param>
    /// <param name="serializerOptions">The serializer settings to use for the property bag.</param>
    public JsonPropertyBag(JsonElement json, JsonSerializerOptions serializerOptions)
    {
        // We always store a cloned version to avoid problems with the underlying
        // JsonDocument being disposed. (If the incoming element was already cloned,
        // no copying occurs.)
        // See ADR 0001 for the rationale.
        this.RawJson = json.Clone();
        this.serializerOptions = serializerOptions;
    }

    /// <summary>
    /// Gets the <see cref="JsonElement"/>.
    /// </summary>
    internal JsonElement RawJson { get; }

    /// <summary>
    /// Get a strongly typed property.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="key">The property key.</param>
    /// <param name="result">The result.</param>
    /// <returns>True if the object was found.</returns>
    public bool TryGet<T>(string key, [NotNullWhen(true)] out T result)
    {
        try
        {
            if (this.RawJson.TryGetProperty(key, out JsonElement propertyValue))
            {
                // Although JsonSerializer.Deserialize<JsonElement> succeeds, it
                // seems to allocate additional memory when doing so, even though
                // all our JsonElements are backed by a JsonDocument produced by
                // the call to JsonElement.Clone in the constructor, so in theory
                // it shouldn't need to allocate anything.
                // Special-casing this scenario and just returning a copy of the
                // JsonElement struct avoids allocations in practice.
                if (typeof(T) == typeof(JsonElement))
                {
                    result = (T)(object)propertyValue;
                    return true;
                }

                result = propertyValue.Deserialize<T>(this.serializerOptions)!;
                return true;
            }
        }
        catch (JsonException ex)
        {
            throw new SerializationException(ex);
        }

        result = default!;
        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<(string Key, PropertyBagEntryType Type)> GetEnumerator()
    {
        return this.RawJson
            .EnumerateObject()
            .Select(jsonProperty => (
                jsonProperty.Name,
                jsonProperty.Value.ValueKind switch
                {
                    JsonValueKind.Object => PropertyBagEntryType.Object,
                    JsonValueKind.Array => PropertyBagEntryType.Array,
                    JsonValueKind.String => PropertyBagEntryType.String,
                    JsonValueKind.Number => jsonProperty.Value.TryGetInt64(out _)
                        ? PropertyBagEntryType.Integer
                        : PropertyBagEntryType.Decimal,
                    JsonValueKind.True => PropertyBagEntryType.Boolean,
                    JsonValueKind.False => PropertyBagEntryType.Boolean,
                    JsonValueKind.Null => PropertyBagEntryType.Null,

                    _ => throw new InvalidOperationException($"Unexpected value kind of {jsonProperty.Value.ValueKind}"),
                }))
            .ToList().GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}