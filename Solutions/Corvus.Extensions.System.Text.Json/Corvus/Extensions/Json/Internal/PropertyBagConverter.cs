// <copyright file="PropertyBagConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json.Internal
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.Extensions.Json;

    /// <summary>
    /// A standard json converter for <see cref="PropertyBag"/>.
    /// </summary>
    public class PropertyBagConverter : JsonConverter<PropertyBag>
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(PropertyBag) == objectType;
        }

        /// <inheritdoc/>
        public override PropertyBag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            return new PropertyBag(document.RootElement.GetRawText(), options);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, PropertyBag value, JsonSerializerOptions options)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                using JsonDocument jsonDocument = value.AsJsonDocument();
                jsonDocument.WriteTo(writer);
            }
        }
    }
}