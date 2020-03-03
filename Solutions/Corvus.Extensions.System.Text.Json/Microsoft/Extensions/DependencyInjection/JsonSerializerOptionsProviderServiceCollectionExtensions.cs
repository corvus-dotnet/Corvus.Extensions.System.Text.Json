// <copyright file="JsonSerializerOptionsProviderServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.Extensions.Json;
    using Corvus.Extensions.Json.Internal;

    /// <summary>
    /// An installer for standard <see cref="JsonConverter"/>s and default settings.
    /// </summary>
    public static class JsonSerializerOptionsProviderServiceCollectionExtensions
    {
        /// <summary>
        /// Add the default JSON serialization settings.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <returns>The service collection.</returns>
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

            services.AddTransient<PropertyBag>();
            services.AddSingleton<JsonConverter, CultureInfoConverter>();
            services.AddSingleton<JsonConverter, DateTimeOffsetConverter>();
            services.AddSingleton<JsonConverter, PropertyBagConverter>();
            services.AddSingleton<JsonConverter>(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            services.AddSingleton<IJsonSerializerOptionsProvider, JsonSerializerOptionsProvider>();
            return services;
        }
    }
}