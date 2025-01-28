// <copyright file="EnumPoco.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json.Specs.Samples;

using Corvus.Json.Specs.Samples;

/// <summary>
/// A plain old CLR type containing enum-typed members.
/// </summary>
/// <param name="FirstEnumValue">An enumerated value of type <see cref="ExampleEnum"/>.</param>
/// <param name="SecondEnumValue">An enumerated value of type <see cref="ExampleEnum2"/>.</param>
public record EnumPoco(ExampleEnum FirstEnumValue, ExampleEnum2 SecondEnumValue);