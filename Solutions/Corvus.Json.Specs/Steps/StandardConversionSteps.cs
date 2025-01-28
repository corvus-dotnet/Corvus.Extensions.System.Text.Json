// <copyright file="StandardConversionSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.Specs.Steps;

using System;
using System.Globalization;
using System.Text.Json;

using Corvus.Json.Specs.Samples;

using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

[Binding]
public class StandardConversionSteps
{
    private readonly JsonExtensionsSpecsSteps jsonSpecs;

    public StandardConversionSteps(
        JsonExtensionsSpecsSteps jsonSpecs)
    {
        this.jsonSpecs = jsonSpecs;
    }

    [Given("I have registered the DateTimeOffsetToIso8601AndUnixTimeConverter")]
    public void GivenIHaveRegisteredTheDateTimeOffsetToIsoAndUnixTimeConverter()
    {
        this.jsonSpecs.Services.AddJsonDateTimeOffsetToIso8601AndUnixTimeConverter();
    }

    [Given("I serialize a POCO with '([^']*)', '([^']*)', '([^']*)', '([^']*)', '([^']*)'")]
    public void GivenISerializeAPOCOWith(string value, string time, string nullableTime, string culture, ExampleEnum someEnum)
    {
        var poco = new PocObject { SomeCulture = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture), SomeDateTime = DateTimeOffset.Parse(time), SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : DateTimeOffset.Parse(nullableTime), SomeEnum = someEnum, SomeValue = value };
        this.jsonSpecs.SerializedJson = JsonSerializer.Serialize(poco, this.jsonSpecs.SerializerOptions);
    }
}