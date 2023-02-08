// <copyright file="JsonElementBackedPropertyBagWithExposedJsonElement.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.PropertyBag.Internal;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;

using Corvus.Json;

/// <summary>
/// This is essentially the same as the real implementation, except that it exposes the underlying
/// <see cref="JsonElement"/> directly.
/// </summary>
/// <remarks>
/// This has been retained for benchmarking comparison purposes. When the decision was taken not to
/// expose the underlying <see cref="JsonElement"/> directly in the public API (see ADR 0001), we
/// compared the memory use and performance of this implementation which DOES make it available
/// against the implementation we ultimately chose. (It turned out that we were able to make
/// property bag contents available as <see cref="JsonElement"/>s through the existing
/// <see cref="TryGet{T}(string, out T)"/> method by detecting when the type argument was
/// <see cref="JsonElement"/> and providing a special zero-allocation code path.) Instead of just
/// discarding this prototype, we've moved into the benchmark project to enable those
/// measurements to be repeated, to verify that on future runtimes, the chosen implementation
/// continues to offer performance that is just as good as this more specialized approach.
/// (This is particularly important because we are effectively relying on the CLR's JIT to do a
/// good job here, so runtime changes could make this decision look wrong in the future.)
/// </remarks>
public class JsonElementBackedPropertyBagWithExposedJsonElement : IPropertyBag, IEnumerable<(string Key, PropertyBagEntryType Type)>
{
    private readonly JsonSerializerOptions serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonElementBackedPropertyBagWithExposedJsonElement"/> class.
    /// </summary>
    /// <param name="json">The UTF8 json data from which to initialize the property bag.</param>
    /// <param name="serializerOptions">The serializer settings to use for the property bag.</param>
    public JsonElementBackedPropertyBagWithExposedJsonElement(JsonElement json, JsonSerializerOptions serializerOptions)
    {
        this.RawJson = json.Clone();
        this.serializerOptions = serializerOptions;
    }

    /// <summary>
    /// Gets the <see cref="JsonElement"/>.
    /// </summary>
    public JsonElement RawJson { get; }

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
                // Special-casing this scenario and just returning theavoids
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