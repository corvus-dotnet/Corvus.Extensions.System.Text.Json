// <copyright file="JsonPropertyBagFactoryExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.PropertyBag;

using System;
using System.Collections.Generic;
using System.Text.Json;

using Corvus.Json.PropertyBag.Internal;

/// <summary>
/// Extension methods for <see cref="JsonPropertyBag"/>.
/// </summary>
public static class JsonPropertyBagFactoryExtensions
{
    /// <summary>
    /// Get the property bag as a <see cref="IDictionary{TKey, TValue}"/> of <see cref="string"/> to <see cref="object"/>.
    /// </summary>
    /// <param name="factory">The property bag factory.</param>
    /// <param name="propertyBag">The property bag.</param>
    /// <returns>The property bag as a <see cref="JsonDocument"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the property bag does not support conversion to JSON.
    /// The property bag must have been created by ths Json.NET property bag factory
    /// or associated deserialization logic for this call to succeed.
    /// </exception>
    public static JsonDocument AsJsonDocument(this IJsonPropertyBagFactory factory, IPropertyBag propertyBag)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(propertyBag);

        // In our current implementation we don't really need the factory for this, but
        // since in general, doing things with the JSON in the bag requires the factory,
        // we ask for it here. Future implementation changes could end up requiring it.
        if (propertyBag is not JsonPropertyBag jpb)
        {
            throw new ArgumentException("This property bag did not come from this factory", nameof(propertyBag));
        }

        return JsonDocument.Parse(jpb.RawJson);
    }
}