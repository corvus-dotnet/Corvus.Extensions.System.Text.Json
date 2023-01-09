// <copyright file="IJsonPropertyBagFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json;

using System;
using System.Text.Json;

/// <summary>
/// Provides System.Text.Json-specific operations for working with an <see cref="IPropertyBag"/>.
/// </summary>
public interface IJsonPropertyBagFactory : IPropertyBagFactory
{
    /// <summary>
    /// Creates a new <see cref="IPropertyBag"/> with the specified properties.
    /// </summary>
    /// <param name="jsonUtf8">
    /// The JSON from which to populate the property bag.
    /// </param>
    /// <returns>
    /// A new property bag.
    /// </returns>
    IPropertyBag Create(ReadOnlyMemory<byte> jsonUtf8);

    /// <summary>
    /// Writes the content of an <see cref="IPropertyBag"/> to a <see cref="Utf8JsonWriter"/>.
    /// </summary>
    /// <param name="propertyBag">The property bag.</param>
    /// <param name="writer">The target into which to write the JSON.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the property bag cannot be converted to JSON. The property bag must have been
    /// created by this System.Text.Json property bag factory or associated deserialization logic
    /// for this call to succeed.
    /// </exception>
    void WriteTo(IPropertyBag propertyBag, Utf8JsonWriter writer);
}