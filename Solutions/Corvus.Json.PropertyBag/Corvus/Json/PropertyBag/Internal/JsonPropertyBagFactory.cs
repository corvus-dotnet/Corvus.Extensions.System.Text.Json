// <copyright file="JsonPropertyBagFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.PropertyBag.Internal;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Corvus.Json;
using Corvus.Json.PropertyBag;
using Corvus.Json.Serialization;

/// <summary>
/// A factory for building property bags that serialize neatly using System.Text.Json.
/// </summary>
internal class JsonPropertyBagFactory : IJsonPropertyBagFactory
{
    private readonly IJsonSerializerOptionsProvider serializerOptionsProvider;

    /// <summary>
    /// Creates a <see cref="JsonPropertyBagFactory"/>.
    /// </summary>
    /// <param name="serializerOptionsProvider">Source of serialization settings.</param>
    public JsonPropertyBagFactory(IJsonSerializerOptionsProvider serializerOptionsProvider)
    {
        this.serializerOptionsProvider = serializerOptionsProvider;
    }

    /// <inheritdoc/>
    public IPropertyBag Create(ReadOnlyMemory<byte> jsonUtf8)
    {
        using var doc = JsonDocument.Parse(jsonUtf8);
        return new JsonPropertyBag(doc.RootElement, this.serializerOptionsProvider.Instance);
    }

    /// <inheritdoc/>
    public IPropertyBag Create(IEnumerable<KeyValuePair<string, object>> values)
    {
        IReadOnlyDictionary<string, object> d = values is IReadOnlyDictionary<string, object> id
            ? id : new Dictionary<string, object>(values);

        // We don't just serialize like this:
        //  JsonSerializer.Serialize(ms, d, this.serializerOptionsProvider.Instance);
        // because the JsonSerializerOptions are typically set up for camelCasing. And while
        // that's what we want for the objects in the property bag, the property bag itself
        // needs to preserve the case of the properies
        MemoryStream ms = new();
        using (Utf8JsonWriter w = new(ms))
        {
            w.WriteStartObject();
            foreach ((string key, object value) in d)
            {
                w.WritePropertyName(key);
                if (value is null)
                {
                    w.WriteNullValue();
                }
                else
                {
                    JsonSerializer.Serialize(w, value, value.GetType(), this.serializerOptionsProvider.Instance);
                }
            }

            w.WriteEndObject();
        }

        ms.Flush();
        using var doc = JsonDocument.Parse(ms.ToArray());
        return new JsonPropertyBag(doc.RootElement, this.serializerOptionsProvider.Instance);
    }

    /// <inheritdoc/>
    public IPropertyBag Create(in JsonElement json)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IPropertyBag Create(Action<Utf8JsonWriter> callback)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IPropertyBag Create<TContext>(TContext context, Action<TContext, Utf8JsonWriter> callback)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IPropertyBag CreateModified(
        IPropertyBag input,
        IEnumerable<KeyValuePair<string, object>>? propertiesToSetOrAdd,
        IEnumerable<string>? propertiesToRemove)
    {
        IReadOnlyDictionary<string, object> existingProperties = input.AsDictionary();
        Dictionary<string, object> newProperties = propertiesToSetOrAdd?.ToDictionary(kv => kv.Key, kv => kv.Value)
            ?? new Dictionary<string, object>();
        HashSet<string>? remove = propertiesToRemove == null ? null : new HashSet<string>(propertiesToRemove);
        foreach (KeyValuePair<string, object> existingKv in existingProperties)
        {
            string key = existingKv.Key;
            bool newPropertyWithThisNameExists = newProperties.ContainsKey(key);
            bool existingPropertyIsToBeRemoved = remove?.Contains(key) == true;
            if (newPropertyWithThisNameExists && existingPropertyIsToBeRemoved)
            {
                throw new ArgumentException($"Property {key} appears in both {nameof(propertiesToSetOrAdd)} and {nameof(propertiesToRemove)}");
            }

            if (!newPropertyWithThisNameExists && !existingPropertyIsToBeRemoved)
            {
                newProperties.Add(key, existingKv.Value);
            }
        }

        return this.Create(newProperties);
    }

    /// <inheritdoc/>
    public void WriteTo(IPropertyBag propertyBag, Utf8JsonWriter writer)
    {
        if (propertyBag is not JsonPropertyBag jsonPropertyBag)
        {
            throw new ArgumentException("This property bag did not come from this factory", nameof(propertyBag));
        }

        jsonPropertyBag.RawJson.WriteTo(writer);
    }
}