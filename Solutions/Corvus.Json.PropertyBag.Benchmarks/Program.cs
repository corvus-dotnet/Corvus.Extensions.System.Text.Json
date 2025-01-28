// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using BenchmarkDotNet.Running;

using Corvus.Json.PropertyBag.Benchmarks;

Type[] benchmarks = { typeof(PropertyBagIntegerSubPropertyBenchmarks), typeof(PropertyBagTopLevelValueTypeBenchmarks) };

#if DEBUG
BenchmarkRunner.Run(benchmarks, new BenchmarkDotNet.Configs.DebugInProcessConfig());
#else
BenchmarkRunner.Run(benchmarks);
#endif