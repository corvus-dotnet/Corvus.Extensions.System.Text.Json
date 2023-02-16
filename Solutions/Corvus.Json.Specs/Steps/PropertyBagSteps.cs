// <copyright file="PropertyBagSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.Specs.Steps;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Corvus.Json.PropertyBag;
using Corvus.Json.Specs.Samples;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

[Binding]
public class PropertyBagSteps
{
    private readonly Dictionary<string, object> creationProperties = new();
    private readonly Dictionary<string, IPropertyBag> namedPropertyBags = new();
    private readonly JsonExtensionsSpecsSteps jsonSteps;

    private IPropertyBagFactory? propertyBagFactory;
    private IJsonPropertyBagFactory? jPropertyBagFactory;
    private IPropertyBag? propertyBag;

    public PropertyBagSteps(
        JsonExtensionsSpecsSteps jsonSteps)
    {
        this.jsonSteps = jsonSteps;

        jsonSteps.Services
            .AddJsonPropertyBagFactory()
            .AddSingleton<JsonConverter>(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    private IPropertyBagFactory PropertyBagFactory => this.propertyBagFactory ??= this.jsonSteps.ServiceProvider.GetRequiredService<IPropertyBagFactory>();

    private IJsonPropertyBagFactory JPropertyBagFactory => this.jPropertyBagFactory ??=
        this.jsonSteps.ServiceProvider.GetRequiredService<IJsonPropertyBagFactory>();

    private IPropertyBag Bag => this.propertyBag ?? throw new InvalidOperationException("The test is trying to use property bag before it has been created");

    [Given("I deserialize a property bag from the string (.*)")]
    [Given("I deserialize a property bag from the string")]
    public void GivenIDeserializeAPropertyBagFromTheString(string json)
    {
        this.propertyBag = JsonSerializer.Deserialize<IPropertyBag>(json, this.jsonSteps.SerializerOptions);
    }

    [Given("the creation properties include '([^']*)' with the value '([^']*)'")]
    public void TheCreationPropertiesInclude(string propertyName, string value)
    {
        this.creationProperties.Add(propertyName, value);
    }

    [Given("the creation properties include '([^']*)' with a null value")]
    public void TheCreationPropertiesInclude(string propertyName)
    {
        this.creationProperties.Add(propertyName, null!);   // TODO - do we need to modify IPropertyBagFactory to allow null values?
    }

    [Given(@"the creation properties include '([^']*)' with the value ([\d]*)")]
    public void TheCreationPropertiesInclude(string propertyName, int value)
    {
        this.creationProperties.Add(propertyName, value);
    }

    [Given("the creation properties include '([^']*)' with the floating point value (.*)")]
    public void TheCreationPropertiesInclude(string propertyName, double value)
    {
        this.creationProperties.Add(propertyName, value);
    }

    [Given("the creation properties include '([^']*)' with the date value '([^']*)'")]
    public void TheCreationPropertiesInclude(string propertyName, DateTimeOffset value)
    {
        this.creationProperties.Add(propertyName, value);
    }

    [Given("the creation properties include a DateTime POCO called '([^']*)' with '([^']*)' '([^']*)'")]
    public void GivenTheCreationPropertiesIncludeADateTimePOCOCalledWith(string name, string time, string nullableTime)
    {
        DateTimeOffsetPocObject poco = MakeDateTimeOffsetPoco(time, nullableTime);

        this.creationProperties.Add(name, poco);
    }

    [Given("the creation properties include a CultureInfo POCO called '([^']*)' with '([^']*)'")]
    public void GivenTheCreationPropertiesIncludeACultureInfoPOCOCalledWith(string name, string culture)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            CultureInfoPocObject poco = MakeCultureInfoPoco(culture);

            this.creationProperties.Add(name, poco);
        }
    }

    [Given("the creation properties include an enum value called '([^']*)' with value '([^']*)'")]
    public void GivenTheCreationPropertiesIncludeAnEnumValueCalledWithValue(string name, ExampleEnum value)
    {
        this.creationProperties.Add(name, value);
    }

    [When("I get the property called '([^']*)' as an IPropertyBag and call it '([^']*)'")]
    public void WhenIGetThePropertyCalledAsAnIPropertyBagAndCallIt(string propertyName, string name)
    {
        Assert.IsTrue(this.Bag.TryGet(propertyName, out IPropertyBag nestedBag));
        this.namedPropertyBags.Add(name, nestedBag);
    }

    [When("I serialize the property bag")]
    public void GivenISerializeThePropertyBag()
    {
        this.jsonSteps.SerializedJson = JsonSerializer.Serialize(this.Bag, this.jsonSteps.SerializerOptions);
    }

    [When("I deserialize the serialized property bag")]
    public void WhenIDeserializeTheSerializedPropertyBag()
    {
        this.propertyBag = JsonSerializer.Deserialize<IPropertyBag>(this.jsonSteps.SerializedJson, this.jsonSteps.SerializerOptions);
    }

    [Then("the result should have a DateTime POCO named '([^']*)' with values '([^']*)' '([^']*)'")]
    public void ThenTheResultShouldHaveADateTimePOCONamedWithValues(string name, string time, string nullableTime)
    {
        Assert.IsTrue(this.Bag.TryGet(name, out DateTimeOffsetPocObject poc), "TryGet return value");
        CheckPocosAreEqual(time, nullableTime, poc);
    }

    [Then("the result should have a CultureInfo POCO named '([^']*)' with value '([^']*)'")]
    public void ThenTheResultShouldHaveACultureInfoPocoNamed(string name, string culture)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            Assert.IsTrue(this.Bag.TryGet(name, out CultureInfoPocObject poc), "TryGet return value");
            CheckPocosAreEqual(culture, poc);
        }
        else
        {
            Assert.IsFalse(this.Bag.TryGet(name, out CultureInfoPocObject _), "TryGet return value");
        }
    }

    [Then("the result should have an enum value named '([^']*)' with value '([^']*)'")]
    public void ThenTheResultShouldHaveAnEnumValueNamedWithValue(string name, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Assert.IsTrue(this.Bag.TryGet(name, out ExampleEnum actual), "TryGet return value");
            Assert.AreEqual(value, actual.ToString());
        }
        else
        {
            Assert.IsFalse(this.Bag.TryGet(name, out ExampleEnum _), "TryGet return value");
        }
    }

    [Given("I create the property bag from the creation properties")]
    [When("I create the property bag from the creation properties")]
    public void WhenICreateThePropertyBagFromTheCreationProperties()
    {
        this.propertyBag = this.PropertyBagFactory.Create(this.creationProperties);
    }

    [When("I create a PropertyBag from the UTF8 JSON string")]
    public void WhenICreateAPropertyBagFromTheUTFJSONString()
    {
        this.propertyBag = this.JPropertyBagFactory.Create(this.jsonSteps.Utf8Json.AsMemory());
    }

    [When("I create a PropertyBag from JsonElement")]
    public void WhenICreateAPropertyBagFromJsonElement()
    {
        this.propertyBag = this.JPropertyBagFactory.Create(this.jsonSteps.JsonDocument.RootElement);
    }

    [When("I create a PropertyBag via a Utf8JsonWriter")]
    public void WhenICreateAPropertyBagViaAUtfWriter()
    {
        this.propertyBag = this.JPropertyBagFactory.Create(this.jsonSteps.WritePropertiesFromDeferredTableToUtf8JsonWriter);
    }

    [When("I create a PropertyBag via a Utf8JsonWriter with a context argument")]
    public void WhenICreateAPropertyBagViaAUtfWriterWithAContextArgument()
    {
        this.propertyBag = this.JPropertyBagFactory.Create(
            this.jsonSteps.Utf8CreateContext,
            (context, writer) =>
            {
                this.jsonSteps.WritePropertiesFromDeferredTableToUtf8JsonWriter(writer);
                this.jsonSteps.Utf8CreateContextReceived = context;
            });
    }

    [When("I get the property bag as a JsonElement")]
    public void WhenIGetPropertyBagAsJsonElement()
    {
        this.jsonSteps.JsonElement = this.JPropertyBagFactory.AsJsonElement(this.Bag);
    }

    [When("I get the property bag's JSON and write it to a JsonElement")]
    public void WhenIGetPropertyBagsJSON()
    {
        MemoryStream ms = new();
        using (Utf8JsonWriter w = new(ms))
        {
            this.JPropertyBagFactory.WriteTo(this.Bag, w);
        }

        this.jsonSteps.JsonElement = this.JPropertyBagFactory.AsJsonElement(this.Bag);
    }

    [When("I add, modify, or remove properties")]
    public void WhenIAddModifyOrRemoveProperties(Table table)
    {
        var propertiesToRemove = new List<string>();
        var propertiesToSetOrAdd = new Dictionary<string, object>();
        foreach ((string propertyName, string value, string type, string action) in table.CreateSet<(string Property, string Value, string Type, string Action)>())
        {
            switch (action)
            {
                case "remove":
                    propertiesToRemove.Add(propertyName);
                    break;

                case "addOrSet":
                    object actualValue = type switch
                    {
                        "string" => value,
                        "integer" => int.Parse(value),
                        _ => throw new InvalidOperationException($"Unknown data type '{type}'"),
                    };
                    propertiesToSetOrAdd.Add(propertyName, actualValue);
                    break;

                default:
                    Assert.Fail($"Unknown action in add/modify/remove table: {action}");
                    break;
            }
        }

        this.propertyBag = this.PropertyBagFactory.CreateModified(
            this.Bag,
            propertiesToSetOrAdd.Count == 0 ? null : propertiesToSetOrAdd,
            propertiesToRemove.Count == 0 ? null : propertiesToRemove);
    }

    [When("I get the property called '([^']*)' as a custom object expecting an exception")]
    public void WhenIGetThePropertyCalledAsACustomObjectExpectingAnException(string propertyName)
    {
        try
        {
            this.Bag.TryGet(propertyName, out CultureInfoPocObject? _);
        }
        catch (SerializationException x)
        {
            this.jsonSteps.SerializationException = x;
        }
    }

    [Then("the property bag should contain a property called '([^']*)' with the value '([^']*)'")]
    public void ThenThePropertyBagShouldContainAPropertyCalledWithTheValue(string key, string expectedValue)
    {
        Assert.True(this.Bag.TryGet(key, out string value), "TryGet return value");
        Assert.AreEqual(expectedValue, value);
    }

    [Then("calling TryGet with '([^']*)' should return false and the result should be null")]
    public void ThenCallingTryGetWithShouldReturnFalseAndTheResultShouldBeNull(string key)
    {
        Assert.False(this.Bag.TryGet(key, out string? value), "TryGet return value");
        Assert.IsNull(value);
    }

    [Then("the IPropertyBag should have the properties")]
    public void ThenTheIPropertyBagShouldHaveTheProperties(Table table)
    {
        AssertPropertyBagHasProperties(this.propertyBag!, table);
    }

    [Then("the IPropertyBag should not have the properties")]
    public void ThenTheResultShouldNotHaveTheProperties(Table table)
    {
        AssertPropertyBagDoesNotHaveProperties(this.Bag, table);
    }

    [Then("the IPropertyBag called '([^']*)' should have the properties")]
    public void ThenTheIPropertyBagCalledShouldHaveTheProperties(string name, Table table)
    {
        IPropertyBag nestedBag = this.namedPropertyBags[name];
        Assert.IsNotNull(nestedBag);
        AssertPropertyBagHasProperties(nestedBag, table);
    }

    private static void AssertPropertyBagHasProperties(IPropertyBag bag, Table table)
    {
        foreach ((string name, string expected, string type) in table.CreateSet<(string Property, string Value, string Type)>())
        {
            object? actualAsObject;
            void Test<T>(IPropertyBag bag, string name)
            {
                Assert.IsTrue(bag.TryGet(name, out T actual));
                actualAsObject = actual;
            }

            switch (type)
            {
                case "string":
                    Test<string?>(bag, name);
                    break;

                case "integer":
                    Test<int>(bag, name);
                    break;

                case "fp":
                    Test<double>(bag, name);
                    break;

                case "boolean":
                    Test<bool>(bag, name);
                    break;

                case "datetime":
                    Test<DateTimeOffset>(bag, name);
                    break;

                case "IPropertyBag":
                    Test<IPropertyBag>(bag, name);
                    break;

                case "object[]":
                    Test<object[]>(bag, name);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown data type '{type}'");
            }

            AssertChildValueIs(expected, type, actualAsObject);
        }
    }

    private static void AssertPropertyBagDoesNotHaveProperties(IPropertyBag bag, Table table)
    {
        foreach (string name in table.CreateSet<string>())
        {
            Assert.IsFalse(bag.TryGet(name, out object? _));
        }
    }

    private static void AssertChildValueIs(string expected, string type, object? actual)
    {
        switch (type)
        {
            case "string":
                Assert.AreEqual(expected, actual);
                break;

            case "integer":
                Assert.AreEqual(int.Parse(expected), actual);
                break;

            case "fp":
                Assert.AreEqual(double.Parse(expected), actual);
                break;

            case "boolean":
                Assert.AreEqual(bool.Parse(expected), actual);
                break;

            case "datetime":
                Assert.AreEqual(DateTimeOffset.Parse(expected), actual);
                break;

            case "IPropertyBag":
                Assert.IsInstanceOf<IPropertyBag>(actual);
                break;

            case "object[]":
                Assert.IsInstanceOf<object[]>(actual);
                break;

            case "IReadOnlyDictionary<string, object>":
                Assert.IsInstanceOf<IReadOnlyDictionary<string, object>>(actual);
                break;

            case "null":
                Assert.IsNull(actual);
                break;

            default:
                throw new InvalidOperationException($"Unknown data type '{type}'");
        }
    }

    private static void CheckPocosAreEqual(string culture, CultureInfoPocObject poc)
    {
        var expected = new CultureInfoPocObject
        {
            SomeCulture = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture),
        };

        Assert.AreEqual(expected, poc);
    }

    private static void CheckPocosAreEqual(string time, string nullableTime, DateTimeOffsetPocObject poc)
    {
        var expected = new DateTimeOffsetPocObject()
        {
            SomeDateTime = DateTimeOffset.Parse(time),
            SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : DateTimeOffset.Parse(nullableTime),
        };

        Assert.AreEqual(expected, poc);
    }

    private static CultureInfoPocObject MakeCultureInfoPoco(string? culture)
    {
        return new CultureInfoPocObject
        {
            SomeCulture = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture),
        };
    }

    private static DateTimeOffsetPocObject MakeDateTimeOffsetPoco(string time, string nullableTime)
    {
        return new DateTimeOffsetPocObject()
        {
            SomeDateTime = DateTimeOffset.Parse(time),
            SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : DateTimeOffset.Parse(nullableTime),
        };
    }
}