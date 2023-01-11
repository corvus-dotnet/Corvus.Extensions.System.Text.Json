@perFeatureContainer

Feature: PropertyBagSpecs for SystemTextJson
	In order to provide strongly typed, extensible properties for a class that serialize neatly as JSON
	As a developer
	I want to be able to use a property bag

Scenario: Create from a property, and get that property
	Given the creation properties include "hello" with the value "world"
	When I create the property bag from the creation properties
	Then the property bag should contain a property called "hello" with the value "world"

Scenario: Create from a property, and get a missing property
	Given the creation properties include "hello" with the value "world"
	When I create the property bag from the creation properties
	Then calling TryGet with "goodbye" should return false and the result should be null

Scenario: Create from a property, and get a null property
	Given the creation properties include "hello" with a null value
	When I create the property bag from the creation properties
	Then calling TryGet with "goodbye" should return false and the result should be null

Scenario: Get and set a badly serialized property
	Given the creation properties include "hello" with the value "jiggerypokery"
	When I create the property bag from the creation properties
	And I get the property called "hello" as a custom object expecting an exception
	Then TryGet should have thrown a SerializationException

Scenario: Convert to a JsonDocument
	Given the creation properties include "hello" with the value "world"
	And the creation properties include "number" with the value 3
	And the creation properties include "fpnumber" with the floating point value 3.14
	And the creation properties include "date" with the date value "2020-04-17T07:06:10+01:00"
	And I create the property bag from the creation properties
	When I get the property bag as a JsonDocument
	Then the JsonDocument should have these properties
	| Property | Value                     | Type    |
	| hello    | world                     | string  |
	| number   | 3                         | integer |
	| fpnumber | 3.14                      | fp      |
	| date     | 2020-04-17T07:06:10+01:00 | string  |

Scenario: Convert to a JsonDocument with non-Pascal-cased property names
	Given the creation properties include "Hello" with the value "World"
	And the creation properties include "Number" with the value 3
	And the creation properties include "FpNumber" with the floating point value 3.14
	And the creation properties include "Date" with the date value "2020-04-17T07:06:10+01:00"
	And I create the property bag from the creation properties
	When I get the property bag as a JsonDocument
	Then the JsonDocument should have these properties
	| Property | Value                     | Type    |
	| Hello    | World                     | string  |
	| Number   | 3                         | integer |
	| FpNumber | 3.14                      | fp      |
	| Date     | 2020-04-17T07:06:10+01:00 | string  |

Scenario: Convert to a JSON string
	Given the creation properties include "hello" with the value "world"
	And the creation properties include "number" with the value 3
	And the creation properties include "fpnumber" with the floating point value 3.14
	And the creation properties include "date" with the date value "2020-04-17T07:06:10+01:00"
	And I create the property bag from the creation properties
	When I get the property bag's JSON and write it to a JsonDocument
	Then the JsonDocument should have these properties
	| Property | Value                     | Type    |
	| hello    | world                     | string  |
	| number   | 3                         | integer |
	| fpnumber | 3.14                      | fp      |
	| date     | 2020-04-17T07:06:10+01:00 | string  |

Scenario: Retrieve an object property as an IPropertyBag
	Given I deserialize a property bag from the string
		"""
		{
			"hello": "world",
			"number": 3,
			"nested": {
				"nestedstring": "goodbye",
				"nestednumber": 4
			}
		}
		"""
	When I get the property called "nested" as an IPropertyBag and call it "nested"
	Then the IPropertyBag called "nested" should have the properties
	| Property     | Value   | Type    |
	| nestedstring | goodbye | string  |
	| nestednumber | 4       | integer |

Scenario: Serialize a property bag
	Given the creation properties include "hello" with the value "world"
	And the creation properties include "number" with the value 3
	And the creation properties include "date" with the date value "2020-04-17T07:06:10+03:00"
	And the creation properties include "preciseDate" with the date value "2020-04-17T07:06:10.12345+01:00"
	And I create the property bag from the creation properties
	When I serialize the property bag
	# Note that the + symbols become \u002B. https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/migrate-from-newtonsoft?pivots=dotnet-6-0#minimal-character-escaping
	Then the result should be
		"""
		{"hello":"world","number":3,"date":"2020-04-17T07:06:10\u002B03:00","preciseDate":"2020-04-17T07:06:10.12345\u002B01:00"}
		"""

Scenario Outline: POCO serialization and deserialization
	Given the creation properties include a DateTime POCO called "datetimepoco" with "<time>" "<nullableTime>"
	Given the creation properties include a CultureInfo POCO called "cultureinfopoco" with "<culture>"
	Given the creation properties include an enum value called "enumvalue" with value "<enum>"
	When I create the property bag from the creation properties
	And I serialize the property bag
	And I deserialize the serialized property bag
	Then the result should have a DateTime POCO named "datetimepoco" with values "<time>" "<nullableTime>"
	Then the result should have a CultureInfo POCO named "cultureinfopoco" with value "<culture>"
	Then the result should have an enum value named "enumvalue" with value "<enum>"
	Examples:
	| time                          | nullableTime                | culture | enum   |
	| 2020-04-17T07:06:10.0+01:00   | 2020-05-01T13:14:15.3+01:00 | en-GB   | First  |
	| 2020-05-04T00:00:00.0+00:00   |                             | en-US   | Second |
	| 2020-03-14T03:49:20.527+00:00 |                             |         | Third  |

Scenario Outline: POCO deserialization
	Given I deserialize a property bag from the string <bagJson>
	Then the result should have a DateTime POCO named "datetimepoco" with values "<time>" "<nullableTime>"
	Then the result should have a CultureInfo POCO named "cultureinfopoco" with value "<culture>"
	Then the result should have an enum value named "enumvalue" with value "<enum>"
	Examples:
	| time                          | nullableTime                | culture | enum   | bagJson                                                                                                                                                                            |
	| 2020-04-17T07:06:10.0+01:00   | 2020-05-01T13:14:15.3+01:00 | en-GB   | First  | {"datetimepoco":{"someDateTime":"2020-04-17T07:06:10.0+01:00","someNullableDateTime":"2020-05-01T13:14:15.3+01:00"},"cultureinfopoco":{"someCulture":"en-GB"},"enumvalue":"First"} |
	| 2020-05-04T00:00:00.0+00:00   |                             | en-US   | Second | {"datetimepoco":{"someDateTime":"2020-05-04T00:00:00.0+00:00"},"cultureinfopoco":{"someCulture":"en-US"},"enumvalue":"Second"}                                                     |
	| 2020-03-14T03:49:20.527+00:00 |                             |         | Third  | {"datetimepoco":{"someDateTime":"2020-03-14T03:49:20.527+00:00"},"enumvalue":"Third"}                                                                                              |

Scenario: Create from UTF8 JSON
	Given I create JSON UTF8 with these properties
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	When I create a PropertyBag from the UTF8 JSON string
	Then the IPropertyBag should have the properties
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |


Scenario: Construct with modifications from an existing property bag that contains nested objects
	Given I deserialize a property bag from the string
		"""
		{
			"hello": "world",
			"number": 3,
			"float": 3.1,
			"t": true,
			"f": false,
			"scalarArray": [1, 2, 3 ,4],
			"objectArray": [
				{ "prop1": "val1", "prop2": 1 },
				{ "prop1": "val2", "prop2": 2 },
				{ "prop1": "val3", "prop2": 3 }
			],
			"nested1": {
				"nestedstring": "goodbye",
				"nestednumber": 4,
				"nestedscalararray": ["a", "b", "c"],
				"nestedobject": {
					"nestedstring": "hello again",
					"nestednumber": 5
				}
			},
			"nested2": {
				"nestedstring": "hello again"
			}
		}
		"""
	When I add, modify, or remove properties
	| Property | Value | Type   | Action   |
	| foo      | bar   | string | addOrSet |
	| nested2  |       |        | remove   |
	And I get the property called "nested1" as an IPropertyBag and call it "nestedbag"
	Then the IPropertyBag should have the properties
	| Property    | Value | Type         |
	| hello       | world | string       |
	| number      | 3     | integer      |
	| float       | 3.1   | fp           |
	| t           | true  | boolean      |
	| f           | false | boolean      |
	| foo         | bar   | string       |
	| objectArray |       | object[]     |
	| scalarArray |       | object[]     |
	| nested1     |       | IPropertyBag |
	And the IPropertyBag should not have the properties
	| Property |
	| nested2  |
	Then the IPropertyBag called "nestedbag" should have the properties
	| Property     | Value   | Type   |
	| nestedstring | goodbye | string |
