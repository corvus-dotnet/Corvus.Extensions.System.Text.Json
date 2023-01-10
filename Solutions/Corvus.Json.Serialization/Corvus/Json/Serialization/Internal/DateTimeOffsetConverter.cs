// <copyright file="DateTimeOffsetConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.Serialization.Internal
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
            if (reader.TokenType == JsonTokenType.String)
            {
                if (!reader.TryGetDateTimeOffset(out DateTimeOffset resultAsDate))
                {
                    throw new JsonException("String was not a valid DateTimeOffset");
                }

                return resultAsDate;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        // No property present, return null value
                        throw new JsonException("Cannot parse object as DateTimeOffset unless it contains a dateTimeOffset property");
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        if (reader.ValueTextEquals("dateTimeOffset"))
                        {
                            // Advance reader to the value
                            reader.Read();
                            DateTimeOffset result = reader.GetDateTimeOffset();

                            // There may be other properties (e.g. the unixTime one might follow the
                            // dateTimeOffset) so we skip over all other properies until we reach the
                            // end of the object.
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                            {
                                reader.Skip();
                            }

                            return result;
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                }
            }

            throw new JsonException("Cannot parse object as DateTimeOffset unless it is either a string, or an object with a dateTimeOffset property");
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