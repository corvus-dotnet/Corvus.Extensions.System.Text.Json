// <copyright file="PropertyBagExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json
{
    using System.Collections.Generic;
    using System.Text.Json;
    using Corvus.Extensions.Json.Internal;

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

        /// <summary>
        /// Gets a writer for setting properties on the property bag.
        /// </summary>
        /// <param name="propertyBag">The property bag for which to get the writer.</param>
        /// <returns>A property bag writer which allows you to more efficiently set multiple properties on the bag.</returns>
        /// <remarks>The changes will not be committed to the bag until the writer is disposed.</remarks>
        public static IPropertyBagWriter GetWriter(this PropertyBag propertyBag)
        {
            return new PropertyBagWriter(propertyBag);
        }
    }
}
