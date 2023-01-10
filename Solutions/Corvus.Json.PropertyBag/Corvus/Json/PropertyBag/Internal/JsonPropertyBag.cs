// <copyright file="JsonPropertyBag.cs" company="Endjin Limited">
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
/// A property bag that serializes neatly.
/// </summary>
internal class JsonPropertyBag : IPropertyBag, IEnumerable<(string Key, PropertyBagEntryType Type)>
{
    private static readonly byte[] EmptyObject = Encoding.UTF8.GetBytes("{}");

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPropertyBag"/> class.
    /// </summary>
    /// <param name="json">The UTF8 json data from which to initialize the property bag.</param>
    /// <param name="serializerOptions">The serializer settings to use for the property bag.</param>
    public JsonPropertyBag(ReadOnlyMemory<byte> json, JsonSerializerOptions serializerOptions)
    {
        this.RawJson = json;
        this.SerializerOptions = serializerOptions;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPropertyBag"/> class.
    /// </summary>
    /// <param name="json">The json string from which to initialize the property bag.</param>
    /// <param name="serializerOptions">The serializer settings to use for the property bag.</param>
    public JsonPropertyBag(string json, JsonSerializerOptions serializerOptions)
        : this(string.IsNullOrEmpty(json) ? EmptyObject : Encoding.UTF8.GetBytes(json), serializerOptions)
    {
    }

    /// <summary>
    /// Gets the serializer options for the property bag.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; }

    /// <summary>
    /// Gets the underlying JSON string which captures the extension properties.
    /// </summary>
    internal ReadOnlyMemory<byte> RawJson { get; }

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
            var reader = new Utf8JsonReader(this.RawJson.Span);
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.ValueTextEquals(key))
                    {
                        reader.Read();
                        result = JsonSerializer.Deserialize<T>(ref reader, this.SerializerOptions)!;
                        return true;
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
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
        // Because JsonDocument is disposable, we have three choices:
        //  1. enumerate all the properties into a list up front
        //  2. implement a custom enumerator that disposes the JsonDocument in its own Dispose
        //  3. implement a custom enumerator with Utf8JsonReader, using JsonReaderState to hold
        //      state between calls to MoveNext
        // We're currently using 1 because it's the most straightforward, and although it
        // entails additional allocations, enumeration of property bag contents is never going
        // to be zero-allocation because property bags are always used via IPropertyBag.
        // (So even if this type defined a custom GetEnumerator that returned a struct,
        // consumers wouldn't be able to see it because this type is internal. That's partly
        // because this isn't the only IPropertyBag implementation - there's an older
        // Json.NET one. It was also by design that the implementation details of property
        // bags are concealed, partly to enable the migration to System.Text.Json that
        // we knew was coming, but also to enable flexibility in how the type stores data.)
        // So it's not clear that it would be worth the effort to develop a more efficient
        // enumerable implementation.
        using var doc = JsonDocument.Parse(this.RawJson);
        return doc.RootElement
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