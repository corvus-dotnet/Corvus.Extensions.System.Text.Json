// <copyright file="IJsonPropertyBagFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.PropertyBag;

using System;
using System.Text.Json;

/// <summary>
/// Provides System.Text.Json-specific operations for working with an <see cref="IPropertyBag"/>.
/// </summary>
public interface IJsonPropertyBagFactory : IPropertyBagFactory
{
    /// <summary>
    /// Creates a new <see cref="IPropertyBag"/> from JSON content in UTF-8 form.
    /// </summary>
    /// <param name="jsonUtf8">
    /// The JSON from which to populate the property bag.
    /// </param>
    /// <returns>
    /// A new property bag.
    /// </returns>
    IPropertyBag Create(ReadOnlyMemory<byte> jsonUtf8);

    /// <summary>
    /// Creates a new <see cref="IPropertyBag"/> from a <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="json">
    /// The <see cref="JsonElement"/> to wrap as a property bag.
    /// </param>
    /// <returns>A property bag.</returns>
    /// <remarks>
    /// This calls <see cref="JsonElement.Clone"/> on the element, so the lifetime of the parent
    /// document does not matter. You are free to dispose of the parent <see cref="JsonDocument"/>
    /// as soon as this method returns.
    /// </remarks>
    IPropertyBag Create(in JsonElement json);

    /// <summary>
    /// Creates a new <see cref="IPropertyBag"/> populated through a <see cref="Utf8JsonWriter"/>.
    /// </summary>
    /// <param name="callback">
    /// A method that will be passed a <see cref="Utf8JsonWriter"/>, which should be used to
    /// write out the JSON object that is to be turned into a property bag.
    /// </param>
    /// <returns>A property bag.</returns>
    IPropertyBag Create(Action<Utf8JsonWriter> callback);

    /// <summary>
    /// Creates a new <see cref="IPropertyBag"/> populated through a <see cref="Utf8JsonWriter"/>.
    /// </summary>
    /// <typeparam name="TContext">The type for the context argument.</typeparam>
    /// <param name="context">
    /// An argument that will be passed straight onto the <paramref name="callback"/>. This can
    /// enable the callback to be a static method where otherwise an instance method (e.g. an
    /// anonymous delegate with captured scope) might have been required.
    /// </param>
    /// <param name="callback">
    /// A method that will be passed the <paramref name="context"/> argument and a <see cref="Utf8JsonWriter"/>, which should be used to
    /// write out the JSON object that is to be turned into a property bag.
    /// </param>
    /// <returns>A property bag.</returns>
    IPropertyBag Create<TContext>(TContext context, Action<TContext, Utf8JsonWriter> callback);

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