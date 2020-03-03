// <copyright file="CultureInfoConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json.Internal
{
    using System;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A standard json converter for <see cref="CultureInfo"/>.
    /// </summary>
    public class CultureInfoConverter : JsonConverter<CultureInfo>
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            if (objectType is null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            return typeof(CultureInfo) == objectType;
        }

        /// <inheritdoc/>
        public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            if (value != null)
            {
                return new CultureInfo(value);
            }

            return default;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }

            if (value is CultureInfo ci)
            {
                writer.WriteStringValue(ci.Name);
            }
            else
            {
                throw new ArgumentException("The object passed was not a CultureInfo", nameof(value));
            }
        }
    }
}
