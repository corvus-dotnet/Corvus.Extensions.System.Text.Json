// <copyright file="PropertyBagExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json
{
    using System.Collections.Generic;
    using System.Text.Json;

    /// <summary>
    /// Extension methods for <see cref="PropertyBag"/>.
    /// </summary>
    public static class PropertyBagExtensions
    {
        /// <summary>
        /// Get the property bag as a <see cref="IDictionary{TKey, TValue}"/> of <see cref="string"/> to <see cref="object"/>.
        /// </summary>
        /// <param name="propertyBag">The property bag.</param>
        /// <returns>The property bag as a <see cref="JsonDocument"/>.</returns>
        public static JsonDocument AsJsonDocument(this PropertyBag propertyBag)
        {
            return JsonDocument.Parse(propertyBag.Properties);
        }
    }
}
