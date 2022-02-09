// <copyright file="IPropertyBagWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json
{
    using System;

    /// <summary>
    /// Provides the ability to efficiently write to a property bag.
    /// </summary>
    public interface IPropertyBagWriter : IDisposable
    {
        /// <summary>
        /// Get a strongly typed property.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="key">The property key.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if the object was found.</returns>
        bool TryGet<T>(string key, out T result);

        /// <summary>
        /// Set a strongly typed property.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key for the property.</param>
        /// <param name="value">The value of the property.</param>
        void Set<T>(string key, T value);

        /// <summary>
        /// Commits the changes to the property bag.
        /// </summary>
        /// <remarks>
        /// This allows you to update the values in the <see cref="PropertyBag"/>
        /// while continuing to make further changes if required.
        /// </remarks>
        void Commit();

        /// <summary>
        /// This reverts any changes made since the last <see cref="Commit()"/>.
        /// </summary>
        void Revert();
    }
}