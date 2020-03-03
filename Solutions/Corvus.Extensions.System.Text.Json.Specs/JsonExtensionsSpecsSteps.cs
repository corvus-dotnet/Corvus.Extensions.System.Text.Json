// <copyright file="JsonExtensionsSpecsSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable CS1591 // Elements should be documented

namespace Corvus.Extensions.Json.Specs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.Extensions.Json.Specs.Samples;
    using Corvus.SpecFlow.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class JsonExtensionsSpecsSteps
    {
        private readonly FeatureContext featureContext;
        private readonly ScenarioContext scenarioContext;

        public JsonExtensionsSpecsSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            this.featureContext = featureContext;
            this.scenarioContext = scenarioContext;
            this.scenarioContext.Set(new PropertyBag(ContainerBindings.GetServiceProvider(featureContext).GetRequiredService<IJsonSerializerOptionsProvider>().Instance));
        }

        [Given(@"I set a property called ""(.*)"" to the value ""(.*)""")]
        public void GivenISetAPropertyCalledToTheValue(string propertyName, string value)
        {
            PropertyBag bag = this.scenarioContext.Get<PropertyBag>();
            using IPropertyBagWriter writer = bag.GetWriter();
            writer.Set(propertyName, value);
        }

        [Given(@"I set a property called ""(.*)"" to null")]
        public void GivenISetAPropertyCalledToNull(string propertyName)
        {
            this.GivenISetAPropertyCalledToTheValue(propertyName, null);
        }

        [Then("the result should be null")]
        public void ThenTheResultShouldBeNull()
        {
            Assert.True(this.scenarioContext.ContainsKey("Result"));
            Assert.AreEqual("(null)", this.scenarioContext.Get<string>("Result"));
        }

        [When(@"I get the property called ""(.*)""")]
        public void WhenIGetThePropertyCalled(string propertyName)
        {
            if (this.scenarioContext.Get<PropertyBag>().TryGet(propertyName, out string value))
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
            if (this.scenarioContext.Get<PropertyBag>().TryGet(propertyName, out PocObject value))
            {
                this.scenarioContext.Set(value, "Result");
            }
            else
            {
                this.scenarioContext.Set("(null)", "Result");
            }
        }

        [Given(@"I deserialize a property bag from the string ""(.*)""")]
        public void GivenIDeserializeAPropertyBagFromTheStringHelloWorldNumber(string json)
        {
            IJsonSerializerOptionsProvider settingsProvider = ContainerBindings.GetServiceProvider(this.featureContext).GetService<IJsonSerializerOptionsProvider>();
            this.scenarioContext.Set(JsonSerializer.Deserialize<PropertyBag>(json, settingsProvider.Instance), "Result");
        }

        [Given("I serialize the property bag")]
        public void GivenISerializeThePropertyBag()
        {
            IJsonSerializerOptionsProvider settingsProvider = ContainerBindings.GetServiceProvider(this.featureContext).GetService<IJsonSerializerOptionsProvider>();

            PropertyBag propertyBag = this.scenarioContext.Get<PropertyBag>();

            this.scenarioContext.Set(JsonSerializer.Serialize(propertyBag, settingsProvider.Instance), "Result");
        }

        [Given(@"I serialize a POCO with ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)""")]
        public void GivenISerializeAPOCOWith(string value, string time, string nullableTime, string culture, ExampleEnum someEnum)
        {
            var poco = new PocObject { SomeCulture = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture), SomeDateTime = DateTimeOffset.Parse(time), SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : (DateTimeOffset?)DateTimeOffset.Parse(nullableTime), SomeEnum = someEnum, SomeValue = value };
            IJsonSerializerOptionsProvider settingsProvider = ContainerBindings.GetServiceProvider(this.featureContext).GetService<IJsonSerializerOptionsProvider>();
            this.scenarioContext.Set(JsonSerializer.Serialize(poco, settingsProvider.Instance), "Result");
        }

        [Given(@"I deserialize a POCO with the json string ""(.*)""")]
        public void GivenIDeserializeAPOCOWithTheJsonString(string json)
        {
            IJsonSerializerOptionsProvider settingsProvider = ContainerBindings.GetServiceProvider(this.featureContext).GetService<IJsonSerializerOptionsProvider>();
            this.scenarioContext.Set(JsonSerializer.Deserialize<PocObject>(json, settingsProvider.Instance), "Result");
        }

        [Then(@"the result should have values ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)""")]
        public void ThenTheResultShouldHaveValues(string value, string time, string nullableTime, string culture, ExampleEnum someEnum)
        {
            PocObject poc = this.scenarioContext.Get<PocObject>("Result");
            var expected = new PocObject { SomeCulture = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture), SomeDateTime = DateTimeOffset.Parse(time), SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : (DateTimeOffset?)DateTimeOffset.Parse(nullableTime), SomeEnum = someEnum, SomeValue = value };
            Assert.AreEqual(expected, poc);
        }

        [Then(@"the result should be ""(.*)""")]
        public void ThenTheResultShouldBe(string expected)
        {
            Assert.True(this.scenarioContext.ContainsKey("Result"));
            Assert.AreEqual(expected, this.scenarioContext.Get<string>("Result"));
        }

        [Given(@"I set a property called ""(.*)"" to the value (.*)")]
        public void GivenISetAPropertyCalledToTheValue(string propertyName, int value)
        {
            PropertyBag bag = this.scenarioContext.Get<PropertyBag>();
            using IPropertyBagWriter writer = bag.GetWriter();
            writer.Set(propertyName, value);
        }

        [When("I cast to a string")]
        public void WhenICastToAstring()
        {
            this.scenarioContext.Set<string>(this.scenarioContext.Get<PropertyBag>(), "Result");
        }

        [Then("the result should be the JSON string")]
        public void ThenTheResultShouldBeTheJson(Table table)
        {
            string expected = CreateJsonFromTable(table);

            string actual = this.scenarioContext.Get<string>("Result");

            Assert.AreEqual(expected, actual);
        }

        [Given("I create a JSON string")]
        public void GivenICreateAJsonString(Table table)
        {
            this.scenarioContext.Set(CreateJsonFromTable(table), "JSON");
        }

        [Then("the result should have the properties")]
        public void ThenTheResultShouldHaveTheProperties(Table table)
        {
            PropertyBag bag = this.scenarioContext.Get<PropertyBag>("Result");

            foreach (TableRow row in table.Rows)
            {
                row.TryGetValue("Property", out string name);
                row.TryGetValue("Value", out string expected);
                row.TryGetValue("Type", out string type);
                switch (type)
                {
                    case "string":
                        {
                            Assert.IsTrue(bag.TryGet(name, out string actual));
                            Assert.AreEqual(expected, actual);
                            break;
                        }

                    case "integer":
                        {
                            Assert.IsTrue(bag.TryGet(name, out int actual));
                            Assert.AreEqual(int.Parse(expected), actual);
                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Unknown data type '{type}'");
                }
            }
        }

        [When("I construct a PropertyBag from the JSON string")]
        public void WhenIConstructAPropertyBagFromTheJsonString()
        {
            this.scenarioContext.Set(new PropertyBag(this.scenarioContext.Get<string>("JSON"), ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<IJsonSerializerOptionsProvider>().Instance), "Result");
        }

        [When("I construct a PropertyBag with no serializer settings")]
        public void WhenIConstructAPropertyBagWithNoSerializerSettings()
        {
            this.scenarioContext.Set(new PropertyBag(), "Result");
        }

        [Given("I create a Dictionary")]
        public void GivenICreateADictionary(Table table)
        {
            this.scenarioContext.Set(CreateDictionaryFromTable(table));
        }

        [When("I construct a PropertyBag from the Dictionary")]
        public void WhenIConstructAPropertyBagFromTheDictionary()
        {
            this.scenarioContext.Set(new PropertyBag(this.scenarioContext.Get<IDictionary<string, object>>(), ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<IJsonSerializerOptionsProvider>().Instance), "Result");
        }

        [When("I construct a PropertyBag from the JSON string with no serializer settings")]
        public void WhenIConstructAPropertyBagFromTheJObjectWithNoSerializerSettings()
        {
            this.scenarioContext.Set(new PropertyBag(this.scenarioContext.Get<string>("JSON")), "Result");
        }

        [When("I construct a PropertyBag from the Dictionary with no serializer settings")]
        public void WhenIConstructAPropertyBagFromTheDictionaryWithNoSerializerSettings()
        {
            this.scenarioContext.Set(new PropertyBag(this.scenarioContext.Get<IDictionary<string, object>>()), "Result");
        }

        [Then("the result should have the default serializer settings")]
        public void ThenTheResultShouldHaveTheDefaultSerializerSettings()
        {
            PropertyBag propertyBag = this.scenarioContext.Get<PropertyBag>("Result");
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.AllowTrailingCommas, propertyBag.SerializerOptions.AllowTrailingCommas);
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.DefaultBufferSize, propertyBag.SerializerOptions.DefaultBufferSize);
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.DictionaryKeyPolicy, propertyBag.SerializerOptions.DictionaryKeyPolicy);
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.Encoder, propertyBag.SerializerOptions.Encoder);
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.IgnoreNullValues, propertyBag.SerializerOptions.IgnoreNullValues);
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.IgnoreReadOnlyProperties, propertyBag.SerializerOptions.IgnoreReadOnlyProperties);
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.MaxDepth, propertyBag.SerializerOptions.MaxDepth);
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.PropertyNameCaseInsensitive, propertyBag.SerializerOptions.PropertyNameCaseInsensitive);
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.PropertyNamingPolicy, propertyBag.SerializerOptions.PropertyNamingPolicy);
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.ReadCommentHandling, propertyBag.SerializerOptions.ReadCommentHandling);
            Assert.AreEqual(PropertyBag.DefaultJsonSerializerOptions.WriteIndented, propertyBag.SerializerOptions.WriteIndented);
            CollectionAssert.AreEquivalent(PropertyBag.DefaultJsonSerializerOptions.Converters, propertyBag.SerializerOptions.Converters);
        }

        private static string CreateJsonFromTable(Table table)
        {
            using var utf8Json = new MemoryStream();
            using var writer = new Utf8JsonWriter(utf8Json);
            writer.WriteStartObject();

            foreach (TableRow row in table.Rows)
            {
                row.TryGetValue("Property", out string name);
                row.TryGetValue("Value", out string value);
                row.TryGetValue("Type", out string type);
                writer.WritePropertyName(name);
                switch (type)
                {
                    case "string":
                        writer.WriteStringValue(value == "(null)" ? null : value);
                        break;
                    case "integer":
                        writer.WriteNumberValue(int.Parse(value));
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown data type '{type}'");
                }
            }

            writer.WriteEndObject();

            writer.Flush();
            utf8Json.Position = 0;
            using var reader = new StreamReader(utf8Json);
            return reader.ReadToEnd();
        }

        private static IDictionary<string, object> CreateDictionaryFromTable(Table table)
        {
            var expected = new Dictionary<string, object>();
            foreach (TableRow row in table.Rows)
            {
                row.TryGetValue("Property", out string name);
                row.TryGetValue("Value", out string value);
                row.TryGetValue("Type", out string type);
                expected[name] = type switch
                {
                    "string" => value == "(null)" ? null : value,
                    "integer" => int.Parse(value),
                    _ => throw new InvalidOperationException($"Unknown data type '{type}'"),
                };
            }

            return expected;
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Elements should be documented