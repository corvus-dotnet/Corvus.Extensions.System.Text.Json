// <copyright file="JsonStringSpecificEnumConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides type-specific string conversion handling for enumeration types.
/// </summary>
internal class JsonStringSpecificEnumConverter : JsonConverterFactory
{
    private readonly Dictionary<Type, Policy> policies;

    /// <summary>
    /// Creates a <see cref="JsonStringSpecificEnumConverter"/>.
    /// </summary>
    /// <param name="policies">
    /// The naming policy to apply for this type.
    /// </param>
    public JsonStringSpecificEnumConverter(
        IEnumerable<Policy> policies)
    {
        this.policies = policies.ToDictionary(p => p.EnumType);
    }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => this.policies.ContainsKey(typeToConvert);

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        (_, JsonNamingPolicy? namingPolicy, bool allowIntegerValues) = this.policies[typeToConvert];
        return new JsonStringEnumConverter(namingPolicy, allowIntegerValues).CreateConverter(typeToConvert, options);
    }

    /// <summary>
    /// A policy for a specific enumeration type.
    /// </summary>
    /// <param name="EnumType">The type of the enumeration.</param>
    /// <param name="NamingPolicy">The naming policy to use for the enumeration values.</param>
    /// <param name="AllowIntegerValues">A value indicating whether integer values are allowed for the enumeration.</param>
    internal record Policy(Type EnumType, JsonNamingPolicy? NamingPolicy, bool AllowIntegerValues);
}