# Ownership of the backing `JsonDocument` for property bags created from a `JsonElement`

## Status

Proposed

## Context

This repository defines a `System.Text.Json`-specific `IJsonPropertyBagFactory` interface enabling conversion between `IPropertyBag` instances and JSON. One of mechanisms it offers accepts a `JsonElement`. But this creates a problem: a `JsonElement` is always a façade over some underlying data, creating lifecycle issues. For as long as an `IPropertyBag` remains reachable, it needs to retain access to its data.

 For example, an application might initialize a `JsonDocument` from a `Stream`, in which case the `JsonDocument` maintains a copy of the data from the stream, and it also builds up indexing information produced by parsing the JSON. It acquires the memory for both of these purposes from `ArrayPool<byte>.Shared.Rent`, and this has two important consequences:

* the `JsonDocument` should be `Dispose`d once it is no longer needed so that the arrays can be returned to the pool
* once the `JsonDocument` has been disposed, it should no longer be used, either directly, or via any `JsonElement`s that came from it

The invalidation of the `JsonElement`s is of particular relevance here. A critical (but not entirely obvious) feature of this part of `System.Text.Json` is that each `JsonElement` is backed by some `JsonDocument` so the moment you `Dispose` a `JsonDocument`, not only do you make that `JsonDocument` invalid, you also make every single `JsonElement` obtained from that `JsonDocument` invalid.

There are some variations, because the data containing the JSON in its UTF-8 form isn't necessarily owned by the `JsonDocument`. For example, if the `JsonDocument` was created by passing in an existing buffer (by passing a `ReadOnlyMemory<byte>` to `Parse`) then the `JsonDocument` does not make its own copy of the UTF-8 data, and does not attempt to free that data when `Dispose`d. Instead it requires that whichever code called `Parse` ensures that the `ReadOnlyMemory<byte>` is not modified for as long as the `JsonDocument` remains in use. Even so, the `JsonDocument` must still be disposed because it still allocates internal data structures to keep track of the structure of data, and the array that holds this indexing data must still be returned to the pool once the `JsonDocument` is no longer needed.

Since each `JsonElement` is associated with an underlying `JsonDocument`, these lifetime issues affect those too. You do not `Dispose` a `JsonElement`, but its lifetime is inextricably linked to its parent `JsonDocument`. There's one interesting special case here, though: if you call `Clone` on a `JsonElement`, the `JsonElement` you get back is not tied to the lifetime of the `JsonDocument` it came from. (Internally, `System.Text.Json` creates a new `JsonDocument` with a copy of the subsection of data needed by `JsonElement` you cloned, but this implementation detail is not directly visible. The hidden `JsonDocument` it creates is special in that it does not need to be `Dispose`d, because it is created in a special way so that its backing stores for both the UTF-8 JSON bytes and the index are just ordinarily allocated arrays, not ones from the pool. We never actually get to see this `JsonDocument` because it's only used internally; the public programming model is simply that `JsonElement.Clone` returns a `JsonElement` that can be used indefinitely, and since `JsonElement` doesn't implement `IDisposable` there is nothing to be disposed.)

What does this mean for an `IPropertyBag` created from a `JsonElement`? We have three options:

1. call `Clone` on the `JsonElement` to ensure that the data remains available for as long as the `IPropertyBag` is reachable
2. store the `JsonElement` directly (a copy of its value, strictly speaking, since it is a `readonly struct`), and make it the application's problem to ensure that the corresponding `JsonDocument` is not disposed for as long as the `IPropertyBag` is in use
3. convert to some other representation, such as a ReadOnlyMemory<byte> (which is what our first prototype did)

There is no doubt that the first option is the safest. The downside is that it might result in unnecessary extra copies of data.

For example, consider a situation in which we fetch JSON documents which will be wrapped as `IPropertyBag` from some remote API, and we wish to cache the results fetched from the API to avoid repeatedly fetching the same data again and again. In this case, we would retain copies of the relevant data in some form. If we cached the entire response from the API, we would have some sort of buffer (e.g., an array) that could be wrapped as a `ReadOnlyMemory<byte>` and ideally we'd want to avoid a `JsonDocument` making its own copy of this data. It won't do if we pass a `ReadOnlyMemory<byte>` directly to `Parse`. And then for each part of the document that we want to present as an `IPropertyBag`, we can have the implementation of the bag hold a `JsonElement` that refers to the relevant segment of that one `JsonDocument`.

But where it gets tricky is when we want to evict data from the cache. This will happen either if it falls out of use, or if we determine that the cache contains stale data. The problem we have at this point is that we can't just `Dispose` the relevant `JsonDocument`, because we have no way of knowing whether any `IPropertyBag` instances are still around and using it.

But if an `IPropertyBag` always calls `Clone` when it is build from a `JsonElement`, it will have its very own arrays holding the data and indexing information, and normal GC behaviour will ensure that this is reachable for as long as necessary. But doesn't that mean lots of extra copies? Not necessarily. In the simple case where there's just one `IPropertyBag` in any particular JSON document, a cache system could create that `IPropertyBag` just once, and hand out the same one each time. There will be a single copy made when that is first created. (If the JSON to be represented as an `IPropertyBag` is only a subsection of the whole document, only that  subsection gets copied.) In the more complex case where we might want to represent several different parts of the same document as individual `IPropertyBag`s, one option we have is to call `document.RootElement.Clone()` which returns a `JsonElement` with its own copy of the whole document. This may not seem like an improvement if we're trying to avoid copies: we now have at least two copies of the data! However, all calls to `Clone` on the resulting `JsonElement` or on any of its descendants will *not* create additional copies. The `JsonElement` knows it is using a cloned copy of the document, and will continue to use that single copy as the basis for all further clones.

The only benefit that option 2 above might offer is that it could sometimes avoid that initial extra copy. If we were able to create an `IPropertyBag` from a `JsonElement` and tell it not to `Clone` it, we would be able to load the data just once (either into some buffer of our own, which we pass as a `ReadOnlyMemory<byte>` to `JsonDocument.Parse`, or we could just pass a `Stream` to `JsonDocument.Parse` or `JsonDocument.ParseAsync` and let the `JsonDocument` allocate its own buffer) and everything would use that via a single `JsonDocument`. By never calling `Clone` we never create an extra copy. But the downside is that this is much easier to get wrong. And since `IPropertyBag` doesn't inherit `IDisposable` there's no reliable way to find out when it's safe to `Dispose` the `JsonDocument`, so this is only really safe if your cache policy is "never discard anything". A few caches can work like that but most can't.

In short, option 2 is much easier to get wrong, and can only offers gains in quite constrained circumstances.

### Obtaining a property bag's `JsonElement` for use with `Corvus.JsonSchema` generated types

There's another issue that is closely related to the one being discussed here, and which needs to be considered in the design: we could like it to be possible to use `Corvus.JsonSchema` style types in conjunction with an `IPropertyBag`.

The usage model for extracting data from a property bag has always been that you call `IPropertyBag.TryGet<T>` and if the specified data is present, it will deserialize the data into an instance of `T`. (The `IPropertyBag` interface is already used extensively throughout our code; at the time of writing this everything uses the old `Newtonsoft.Json`-based implementation. So this usage model is a long established fact.) But that necessarily entails copying all of the data out of JSON form and into a .NET representation. This almost invariably means allocating memory on the GC heap. (There are some simple scenarios using just value types that can avoid this, but if you deserialize to a reference type, or if your data includes strings, there will be allocations.)

The types in `Corvus.Json.ExtendedTypes`, and the generated types produced by `Corvus.JsonSchema` can often avoid additional allocation beyond whatever was required to hold the UTF-8 JSON data itself. They work as lightweight value-typed facades over `JsonElement`s, extracting only the data the application actually asks to see, and not allocating anything on the heap unless it really is unavoidable.

Any code that wants to use these techniques while working with property bags will need some way of getting data from the bag in `JsonElement` form. This raises an important question:

Should the property bag expose its underlying `JsonElement` directly?

On the face of it, this might seem like a useful extension. However, there are two design options to compare:

* make the underlying `JsonElement` directly available through a new memory (and perhaps an `IJsonPropertyBag` interface)
* support `bag.TryGet(key, out JsonElement elem)`, enabling access to the `JsonElement` through the existing `TryGet<T>` method

I experimented with an implementation that provided a public `RawJson` property of type `JsonElement` so we could run some benchmarks. This was better than a naive `TryGet` implementation that relies on `JsonSerializer` to return a `JsonElement`. (It turns out that if you call `JsonSerializer.Deserialize<JsonElement>` it returns you a working `JsonElement`, but it will allocate some data on the GC heap as part of this.)

However, if we add code to the `TryGet` implementation that detects when the caller wants a `JsonElement`, we can implement this with a zero-allocation code path. Once I had added that, there were no memory allocation benefits to adding a member specifically for working with the `JsonElement`, and only some very small performance benefits that seem to come and go with small changes to the structure of the test.

Note that the benchmarks all showed superior performance for a `JsonElement.Clone`-backed implementation over our earlier prototype that stored a copy of the UTF-8 data in an array. With that array-backed prototype we could not provide zero-allocation access to a `JsonElement`. (If you don't keep hold of a `JsonDocument`, either directly, or via a cloned `JsonElement`, you end up needing to create a new one every time someone asks for a `JsonElement`.) And every single benchmark ran slower, most likely because with a cloned `JsonElement`, the hidden `JsonDocument` maintains an index describing the parsed structure, whereas if we store only the raw UTF-8 data, we end up forcing `System.Text.Json` to reparse it every time we look at it.

The benchmarks load this JSON document into an `IPropertyBag`:

```json
{
  "intProperty": 42,
  "objectProperty": {
    "intValue": 99,
    "int64Value": 3000000000000,
    "boolValue": false,
    "stringValue": "Hello, world"
  }
}
```

These benchmarks show various different ways of retrieving the `intValue` and `int64Value` subproperties of the `objectProperty`. The baseline calls `bag.TryGet("objectProperty", out JsonElement elem)` and then wraps the resulting element in a type generated by `Corvus.JsonSchema`. 

|                                                      Method |        Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------------------------------------------------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|                     GetIntPropertyOfObjectViaGetJsonElement |    98.27 ns |  1.949 ns |  4.926 ns |  1.00 |    0.00 |      - |         - |          NA |
|                   GetInt64PropertyOfObjectViaGetJsonElement |    95.73 ns |  1.790 ns |  1.587 ns |  0.92 |    0.05 |      - |         - |          NA |
|                        GetIntPropertyOfObjectViaDeserialize |   697.12 ns | 13.899 ns | 16.006 ns |  6.81 |    0.42 | 0.0334 |     280 B |          NA |
|                      GetInt64PropertyOfObjectViaDeserialize |   689.07 ns | 13.686 ns | 20.061 ns |  6.83 |    0.45 | 0.0334 |     280 B |          NA |
|           GetIntPropertyOfObjectViaGetJsonElementUtf8Backed | 1,339.75 ns | 26.324 ns | 35.142 ns | 13.11 |    0.76 | 0.0458 |     384 B |          NA |
|            GetInt64PropertyOfObjectViaJsonElementUtf8Backed | 1,346.36 ns | 26.824 ns | 42.545 ns | 13.40 |    0.74 | 0.0458 |     384 B |          NA |
|              GetIntPropertyOfObjectViaDeserializeUtf8Backed | 1,128.69 ns | 22.106 ns | 27.148 ns | 11.04 |    0.59 | 0.0324 |     280 B |          NA |
|            GetInt64PropertyOfObjectViaDeserializeUtf8Backed | 1,123.50 ns | 22.307 ns | 46.563 ns | 11.39 |    0.79 | 0.0324 |     280 B |          NA |
|   GetIntPropertyOfObjectViaGetJsonElementExposedJsonElement |    89.33 ns |  1.809 ns |  3.168 ns |  0.90 |    0.06 |      - |         - |          NA |
| GetInt64PropertyOfObjectViaGetJsonElementExposedJsonElement |    88.66 ns |  1.779 ns |  2.250 ns |  0.87 |    0.05 |      - |         - |          NA |

There are also some benchmarks that read the top-level value type property (`intProperty`) in a few different ways. The most interesting point here is that the UTF-8-backed version here will caused allocation in this case too if you attempt to fetch it as a `JsonElement` and then wrap it in a `Corvus.ExtendedTypes` `JsonInteger`.

|                                                                Method |     Mean |   Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------------------------------------------------------------- |---------:|--------:|---------:|------:|--------:|-------:|----------:|------------:|
|                                       GetDirectIntPropertyViaGetAsInt | 122.8 ns | 2.17 ns |  1.92 ns |  1.00 |    0.00 |      - |         - |          NA |
|                  GetDirectIntPropertyViaGetAsJsonElementToJsonInteger | 117.2 ns | 2.34 ns |  2.70 ns |  0.96 |    0.03 |      - |         - |          NA |
|                             GetDirectIntPropertyViaGetAsIntUtf8Backed | 239.8 ns | 4.60 ns |  3.84 ns |  1.95 |    0.05 |      - |         - |          NA |
|        GetDirectIntPropertyViaGetAsJsonElementToJsonIntegerUtf8Backed | 456.6 ns | 8.99 ns | 11.04 ns |  3.74 |    0.11 | 0.0191 |     160 B |          NA |
| GetDirectIntPropertyiaGetAsJsonElementToJsonIntegerExposedJsonElement | 112.3 ns | 2.16 ns |  2.40 ns |  0.92 |    0.03 |      - |         - |          NA |

(Note that the benchmarks that determine this create a single property bag during initialization, and are measuring repeated reading of data. This is consistent with the main use case we have in mind (tenancy) in which we will be caching entities with property bags, but it does mean that in a scenario in which we only ever read properties once, it's conceivable that the raw UTF-8 storage, which used `Utf8JsonReader`, might perform better: if you are in fact only ever going to read the data once, the fact that `JsonElement`/`JsonDocument` keep hold of the results of the parsing process turns from an advantage into a liability. If new single-use scenarios arise in which property bag performance is a critical factor, we could revisit this to see if a specialized factory offering a `Utf8JsonReader`-backed implementation might be worthwhile. But this would only be applicable if you were not intending to use `Corvus.JsonSchema`-generated types or `Corvus.Json.ExtendedTypes`.)


## Decision

Property bags will use a cloned `JsonElement` as their internal storage. This will not be made directly visible at the API surface, but the `TryGet<T>` implementation will detect when `T` is `JsonElement` to provide a zero-allocation code path, enabling efficient use of `Corvus.JsonSchema` style wrappers.


## Consequences

Existing code that has been using the `Newtonsoft.Json`-backed `IPropertyBag` implementation should be oblivious to this change. Applications should be able to modify their DI code to use this `System.Json.Text`-based implementation in its place (registering the factory and, if needed, `JsonConverter`s) without needing to change anything else. Existing libraries that use `IPropertyBag` shouldn't need to be changed.

The immediate benefit will be the removal of a dependency on `Newtonsoft.Json`. (Eliminating such dependencies entirely may be out of reach for a lot of applications, not least because the Azure SDK's storage libraries seem likely to continue to use it for a long time for compatibility purposes. But it may enable eliminating its use at runtime.)

Code that wishes to take advantage of `Corvus.Json.ExtendedTypes` or even `Corvus.JsonSchema`-generated types can call `TryGet<JsonElement>`. This means that once the initial allocation required to get a cloned `JsonElement` have been performed, all further use of the data in the property bag can be performed with zero allocations.

### Future options

We believe that this design choice does not close the door to an alternative to `IJsonPropertyBagFactory` that could support more complex buffer sharing scenarios. We won't do it unless we can demonstrate scenario in which this is a significant win, but nothing in the API design precludes that as a future direction. (After all, we're continuing to use `IPropertyBag)

