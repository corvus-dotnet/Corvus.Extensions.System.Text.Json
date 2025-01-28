// <copyright file="ObjectInPropertyBagClassicSerializable.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.PropertyBag.Benchmarks;

/// <summary>
/// Type used in benchmarks that test performance of deserializing object-like data.
/// </summary>
/// <param name="IntValue">A 32-bit integer.</param>
/// <param name="Int64Value">A 64-bit integer.</param>
/// <param name="BoolValue">A boolean value.</param>
/// <param name="StringValue">A string.</param>
public record ObjectInPropertyBagClassicSerializable(int IntValue, long Int64Value, bool BoolValue, string StringValue);