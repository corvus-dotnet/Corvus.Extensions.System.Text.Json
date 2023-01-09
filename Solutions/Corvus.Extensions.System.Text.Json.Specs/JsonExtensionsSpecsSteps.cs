// <copyright file="JsonExtensionsSpecsSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Extensions.Json.Specs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;

    using Corvus.Extensions.Json.Specs.Samples;
    using Corvus.Json;
    using Corvus.Testing.SpecFlow;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Assist;

    [Binding]
    public class JsonExtensionsSpecsSteps
    {
        private readonly ScenarioContext scenarioContext;
        private readonly IPropertyBagFactory propertyBagFactory;
        private readonly IJsonPropertyBagFactory jPropertyBagFactory;
        private readonly IServiceProvider serviceProvider;

        private readonly Dictionary<string, object> creationProperties = new();
        private readonly Dictionary<string, IPropertyBag> namedPropertyBags = new();
        private IPropertyBag? propertyBag;
        private JsonDocument? jsonDocument;
        private string? serializedJson;
        private SerializationException? exception;
        private byte[]? utf8Json;
        private PocObject? poco;

        public JsonExtensionsSpecsSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
            this.serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
            this.propertyBagFactory = this.serviceProvider.GetRequiredService<IPropertyBagFactory>();
            this.jPropertyBagFactory = this.serviceProvider.GetRequiredService<IJsonPropertyBagFactory>();
        }

        private IPropertyBag Bag => this.propertyBag ?? throw new InvalidOperationException("The test is trying to use property bag before it has been created");

        [Given(@"the creation properties include ""(.*)"" with the value ""([^""]*)""")]
        public void TheCreationPropertiesInclude(string propertyName, string value)
        {
            this.creationProperties.Add(propertyName, value);
        }

        [Given(@"the creation properties include ""(.*)"" with a null value")]
        public void TheCreationPropertiesInclude(string propertyName)
        {
            this.creationProperties.Add(propertyName, null!);   // TODO - do we need to modify IPropertyBagFactory to allow null values?
        }

        [Given(@"the creation properties include ""(.*)"" with the value ([\d]*)")]
        public void TheCreationPropertiesInclude(string propertyName, int value)
        {
            this.creationProperties.Add(propertyName, value);
        }

        [Given(@"the creation properties include ""(.*)"" with the floating point value (.*)")]
        public void TheCreationPropertiesInclude(string propertyName, double value)
        {
            this.creationProperties.Add(propertyName, value);
        }

        [Given(@"the creation properties include ""(.*)"" with the date value ""(.*)""")]
        public void TheCreationPropertiesInclude(string propertyName, DateTimeOffset value)
        {
            this.creationProperties.Add(propertyName, value);
        }

        [Given(@"the creation properties include a DateTime POCO called ""(.*)"" with ""(.*)"" ""(.*)""")]
        public void GivenTheCreationPropertiesIncludeADateTimePOCOCalledWith(string name, string time, string nullableTime)
        {
            DateTimeOffsetPocObject poco = MakeDateTimeOffsetPoco(time, nullableTime);

            this.creationProperties.Add(name, poco);
        }

        [Given(@"the creation properties include a CultureInfo POCO called ""(.*)"" with ""(.*)""")]
        public void GivenTheCreationPropertiesIncludeACultureInfoPOCOCalledWith(string name, string culture)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfoPocObject poco = MakeCultureInfoPoco(culture);

                this.creationProperties.Add(name, poco);
            }
        }

        [Given(@"the creation properties include an enum value called ""(.*)"" with value ""(.*)""")]
        public void GivenTheCreationPropertiesIncludeAnEnumValueCalledWithValue(string name, ExampleEnum value)
        {
            this.creationProperties.Add(name, value);
        }

        [Given("I deserialize a property bag from the string (.*)")]
        [Given("I deserialize a property bag from the string")]
        public void GivenIDeserializeAPropertyBagFromTheString(string json)
        {
            IJsonSerializerOptionsProvider settingsProvider = this.serviceProvider.GetRequiredService<IJsonSerializerOptionsProvider>();
            this.propertyBag = JsonSerializer.Deserialize<IPropertyBag>(json, settingsProvider.Instance);
        }

        [Given("I create JSON UTF8 with these properties")]
        public void GivenICreateAJSONString(Table table)
        {
            using var utf8Json = new MemoryStream();
            using var writer = new Utf8JsonWriter(utf8Json);
            writer.WriteStartObject();

            JsonObject jo = new();
            foreach ((string Property, string Value, string Type) row in table.CreateSet<(string Property, string Value, string Type)>())
            {
                writer.WritePropertyName(row.Property);
                switch (row.Type)
                {
                    case "string":
                        writer.WriteStringValue(row.Value == "(null)" ? null : row.Value);
                        break;
                    case "integer":
                        writer.WriteNumberValue(int.Parse(row.Value));
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown data type '{row.Type}'");
                }
            }

            writer.WriteEndObject();
            writer.Flush();

            this.utf8Json = utf8Json.ToArray();
        }

        [Then("the result should be null")]
        public void ThenTheResultShouldBeNull()
        {
            Assert.True(this.scenarioContext.ContainsKey("Result"));
            Assert.AreEqual("(null)", this.scenarioContext.Get<string>("Result"));
        }

        [When(@"I get the property called ""([\""]*)""")]
        public void WhenIGetThePropertyCalled(string propertyName)
        {
            if (this.Bag.TryGet(propertyName, out string value))
            {
                this.scenarioContext.Set(value ?? "(null)", "Result");
            }
            else
            {
                this.scenarioContext.Set("(null)", "Result");
            }
        }

        [When(@"I get the property called ""(.*)"" as a custom object")]
        public void WhenIGetThePropertyCalledAsACustomObject(string propertyName)
        {
            if (this.Bag.TryGet(propertyName, out PocObject value))
            {
                this.scenarioContext.Set(value, "Result");
            }
            else
            {
                this.scenarioContext.Set("(null)", "Result");
            }
        }

        [When(@"I get the property called ""(.*)"" as an IPropertyBag and call it ""(.*)""")]
        public void WhenIGetThePropertyCalledAsAnIPropertyBagAndCallIt(string propertyName, string name)
        {
            Assert.IsTrue(this.Bag.TryGet(propertyName, out IPropertyBag nestedBag));
            this.namedPropertyBags.Add(name, nestedBag);
        }

        [When("I serialize the property bag")]
        public void GivenISerializeThePropertyBag()
        {
            IJsonSerializerOptionsProvider jsonSerializerOptionsProvider = this.serviceProvider.GetRequiredService<IJsonSerializerOptionsProvider>();

            this.serializedJson = JsonSerializer.Serialize(this.Bag, jsonSerializerOptionsProvider.Instance);
        }

        [When("I deserialize the serialized property bag")]
        public void WhenIDeserializeTheSerializedPropertyBag()
        {
            IJsonSerializerOptionsProvider jsonSerializerOptionsProvider = this.serviceProvider.GetRequiredService<IJsonSerializerOptionsProvider>();
            this.propertyBag = JsonSerializer.Deserialize<IPropertyBag>(this.serializedJson!, jsonSerializerOptionsProvider.Instance);
        }

        [Given(@"I serialize a POCO with ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)""")]
        public void GivenISerializeAPOCOWith(string value, string time, string nullableTime, string culture, ExampleEnum someEnum)
        {
            var poco = new PocObject { SomeCulture = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture), SomeDateTime = DateTimeOffset.Parse(time), SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : (DateTimeOffset?)DateTimeOffset.Parse(nullableTime), SomeEnum = someEnum, SomeValue = value };
            IJsonSerializerOptionsProvider jsonSerializerOptionsProvider = this.serviceProvider.GetRequiredService<IJsonSerializerOptionsProvider>();
            this.serializedJson = JsonSerializer.Serialize(poco, jsonSerializerOptionsProvider.Instance);
        }

        [Given(@"I serialize a POCO with ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)"" with the DateTimeOffsetToIso8601AndUnixTimeConverter")]
        public void GivenISerializeAPOCOWithOddDateTimeOffsetConverter(string value, string time, string nullableTime, string culture, ExampleEnum someEnum)
        {
            IJsonSerializerOptionsProvider jsonSerializerOptionsProvider = GetSerializerOptionsWithOddDateTimeOffsetHandling();

            var poco = new PocObject { SomeCulture = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture), SomeDateTime = DateTimeOffset.Parse(time), SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : (DateTimeOffset?)DateTimeOffset.Parse(nullableTime), SomeEnum = someEnum, SomeValue = value };
            this.serializedJson = JsonSerializer.Serialize(poco, jsonSerializerOptionsProvider.Instance);
        }

        [Given(@"I deserialize a POCO with the json string ""(.*)""")]
        public void GivenIDeserializeAPOCOWithTheJsonString(string json)
        {
            IJsonSerializerOptionsProvider jsonSerializerOptionsProvider = this.serviceProvider.GetRequiredService<IJsonSerializerOptionsProvider>();
            this.poco = JsonSerializer.Deserialize<PocObject>(json, jsonSerializerOptionsProvider.Instance);
        }

        [Given(@"I deserialize a POCO with the json string ""(.*?)"" with the DateTimeOffsetToIso8601AndUnixTimeConverter")]
        public void GivenIDeserializeAPOCOWithTheJsonStringWithOddDateTimeOffsetHandling(string json)
        {
            IJsonSerializerOptionsProvider jsonSerializerOptionsProvider = GetSerializerOptionsWithOddDateTimeOffsetHandling();
            this.poco = JsonSerializer.Deserialize<PocObject>(json, jsonSerializerOptionsProvider.Instance);
        }

        [Then(@"the result should have values ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)""")]
        public void ThenTheResultShouldHaveValues(string value, string time, string nullableTime, string culture, ExampleEnum someEnum)
        {
            var expected = new PocObject { SomeCulture = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture), SomeDateTime = DateTimeOffset.Parse(time), SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : (DateTimeOffset?)DateTimeOffset.Parse(nullableTime), SomeEnum = someEnum, SomeValue = value };
            Assert.AreEqual(expected, this.poco);
        }

        [Then(@"the result should have a DateTime POCO named ""(.*)"" with values ""(.*)"" ""(.*)""")]
        public void ThenTheResultShouldHaveADateTimePOCONamedWithValues(string name, string time, string nullableTime)
        {
            Assert.IsTrue(this.Bag.TryGet(name, out DateTimeOffsetPocObject poc), "TryGet return value");
            CheckPocosAreEqual(time, nullableTime, poc);
        }

        [Then(@"the result should have a CultureInfo POCO named ""(.*)"" with value ""(.*)""")]
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

        [Then(@"the result should have an enum value named ""(.*)"" with value ""(.*)""")]
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

        [Then(@"the result should be ""(.*)""")]
        [Then("the result should be")] // For multi-line inputs (which have less intrusive escaping for JSON
        public void ThenTheResultShouldBe(string expected)
        {
            Assert.AreEqual(expected, this.serializedJson);
        }

        [Given("I create the property bag from the creation properties")]
        [When("I create the property bag from the creation properties")]
        public void WhenICreateThePropertyBagFromTheCreationProperties()
        {
            this.propertyBag = this.propertyBagFactory.Create(this.creationProperties);
        }

        [When("I create a PropertyBag from the UTF8 JSON string")]
        public void WhenICreateAPropertyBagFromTheUTFJSONString()
        {
            this.propertyBag = this.jPropertyBagFactory.Create(this.utf8Json.AsMemory());
        }

        [When(@"I get the property called ""(.*)"" as a custom object expecting an exception")]
        public void WhenIGetThePropertyCalledAsACustomObjectExpectingAnException(string propertyName)
        {
            try
            {
                this.Bag.TryGet(propertyName, out CultureInfoPocObject? _);
            }
            catch (SerializationException x)
            {
                this.exception = x;
            }
        }

        [When("I get the property bag as a JsonDocument")]
        public void WhenIGetPropertyBagAsJsonDocument()
        {
            this.jsonDocument = this.jPropertyBagFactory.AsJsonDocument(this.Bag);
        }

        [When("I get the property bag's JSON and write it to a JsonDocument")]
        public void WhenIGetPropertyBagsJSON()
        {
            MemoryStream ms = new();
            using (Utf8JsonWriter w = new(ms))
            {
                this.jPropertyBagFactory.WriteTo(this.Bag, w);
            }

            this.jsonDocument = this.jPropertyBagFactory.AsJsonDocument(this.Bag);
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

            this.propertyBag = this.propertyBagFactory.CreateModified(
                this.Bag,
                propertiesToSetOrAdd.Count == 0 ? null : propertiesToSetOrAdd,
                propertiesToRemove.Count == 0 ? null : propertiesToRemove);
        }

        [Then(@"the property bag should contain a property called ""([^""]*)"" with the value ""([^""]*)""")]
        public void ThenThePropertyBagShouldContainAPropertyCalledWithTheValue(string key, string expectedValue)
        {
            Assert.True(this.Bag.TryGet(key, out string value), "TryGet return value");
            Assert.AreEqual(expectedValue, value);
        }

        [Then(@"calling TryGet with ""(.*)"" should return false and the result should be null")]
        public void ThenCallingTryGetWithShouldReturnFalseAndTheResultShouldBeNull(string key)
        {
            Assert.False(this.Bag.TryGet(key, out string? value), "TryGet return value");
            Assert.IsNull(value);
        }

        [Then("TryGet should have thrown a SerializationException")]
        public void ThenTryGetShouldHaveThrownASerializationException()
        {
            Assert.IsInstanceOf<SerializationException>(this.exception);
        }

        [Then("the JsonDocument should have these properties")]
        public void ThenTheJsonDocumentShouldHaveTheseProperties(Table table)
        {
            foreach ((string Property, string Value, string Type) row in table.CreateSet<(string Property, string Value, string Type)>())
            {
                Assert.IsTrue(this.jsonDocument!.RootElement.TryGetProperty(row.Property, out JsonElement property));
                (JsonValueKind expectedKind, object expectedValue, object actualValue) = row.Type switch
                {
                    "string" => (JsonValueKind.String, (object)row.Value, (object)property.GetString()!),
                    "integer" => (JsonValueKind.Number, int.Parse(row.Value), property.GetInt32()),
                    "fp" => (JsonValueKind.Number, double.Parse(row.Value), property.GetDouble()),
                    _ => throw new InvalidOperationException($"Unknown type: {row.Type}"),
                };
                Assert.AreEqual(expectedKind, property.ValueKind);
                Assert.AreEqual(expectedValue, actualValue);
            }
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

        [Then(@"the IPropertyBag called ""(.*)"" should have the properties")]
        public void ThenTheIPropertyBagCalledShouldHaveTheProperties(string name, Table table)
        {
            IPropertyBag nestedBag = this.namedPropertyBags[name];
            Assert.IsNotNull(nestedBag);
            AssertPropertyBagHasProperties(nestedBag, table);
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
                SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : (DateTimeOffset?)DateTimeOffset.Parse(nullableTime),
            };
        }

        private static IJsonSerializerOptionsProvider GetSerializerOptionsWithOddDateTimeOffsetHandling()
        {
            IServiceCollection services = new ServiceCollection()
                .AddJsonSerializerSettingsProvider()
                .AddJsonPropertyBagFactory()
                .AddJsonCultureInfoConverter()
                .AddJsonNetDateTimeOffsetToIso8601AndUnixTimeConverter()
                .AddSingleton<JsonConverter>(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IJsonSerializerOptionsProvider>();
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
            foreach ((string name, _, _) in table.CreateSet<(string Property, string Value, string Type)>())
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
                SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : (DateTimeOffset?)DateTimeOffset.Parse(nullableTime),
            };

            Assert.AreEqual(expected, poc);
        }
    }
}