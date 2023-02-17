// <copyright file="CorvusJsonPropertyBagSerializationServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Text.Json.Serialization;

using Corvus.Json;
using Corvus.Json.PropertyBag;
using Corvus.Json.PropertyBag.Internal;

/// <summary>
/// JSON service collection extension methods.
/// </summary>
public static class CorvusJsonPropertyBagSerializationServiceCollectionExtensions
{
    /// <summary>
    /// Add the System.Text.Json implementation of <see cref="IPropertyBagFactory"/> and <see cref="IPropertyBag"/>.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// To get a configuration equivalent to what the old Corvus.Extensions.Newtonsoft setup did, do the following:
    /// </para>
    /// <code>
    /// <![CDATA[
    /// services.AddJsonSerializerSettingsProvider();
    /// services.AddJsonPropertyBagFactory();
    /// services.AddJsonCultureInfoConverter();
    /// services.AddJsonDateTimeOffsetToIso8601AndUnixTimeConverter();
    /// services.AddSingleton<JsonConverter>(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    /// ]]>
    /// </code>
    /// <para>
    /// The <c>DateTimeOffsetToIso8601AndUnixTimeConverter</c> is typically not needed by new apps, and
    /// the CultureInfoConverter is generally needed only if you need to serialize <c>CultureInfo</c>
    /// instances as culture strings (e.g. <c>en-US</c>). So many applications will require just this:
    /// </para>
    /// <code>
    /// <![CDATA[
    /// services.AddJsonSerializerSettingsProvider();
    /// services.AddJsonPropertyBagFactory();
    /// services.AddSingleton<JsonConverter>(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    /// ]]>
    /// </code>
    /// </remarks>
    public static IServiceCollection AddJsonPropertyBagFactory(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(s => typeof(IJsonPropertyBagFactory).IsAssignableFrom(s.ServiceType)))
        {
            services.AddSingleton<IJsonPropertyBagFactory, JsonPropertyBagFactory>();
            services.AddSingleton<IPropertyBagFactory>(sp => sp.GetRequiredService<IJsonPropertyBagFactory>());

            services.AddSingleton<JsonConverter, PropertyBagConverter>();
        }

        return services;
    }
}