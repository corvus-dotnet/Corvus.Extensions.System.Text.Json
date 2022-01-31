// <copyright file="PropertyBagWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json.Internal
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// An implementation of the <see cref="IPropertyBagWriter"/> interface.
    /// </summary>
    internal class PropertyBagWriter : IPropertyBagWriter
    {
        private readonly Dictionary<string, object> entities = new();
        private readonly PropertyBag bag;
        private JsonDocument document;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBagWriter"/> class.
        /// </summary>
        /// <param name="bag">The bag for which to create the writer.</param>
        public PropertyBagWriter(PropertyBag bag)
        {
            this.bag = bag;
            this.Revert();
        }

        /// <inheritdoc/>
        public void Revert()
        {
            this.document?.Dispose();
            this.document = this.bag.AsJsonDocument();
            this.BuildJsonElementDictionaryFromObject();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Commit();
            this.document?.Dispose();
        }

        /// <inheritdoc/>
        public void Commit()
        {
            this.bag.Properties = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this.entities, this.bag.SerializerOptions));
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value)
        {
            this.entities[key] = value;
        }

        /// <inheritdoc/>
        public bool TryGet<T>(string key, out T result)
        {
            if (this.entities[key] is T value)
            {
                result = value;
                return true;
            }

            if (this.entities[key] is JsonElement jsonElement)
            {
                try
                {
                    result = JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), this.bag.SerializerOptions);
                    return true;
                }
                catch (JsonException)
                {
                    result = default;
                    return false;
                }
            }

            result = default;
            return false;
        }

        private void BuildJsonElementDictionaryFromObject()
        {
            foreach (JsonProperty element in this.document.RootElement.EnumerateObject())
            {
                this.entities.Add(element.Name, element.Value);
            }
        }
    }
}