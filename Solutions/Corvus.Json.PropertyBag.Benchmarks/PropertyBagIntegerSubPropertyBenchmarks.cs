// <copyright file="PropertyBagIntegerSubPropertyBenchmarks.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.PropertyBag.Benchmarks;

using System;
using System.Text.Json;

using BenchmarkDotNet.Attributes;

using Marain.Tenancy.ClientTenantProvider.TenancyClientSchemaTypes;

[MemoryDiagnoser]
public class PropertyBagIntegerSubPropertyBenchmarks : PropertyBagBenchmarkBase
{
    // Before adding the test for typeof(T) == typeof(JsonElement), this caused allocation (384B)
    // but with that test in place, this become zero-allocation
    [Benchmark(Baseline = true)]
    public int GetIntPropertyOfObjectViaGetJsonElement()
    {
        if (!this.RealJsonPropertyBag.TryGet("objectProperty", out JsonElement elem))
        {
            throw new InvalidOperationException("Failed to get objectProperty as JsonElement");
        }

        ObjectInPropertyBag objectProperty = new(elem);
        int result = objectProperty.IntValue;
        if (result != 99)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {result} vs 99");
        }

        return result;
    }

    // Before adding the test for typeof(T) == typeof(JsonElement), this caused allocation (384B)
    // but with that test in place, this become zero-allocation
    [Benchmark]
    public long GetInt64PropertyOfObjectViaGetJsonElement()
    {
        if (!this.RealJsonPropertyBag.TryGet("objectProperty", out JsonElement elem))
        {
            throw new InvalidOperationException("Failed to get objectProperty as JsonElement");
        }

        ObjectInPropertyBag objectProperty = new(elem);
        long result = objectProperty.Int64Value;
        if (result != 3000000000000)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {result} vs 99");
        }

        return result;
    }

    // Causes allocation (280B)
    [Benchmark]
    public int GetIntPropertyOfObjectViaDeserialize()
    {
        if (!this.RealJsonPropertyBag.TryGet("objectProperty", out ObjectInPropertyBagClassicSerializable objectProperty))
        {
            throw new InvalidOperationException("Failed to get objectProperty as ObjectInPropertyBagClassicSerializable");
        }

        int result = objectProperty.IntValue;
        if (result != 99)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {result} vs 99");
        }

        return result;
    }

    [Benchmark]
    public long GetInt64PropertyOfObjectViaDeserialize()
    {
        if (!this.RealJsonPropertyBag.TryGet("objectProperty", out ObjectInPropertyBagClassicSerializable objectProperty))
        {
            throw new InvalidOperationException("Failed to get objectProperty as ObjectInPropertyBagClassicSerializable");
        }

        long result = objectProperty.Int64Value;
        if (result != 3000000000000)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {result} vs 99");
        }

        return result;
    }

    // Causes allocation (384B)
    [Benchmark]
    public int GetIntPropertyOfObjectViaGetJsonElementUtf8Backed()
    {
        if (!this.Utf8ArrayBackedPropertyBag.TryGet("objectProperty", out JsonElement elem))
        {
            throw new InvalidOperationException("Failed to get objectProperty as JsonElement");
        }

        ObjectInPropertyBag objectProperty = new(elem);
        int result = objectProperty.IntValue;
        if (result != 99)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {result} vs 99");
        }

        return result;
    }

    // Causes allocation (384B)
    [Benchmark]
    public long GetInt64PropertyOfObjectViaJsonElementUtf8Backed()
    {
        if (!this.Utf8ArrayBackedPropertyBag.TryGet("objectProperty", out JsonElement elem))
        {
            throw new InvalidOperationException("Failed to get objectProperty as JsonElement");
        }

        ObjectInPropertyBag objectProperty = new(elem);
        long result = objectProperty.Int64Value;
        if (result != 3000000000000)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {result} vs 99");
        }

        return result;
    }

    // Causes allocation (280B)
    [Benchmark]
    public int GetIntPropertyOfObjectViaDeserializeUtf8Backed()
    {
        if (!this.Utf8ArrayBackedPropertyBag.TryGet("objectProperty", out ObjectInPropertyBagClassicSerializable objectProperty))
        {
            throw new InvalidOperationException("Failed to get objectProperty as ObjectInPropertyBagClassicSerializable");
        }

        int result = objectProperty.IntValue;
        if (result != 99)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {result} vs 99");
        }

        return result;
    }

    [Benchmark]
    public long GetInt64PropertyOfObjectViaDeserializeUtf8Backed()
    {
        if (!this.Utf8ArrayBackedPropertyBag.TryGet("objectProperty", out ObjectInPropertyBagClassicSerializable objectProperty))
        {
            throw new InvalidOperationException("Failed to get objectProperty as ObjectInPropertyBagClassicSerializable");
        }

        long result = objectProperty.Int64Value;
        if (result != 3000000000000)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {result} vs 99");
        }

        return result;
    }

    // No allocations
    [Benchmark]
    public int GetIntPropertyOfObjectViaGetJsonElementExposedJsonElement()
    {
        if (!this.PropertyBagWithExposedJsonElement.RawJson.TryGetProperty("objectProperty", out JsonElement elem))
        {
            throw new InvalidOperationException("Failed to get objectProperty as JsonElement");
        }

        ObjectInPropertyBag objectProperty = new(elem);
        int result = objectProperty.IntValue;
        if (result != 99)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {result} vs 99");
        }

        return result;
    }

    // No allocations
    [Benchmark]
    public long GetInt64PropertyOfObjectViaGetJsonElementExposedJsonElement()
    {
        if (!this.PropertyBagWithExposedJsonElement.RawJson.TryGetProperty("objectProperty", out JsonElement elem))
        {
            throw new InvalidOperationException("Failed to get objectProperty as JsonElement");
        }

        ObjectInPropertyBag objectProperty = new(elem);
        long result = objectProperty.Int64Value;
        if (result != 3000000000000)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {result} vs 99");
        }

        return result;
    }
}