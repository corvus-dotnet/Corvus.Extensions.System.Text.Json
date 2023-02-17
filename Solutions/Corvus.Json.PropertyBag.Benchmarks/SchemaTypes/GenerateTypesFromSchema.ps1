$SchemaPath = Join-Path -Resolve $PSScriptRoot Schema.json

[string[]]$SchemaTypes = "ObjectInPropertyBag"

foreach ($SchemaType in $SchemaTypes) {
    generatejsonschematypes `
        $SchemaPath `
        --rootNamespace Marain.Tenancy.ClientTenantProvider.TenancyClientSchemaTypes --rootPath "#/schemas/$SchemaType" --outputPath .
}