// <copyright file="CorvusJsonSerializationServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Corvus.Json.Serialization;
using Corvus.Json.Serialization.Internal;

/// <summary>
/// JSON serialization service collection extension methods.
/// </summary>
public static class CorvusJsonSerializationServiceCollectionExtensions
{
    /// <summary>
    /// Add the default JSON serialization options provider.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="configurationCallback">Optional callback used to modify the <see cref="JsonSerializerOptions"/>.</param>
    /// <returns>The service collection.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this service has already been configured, and this call supplied a callback. (We will be unable
    /// to invoke the callback supplied in this case.)
    /// </exception>
    public static IServiceCollection AddJsonSerializerOptionsProvider(
        this IServiceCollection services,
        Action<IServiceProvider, JsonSerializerOptions>? configurationCallback = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(s => typeof(IJsonSerializerOptionsProvider).IsAssignableFrom(s.ServiceType)))
        {
            services.AddSingleton<IJsonSerializerOptionsProvider>(
                sp =>
                {
                    IEnumerable<JsonConverter> converters = sp.GetServices<JsonConverter>();
                    var serializerSettingsProvider = new JsonSerializerOptionsProvider(converters);

                    configurationCallback?.Invoke(sp, serializerSettingsProvider.Instance);

                    return serializerSettingsProvider;
                });
        }
        else if (configurationCallback is not null)
        {
            throw new InvalidOperationException("Service configuration for IJsonSerializerSettingsProvider has already completed, so it is not possible to invoke the supplied configurationCallback");
        }

        return services;
    }

    /// <summary>
    /// Add the <see cref="CultureInfoConverter"/> to the service collection. This ensures that members of type
    /// <see cref="CultureInfo"/> are serialized as their <see cref="CultureInfo.Name"/>.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddJsonCultureInfoConverter(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(s => s.ImplementationType == typeof(CultureInfoConverter)))
        {
            services.AddSingleton<JsonConverter, CultureInfoConverter>();
        }

        return services;
    }

    /// <summary>
    /// Add the <see cref="DateTimeOffsetConverter"/> to the service collection. This ensures that members of type
    /// <see cref="DateTimeOffset"/> are serialized as an object containing the date/time in ISO 8601 form, as well
    /// a unix time long.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// This converter is useful when the resultant serialized data is used with a store that supports querying
    /// and filtering. By default, ISO 8601 date/time strings include a timezone offset, but this means that
    /// it's not possible to use standard string comparison for sorting/filtering. Using the converter means
    /// that the Unix time in the resulting object can be used for sorting/filtering, while the ISO 8601 form
    /// is retained for deserialization so that timezone information is not lost.
    /// </para>
    /// <para>
    /// If you don't wish to use this converter, the alternative is to ensure all date/time values are stored as
    /// UTC. This means that standard string comparisons are viable, at the expense of not retaining the original
    /// time zone values.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddJsonDateTimeOffsetToIso8601AndUnixTimeConverter(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(s => s.ImplementationType == typeof(DateTimeOffsetConverter)))
        {
            services.AddSingleton<JsonConverter, DateTimeOffsetConverter>();
        }

        return services;
    }

    /// <summary>
    /// Adds type-specific serialization handling for enumeration types. This must be called before
    /// registering any converters that will handle all enumeration types.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="namingPolicy">The naming policy to apply for these types.</param>
    /// <param name="allowIntegerValues">
    /// Indicates whether enumeration types' integer values will be accepted during
    /// deserialization.
    /// </param>
    /// <param name="enumTypes">The types for which to apply a specific naming policy.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// Some applications use a mixture of casing conventions when serializing enumerations types.
    /// This might be to conform to external specifications, or simply for backwards compatibility
    /// with older service versions. In these cases, a single global policy cannot be registered.
    /// </para>
    /// <para>
    /// If an application wants to register a global default enumeration handling policy, but to
    /// override the behaviour for one or more specific enumeration types, the registration order
    /// matters: you MUST call this method first. <c>System.Text.Json</c> will consider converters
    /// in the order in which we supply them, meaning that if any general-purpose converters are
    /// registered ahead of specialized converters, the specialized ones won't get a chance to run
    /// for types that the more generalized ones say they can handle.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddConverterForEnums(
        this IServiceCollection services,
        JsonNamingPolicy? namingPolicy,
        bool allowIntegerValues,
        params Type[] enumTypes)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(s => s.ImplementationType == typeof(JsonStringSpecificEnumConverter)))
        {
            services.AddSingleton<JsonConverter, JsonStringSpecificEnumConverter>();
        }

        foreach (Type enumType in enumTypes)
        {
            services.AddSingleton(new JsonStringSpecificEnumConverter.Policy(enumType, namingPolicy, allowIntegerValues));
        }

        return services;
    }

    /// <summary>
    /// Adds type-specific camelCased serialization handling for enumeration types. This must be
    /// called before registering any converters that will handle all enumeration types.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="enumTypes">The types for which to apply a specific naming policy.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// Some applications use a mixture of casing conventions when serializing enumerations types.
    /// This might be to conform to external specifications, or simply for backwards compatibility
    /// with older service versions. In these cases, a single global policy cannot be registered.
    /// </para>
    /// <para>
    /// If an application wants to register a global default enumeration handling policy, but to
    /// override the behaviour for one or more specific enumeration types, the registration order
    /// matters: you MUST call this method first. <c>System.Text.Json</c> will consider converters
    /// in the order in which we supply them, meaning that if any general-purpose converters are
    /// registered ahead of specialized converters, the specialized ones won't get a chance to run
    /// for types that the more generalized ones say they can handle.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddCamelCaseConverterForEnums(
        this IServiceCollection services,
        params Type[] enumTypes)
    {
        return services.AddConverterForEnums(JsonNamingPolicy.CamelCase, false, enumTypes);
    }

    /// <summary>
    /// Adds type-specific PascalCased serialization handling for enumeration types. This must be
    /// called before registering any converters that will handle all enumeration types.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="enumTypes">The types for which to apply a specific naming policy.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// Some applications use a mixture of casing conventions when serializing enumerations types.
    /// This might be to conform to external specifications, or simply for backwards compatibility
    /// with older service versions. In these cases, a single global policy cannot be registered.
    /// </para>
    /// <para>
    /// If an application wants to register a global default enumeration handling policy, but to
    /// override the behaviour for one or more specific enumeration types, the registration order
    /// matters: you MUST call this method first. <c>System.Text.Json</c> will consider converters
    /// in the order in which we supply them, meaning that if any general-purpose converters are
    /// registered ahead of specialized converters, the specialized ones won't get a chance to run
    /// for types that the more generalized ones say they can handle.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddPascalCaseConverterForEnums(
        this IServiceCollection services,
        params Type[] enumTypes)
    {
        return services.AddConverterForEnums(null, false, enumTypes);
    }
}