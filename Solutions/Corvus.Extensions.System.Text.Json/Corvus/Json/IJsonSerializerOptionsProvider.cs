// <copyright file="IJsonSerializerOptionsProvider.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A factory to get the default <see cref="JsonSerializerOptions"/> for the context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This sets up a standard <see cref="JsonSerializerOptions"/> instance with opinionated defaults.
    /// </para>
    /// <para>
    /// The default implementation uses string-based serialized for enums, and resolves <see cref="JsonConverter"/>
    /// instances that are registered in the container.
    /// </para>
    /// </remarks>
    public interface IJsonSerializerOptionsProvider
    {
        /// <summary>
        /// Gets an instance of the default JsonSerializerSettings.
        /// </summary>
        JsonSerializerOptions Instance { get; }
    }
}