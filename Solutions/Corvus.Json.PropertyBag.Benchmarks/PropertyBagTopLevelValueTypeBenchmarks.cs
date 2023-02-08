// <copyright file="PropertyBagTopLevelValueTypeBenchmarks.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Json.PropertyBag.Benchmarks;

using System;
using System.Text.Json;

using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class PropertyBagTopLevelValueTypeBenchmarks : PropertyBagBenchmarkBase
{
    // No allocations
    [Benchmark(Baseline = true)]
    public int GetDirectIntPropertyViaGetAsInt()
    {
        if (!this.RealJsonPropertyBag.TryGet("intProperty", out int intProperty))
        {
            throw new InvalidOperationException("Failed to get intProperty as JsonElement");
        }

        if (intProperty != 42)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {(int)intProperty} vs 42");
        }

        return intProperty;
    }

    // Before adding the test for typeof(T) == typeof(JsonElement), this caused allocation (160B)
    // but with that test in place, this become zero-allocation
    [Benchmark]
    public int GetDirectIntPropertyViaGetAsJsonElementToJsonInteger()
    {
        if (!this.RealJsonPropertyBag.TryGet("intProperty", out JsonElement elem))
        {
            throw new InvalidOperationException("Failed to get intProperty as JsonElement");
        }

        JsonInteger intProperty = new(elem);
        if (intProperty != 42)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {(int)intProperty} vs 42");
        }

        return intProperty;
    }

    // No allocations
    [Benchmark]
    public int GetDirectIntPropertyViaGetAsIntUtf8Backed()
    {
        if (!this.Utf8ArrayBackedPropertyBag.TryGet("intProperty", out int intProperty))
        {
            throw new InvalidOperationException("Failed to get intProperty as JsonElement");
        }

        if (intProperty != 42)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {intProperty} vs 42");
        }

        return intProperty;
    }

    // Causes allocation (160B)
    [Benchmark]
    public int GetDirectIntPropertyViaGetAsJsonElementToJsonIntegerUtf8Backed()
    {
        if (!this.Utf8ArrayBackedPropertyBag.TryGet("intProperty", out JsonElement elem))
        {
            throw new InvalidOperationException("Failed to get intProperty as JsonElement");
        }

        JsonInteger intProperty = new(elem);
        if (intProperty != 42)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {(int)intProperty} vs 42");
        }

        return intProperty;
    }

    // No allocations
    [Benchmark]
    public int GetDirectIntPropertyiaGetAsJsonElementToJsonIntegerExposedJsonElement()
    {
        if (!this.PropertyBagWithExposedJsonElement.RawJson.TryGetProperty("intProperty", out JsonElement elem))
        {
            throw new InvalidOperationException("Failed to get intProperty as JsonElement");
        }

        JsonInteger intProperty = new(elem);
        if (intProperty != 42)
        {
            throw new InvalidOperationException($"intProperty did not have expected value: {(int)intProperty} vs 42");
        }

        return intProperty;
    }
}