// <copyright file="PropertyBag.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// A property bag that serializes neatly.
    /// </summary>
    public class PropertyBag
    {
        /// <summary>
        /// Gets the fallback default JsonSerializerOptions.
        /// </summary>
        public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new();

        private static readonly byte[] EmptyObject = Encoding.UTF8.GetBytes("{}");

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag"/> class.
        /// </summary>
        public PropertyBag()
            : this(new JsonSerializerOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag"/> class.
        /// </summary>
        /// <param name="json">The json string from which to initialize the property bag.</param>
        /// <param name="serializerOptions">The serializer settings to use for the property bag.</param>
        public PropertyBag(string json, JsonSerializerOptions serializerOptions = null)
        {
            this.Properties = string.IsNullOrEmpty(json) ? EmptyObject : Encoding.UTF8.GetBytes(json);
            this.SerializerOptions = serializerOptions ?? DefaultJsonSerializerOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag"/> class.
        /// </summary>
        /// <param name="dictionary">A dictionary with which to initialize the bag.</param>
        /// <param name="serializerOptions">The serializer settings to use for the property bag.</param>
        public PropertyBag(IDictionary<string, object> dictionary, JsonSerializerOptions serializerOptions = null)
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            this.Properties = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dictionary, serializerOptions));
            this.SerializerOptions = serializerOptions ?? DefaultJsonSerializerOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag"/> class.
        /// </summary>
        /// <param name="serializerOptionsProvider">The serializer settings provider.</param>
        public PropertyBag(IJsonSerializerOptionsProvider serializerOptionsProvider)
        {
            if (serializerOptionsProvider is null)
            {
                throw new ArgumentNullException(nameof(serializerOptionsProvider));
            }

            this.Properties = EmptyObject;
            this.SerializerOptions = serializerOptionsProvider.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag"/> class.
        /// </summary>
        /// <param name="serializerSettings">The serializer settings to use for the property bag.</param>
        public PropertyBag(JsonSerializerOptions serializerSettings)
        {
            this.Properties = EmptyObject;
            this.SerializerOptions = serializerSettings ?? DefaultJsonSerializerOptions;
        }

        /// <summary>
        /// Gets the serializer options for the property bag.
        /// </summary>
        public JsonSerializerOptions SerializerOptions { get; }

        /// <summary>
        /// Gets or sets the underlying JSON string which captures the extension properties.
        /// </summary>
        internal byte[] Properties { get; set; }

        /// <summary>
        /// Implicit cast from <see cref="PropertyBag"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="bag">The property bag to cast.</param>
        public static implicit operator string(PropertyBag bag)
        {
            if (bag is null)
            {
                throw new ArgumentNullException(nameof(bag));
            }

            return Encoding.UTF8.GetString(bag.Properties);
        }

        /// <summary>
        /// Get a strongly typed property.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="key">The property key.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if the object was found.</returns>
        public bool TryGet<T>(string key, out T result)
        {
            try
            {
                var reader = new Utf8JsonReader(this.Properties.AsSpan());
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        if (reader.ValueTextEquals(key))
                        {
                            reader.Read();
                            result = JsonSerializer.Deserialize<T>(ref reader, this.SerializerOptions);
                            return true;
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // NOP - fall through t
            }

            result = default;
            return false;
        }
    }
}