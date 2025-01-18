// <copyright file="EnumCasingSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.Specs.Steps;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Corvus.Extensions.Json.Specs.Samples;
using Corvus.Json.Specs.Samples;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;
using Reqnroll;

[Binding]
public class EnumCasingSteps
{
    private readonly JsonExtensionsSpecsSteps jsonSteps;

    private EnumPoco? deserialized;

    public EnumCasingSteps(JsonExtensionsSpecsSteps jsonSteps)
    {
        this.jsonSteps = jsonSteps;
    }

    private ServiceCollection Services => this.jsonSteps.Services;

    private EnumPoco Deserialized => this.deserialized ?? throw new InvalidOperationException("Test has not deserialized the EnumPoco yet");

    [Given("I have registered a global enum policy of '([^']*)'")]
    public void GivenIHaveRegisteredAGlobalEnumPolicyOf(string policy)
    {
        switch (policy)
        {
            case "CamelCase":
                this.Services.AddSingleton<JsonConverter>(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                break;

            case "PascalCase":
                this.Services.AddSingleton<JsonConverter>(new JsonStringEnumConverter());
                break;
        }
    }

    [Given("I have registered an enum policy of '([^']*)' for '([^']*)'")]
    public void GivenIHaveRegisteredAnEnumPolicyOfForExampleEnum(string policy, string type)
    {
        Type t = type switch
        {
            nameof(ExampleEnum) => typeof(ExampleEnum),
            nameof(ExampleEnum2) => typeof(ExampleEnum2),
            _ => throw new ArgumentException($"Unrecognized enum type '{type}'", nameof(type)),
        };

        switch (policy)
        {
            case "CamelCase":
                this.Services.AddCamelCaseConverterForEnums(t);
                break;

            case "PascalCase":
                this.Services.AddPascalCaseConverterForEnums(t);
                break;
        }
    }

    [When("I serialize an EnumPoco POCO with '([^']*)', '([^']*)'")]
    public void WhenISerializeAnEnumPocoPOCOWith(string firstValue, string secondValue)
    {
        EnumPoco data = new(Enum.Parse<ExampleEnum>(firstValue), Enum.Parse<ExampleEnum2>(secondValue));

        this.jsonSteps.SerializedJson = JsonSerializer.Serialize(data, this.jsonSteps.SerializerOptions);
    }

    [When("I deserialize an EnumPoco POCO with the json string '([^']*)'")]
    public void WhenIDeserializeAnEnumPocoPOCOWithTheJsonString(string serialized)
    {
        this.deserialized = JsonSerializer.Deserialize<EnumPoco>(serialized, this.jsonSteps.SerializerOptions);
    }

    [Then("the result should have enum values '([^']*)', '([^']*)'")]
    public void ThenTheResultShouldHaveEnumValues(ExampleEnum first, ExampleEnum2 second)
    {
        Assert.AreEqual(first, this.Deserialized.FirstEnumValue, "first");
        Assert.AreEqual(second, this.Deserialized.SecondEnumValue, "second");
    }
}