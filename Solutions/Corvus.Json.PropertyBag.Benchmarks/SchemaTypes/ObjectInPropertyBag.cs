// <copyright file="ObjectInPropertyBag.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Marain.Tenancy.ClientTenantProvider.TenancyClientSchemaTypes;

using Corvus.Json;

[JsonSchemaTypeGenerator("./Schema.json#/$defs/ObjectInPropertyBag")]
public readonly partial struct ObjectInPropertyBag;