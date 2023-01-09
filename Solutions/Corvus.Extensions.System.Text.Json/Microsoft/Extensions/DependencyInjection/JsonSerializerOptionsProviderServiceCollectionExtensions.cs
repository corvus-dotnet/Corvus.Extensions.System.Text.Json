// <copyright file="JsonSerializerOptionsProviderServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Corvus.Json;
    using Corvus.Json.Internal;

    /// <summary>
    /// Service collection extension methods for JSON serialization settings.
    /// </summary>
    public static class JsonSerializerOptionsProviderServiceCollectionExtensions
    {
        /// <summary>
        /// Add the default JSON serialization settings provider.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <returns>The service collection.</returns>
        /// <remarks>
        /// <para>
        /// Calling this method is now equivalent to calling the following methods:
        /// </para>
        /// <list type="bullet">
        ///     <item><c>AddJsonSerializerSettingsProvider</c></item>
        ///     <item><c>AddJsonPropertyBagFactory</c></item>
        ///     <item><c>AddJsonCultureInfoConverter</c></item>
        ///     <item><c>AddJsonNetDateTimeOffsetToIso8601AndUnixTimeConverter</c></item>
        /// </list>
        /// <para>
        /// It also adds the <see cref="JsonStringEnumConverter"/>, specifying that enumerations are written as camel cased
        /// strings.
        /// </para>
        /// <para>
        /// The equivalent replacement code for this method is as follows:
        /// </para>
        /// <code>
        /// <![CDATA[
        /// services.AddJsonSerializerSettingsProvider();
        /// services.AddJsonPropertyBagFactory();
        /// services.AddJsonCultureInfoConverter();
        /// services.AddJsonNetDateTimeOffsetToIso8601AndUnixTimeConverter();
        /// services.AddSingleton<JsonConverter>(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        /// ]]>
        /// </code>
        /// </remarks>
        [Obsolete("This method is replaced by separate methods to register the different types of component in this library. See the remarks for the equivalent code you should use.")]
        public static IServiceCollection AddJsonSerializerSettings(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new System.ArgumentNullException(nameof(services));
            }

            if (services.Any(s => typeof(IJsonSerializerOptionsProvider).IsAssignableFrom(s.ServiceType)))
            {
                // Already configured
                return services;
            }

            services.AddTransient<JsonPropertyBag>();
            services.AddSingleton<JsonConverter, CultureInfoConverter>();
            services.AddSingleton<JsonConverter, DateTimeOffsetConverter>();
            services.AddSingleton<JsonConverter, PropertyBagConverter>();
            services.AddSingleton<JsonConverter>(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            services.AddSingleton<IJsonSerializerOptionsProvider, JsonSerializerOptionsProvider>();
            return services;
        }
    }
}