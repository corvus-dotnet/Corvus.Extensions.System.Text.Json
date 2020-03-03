// <copyright file="DateTimeOffsetConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json.Internal
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A standard json converter for <see cref="DateTimeOffset"/>.
    /// </summary>
    public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            if (objectType is null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            return typeof(DateTimeOffset) == objectType;
        }

        /// <inheritdoc/>
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && reader.TryGetDateTimeOffset(out DateTimeOffset resultAsDate))
            {
                return resultAsDate;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                DateTimeOffset result;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        // No property present, return null value
                        return result;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        if (reader.ValueTextEquals("dateTimeOffset"))
                        {
                            // reader to the value
                            reader.Read();
                            result = reader.GetDateTimeOffset();
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                }
            }

            return default;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>
        /// Note that this will write the <see cref="DateTimeOffset"/> as a complex object containing both an ISO Date Time string with timezone, and a unix time long.
        /// </para>
        /// <code>
        /// ![<![CDATA[ { "dateTimeOffset": "2009-06-15T13:45:30.0000000-07:00", "unixTime": 1245098730000]]>
        /// </code>
        /// </remarks>
        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("dateTimeOffset", value);
            writer.WriteNumber("unixTime", value.ToUnixTimeMilliseconds());
            writer.WriteEndObject();
        }
    }
}
