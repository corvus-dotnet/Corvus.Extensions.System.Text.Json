// <copyright file="PropertyBagBenchmarkBase.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.PropertyBag.Benchmarks;

using System;
using System.Text.Json;

using Corvus.Json.PropertyBag.Internal;
using Corvus.Json.Serialization;

using Microsoft.Extensions.DependencyInjection;

public abstract class PropertyBagBenchmarkBase : IDisposable
{
    private static readonly byte[] Source = """
        {
            "intProperty": 42,
            "objectProperty": {
            "intValue": 99,
            "int64Value": 3000000000000,
            "boolValue": false,
            "stringValue": "Hello, world"
            }
        }
        """u8.ToArray();

    private readonly IJsonPropertyBagFactory propertyBagFactory;
    private readonly JsonDocument jsonDocument;

    protected PropertyBagBenchmarkBase()
    {
        IServiceProvider sp = new ServiceCollection()
            .AddJsonSerializerOptionsProvider()
            .AddJsonPropertyBagFactory()
            .BuildServiceProvider();

        JsonSerializerOptions serializerOptions = sp.GetRequiredService<IJsonSerializerOptionsProvider>().Instance;

        this.propertyBagFactory = sp.GetRequiredService<IJsonPropertyBagFactory>();
        this.RealJsonPropertyBag = this.propertyBagFactory.Create(Source);

        this.Utf8ArrayBackedPropertyBag = new Utf8MemoryBackedJsonPropertyBag(Source, serializerOptions);

        this.jsonDocument = JsonDocument.Parse(Source);
        this.PropertyBagWithExposedJsonElement = new JsonElementBackedPropertyBagWithExposedJsonElement(
            this.jsonDocument.RootElement,
            serializerOptions);
    }

    protected IPropertyBag RealJsonPropertyBag { get; }

    protected IPropertyBag Utf8ArrayBackedPropertyBag { get; }

    protected JsonElementBackedPropertyBagWithExposedJsonElement PropertyBagWithExposedJsonElement { get; }

    public virtual void Dispose()
    {
        this.jsonDocument.Dispose();
    }
}