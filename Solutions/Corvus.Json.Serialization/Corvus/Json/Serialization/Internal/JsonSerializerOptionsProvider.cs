﻿// <copyright file="JsonSerializerOptionsProvider.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.Serialization.Internal
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Corvus.Json.Serialization;

    /// <summary>
    /// Provides access to <see cref="JsonSerializerOptions"/> via DI, incorporating any
    /// <see cref="JsonConverter"/> instances registered in DI.
    /// </summary>
    internal class JsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerOptionsProvider"/> class.
        /// </summary>
        /// <param name="converters">The list of JsonConverters to add.</param>
        /// <remarks>
        /// You should not modify these settings directly. They are shared by all users.
        /// </remarks>
        public JsonSerializerOptionsProvider(IEnumerable<JsonConverter> converters)
        {
            this.Instance = new JsonSerializerOptions
            {
                AllowTrailingCommas = false,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                IgnoreReadOnlyProperties = false,
                PropertyNameCaseInsensitive = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReadCommentHandling = JsonCommentHandling.Skip,
                WriteIndented = false,
                MaxDepth = 4096,
                Encoder = null,
            };

            foreach (JsonConverter converter in converters)
            {
                this.Instance.Converters.Add(converter);
            }
        }

        /// <summary>
        /// Gets the instance of <see cref="JsonSerializerOptions"/> to use as the default.
        /// </summary>
        public JsonSerializerOptions Instance { get; }
    }
}