// <copyright file="JsonExtensionsSpecsSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.Specs;

using System;
using System.Globalization;
using System.IO;
using System.Text.Json;

using Corvus.Json;
using Corvus.Json.Serialization;
using Corvus.Json.Specs.Samples;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

[Binding]
public class JsonExtensionsSpecsSteps
{
    private readonly ServiceCollection services = new();

    private ServiceProvider? serviceProvider;

    private byte[]? utf8Json;
    private object? utf8CreateContext;
    private Table? tableForDeferredUtf8Write;

    private JsonDocument? jsonDocument;
    private JsonElement? jsonElement;

    private string? serializedJson;
    private PocObject? poco;

    public JsonExtensionsSpecsSteps()
    {
        this.Services
            .AddJsonSerializerOptionsProvider()
            .AddJsonCultureInfoConverter();
    }

    /// <summary>
    /// Gets service collection for JSON serialization tests.
    /// </summary>
    /// <remarks>
    /// Some tests want to specify exactly what's going into DI, so we're not using the
    /// per-scenario DI container in these tests.
    /// </remarks>
    public ServiceCollection Services => this.serviceProvider is null
        ? this.services
        : throw new InvalidOperationException("EnumCasingSteps service provider has already been built, so test cannot add further services");

    public IServiceProvider ServiceProvider => this.serviceProvider ??= this.Services.BuildServiceProvider();

    public IJsonSerializerOptionsProvider SerializerOptionsProvider => this.ServiceProvider.GetRequiredService<IJsonSerializerOptionsProvider>();

    public JsonSerializerOptions SerializerOptions => this.SerializerOptionsProvider.Instance;

    public JsonDocument JsonDocument => this.jsonDocument ?? throw new InvalidOperationException("No JsonDocument has been created");

    public object Utf8CreateContext => this.utf8CreateContext ??= new();

    public object? Utf8CreateContextReceived { get; set; }

    public byte[] Utf8Json => this.utf8Json ?? throw new InvalidOperationException("Test is trying to read Utf8Json without having populated it");

    public SerializationException? SerializationException { get; set; }

    public string SerializedJson
    {
        get => this.serializedJson ?? throw new InvalidOperationException("This test has not serialized any JSON");
        set
        {
            if (this.serializedJson is not null)
            {
                throw new InvalidOperationException("Test has tried to deserialize JSON twice");
            }

            this.serializedJson = value;
        }
    }

    public JsonElement JsonElement
    {
        get => this.jsonElement ?? throw new InvalidOperationException("The test is trying to use a JsonElement before retrieving it");
        set
        {
            if (this.jsonElement is not null)
            {
                throw new InvalidOperationException("Test has tried to set a JsonElement twice");
            }

            this.jsonElement = value;
        }
    }

    [Given("I create JSON UTF8 with these properties")]
    public void GivenICreateFromAJSONString(Table table)
    {
        using var utf8Json = new MemoryStream();
        using var writer = new Utf8JsonWriter(utf8Json);

        WritePropertiesToUtf8JsonWriter(table, writer);
        writer.Flush();

        this.utf8Json = utf8Json.ToArray();
    }

    [Given("I create a JsonElement with these properties")]
    public void GivenICreateAJsonElementWithTheseProperties(Table table)
    {
        this.GivenICreateFromAJSONString(table);
        this.jsonDocument = JsonDocument.Parse(this.utf8Json);
    }

    [Given("I will write these properties to the Utf8JsonWriter")]
    public void GivenIWillWriteThesePropertiesToTheUtfWriter(Table table)
    {
        this.tableForDeferredUtf8Write = table;
    }

    [Given("I deserialize a POCO with the json string '([^']*)'")]
    public void GivenIDeserializeAPOCOWithTheJsonString(string json)
    {
        this.poco = JsonSerializer.Deserialize<PocObject>(json, this.SerializerOptions);
    }

    [Then("the deserialized POCO should have values '([^']*)', '([^']*)', '([^']*)', '([^']*)', '([^']*)'")]
    public void ThenTheResultShouldHaveValues(string value, string time, string nullableTime, string culture, ExampleEnum someEnum)
    {
        var expected = new PocObject { SomeCulture = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture), SomeDateTime = DateTimeOffset.Parse(time), SomeNullableDateTime = string.IsNullOrEmpty(nullableTime) ? null : DateTimeOffset.Parse(nullableTime), SomeEnum = someEnum, SomeValue = value };
        Assert.AreEqual(expected, this.poco);
    }

    [Then("the serialized JSON text should be '([^']*)'")]
    [Then("the result should be")] // For multi-line inputs (which have less intrusive escaping for JSON
    public void ThenTheResultShouldBe(string expected)
    {
        Assert.AreEqual(expected, this.serializedJson);
    }

    [Then("the context argument passed to the Utf8JsonWriter callback should be the same as was passed to Create")]
    public void ThenTheContextArgumentPassedToTheUtfWriterCallbackShouldBeTheSameAsWasPassedToCreate()
    {
        Assert.AreSame(this.utf8CreateContext, this.Utf8CreateContextReceived);
    }

    [Then("TryGet should have thrown a SerializationException")]
    public void ThenTryGetShouldHaveThrownASerializationException()
    {
        Assert.IsInstanceOf<SerializationException>(this.SerializationException);
    }

    [Then("the JsonElement should have these properties")]
    public void ThenTheJsonElementShouldHaveTheseProperties(Table table)
    {
        foreach ((string Property, string Value, string Type) row in table.CreateSet<(string Property, string Value, string Type)>())
        {
            Assert.IsTrue(
                this.JsonElement.TryGetProperty(row.Property, out JsonElement property),
                $"Getting property {row.Property}");
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

    public void WritePropertiesFromDeferredTableToUtf8JsonWriter(Utf8JsonWriter writer)
    {
        WritePropertiesToUtf8JsonWriter(this.tableForDeferredUtf8Write!, writer);
    }

    public static void WritePropertiesToUtf8JsonWriter(Table table, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

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
    }
}