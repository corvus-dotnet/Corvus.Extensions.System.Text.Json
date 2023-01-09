// <copyright file="PropertyBagConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.Internal
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Corvus.Json;

    /// <summary>
    /// A json converter for <see cref="JsonPropertyBag"/>.
    /// </summary>
    internal class PropertyBagConverter : JsonConverter<IPropertyBag>
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IPropertyBag) == objectType || typeof(JsonPropertyBag) == objectType;
        }

        /// <inheritdoc/>
        public override IPropertyBag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            return new JsonPropertyBag(document.RootElement.GetRawText(), options);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, IPropertyBag value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(writer);

            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                if (value is not JsonPropertyBag jpb)
                {
                    throw new InvalidOperationException($"Can only serialize {nameof(JsonPropertyBag)}, but this is a {value.GetType().Name}.");
                }

                using var jsonDocument = JsonDocument.Parse(jpb.RawJson);

                jsonDocument.RootElement.WriteTo(writer);
            }
        }
    }
}