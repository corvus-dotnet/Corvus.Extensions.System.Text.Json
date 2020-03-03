@setupContainer
Feature: PropertyBagSpecs for SystemTextJson
	In order to provide strongly typed, extensible properties for a class
	As a developer
	I want to be able to use a property bag

Scenario: Get and set a property
	Given I set a property called "hello" to the value "world"
	When I get the property called "hello"
	Then the result should be "world"

Scenario: Get and set a missing property
	Given I set a property called "hello" to the value "world"
	When I get the property called "goodbye"
	Then the result should be null

Scenario: Get and set a null property
	Given I set a property called "hello" to null
	When I get the property called "hello"
	Then the result should be null

Scenario: Get and set a property to something, then to null
	Given I set a property called "hello" to the value "world"
	And I set a property called "hello" to null
	When I get the property called "hello"
	Then the result should be null

Scenario: Get and set a badly serialized property
	Given I set a property called "hello" to the value "jiggerypokery"
	When I get the property called "hello" as a custom object
	Then the result should be null

Scenario: Convert to a JSON string
	Given I set a property called "hello" to the value "world"
	And I set a property called "number" to the value 3
	When I cast to a string
	Then the result should be the JSON string
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |

Scenario: Serialize a property bag
	Given I set a property called "hello" to the value "world"
	And I set a property called "number" to the value 3
	And I serialize the property bag
	Then the result should be "{"hello":"world","number":3}"

Scenario: Deserialize a property bag
	Given I deserialize a property bag from the string "{"hello":"world","number":3}"
	Then the result should have the properties
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |


Scenario: Construct from a JSON string
	Given I create a JSON string
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	When I construct a PropertyBag from the JSON string
	Then the result should have the properties
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |

Scenario: Construct from a Dictionary
	Given I create a Dictionary
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	When I construct a PropertyBag from the Dictionary
	Then the result should have the properties
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |

Scenario: Construct from a JSON string with no serializer settings and configured defaults
	Given I create a JSON string
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	When I construct a PropertyBag from the JSON string with no serializer settings
	Then the result should have the properties
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	And the result should have the default serializer settings

Scenario: Construct from a Dictionary with no serializer settings and configured defaults
	Given I create a Dictionary
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	When I construct a PropertyBag from the Dictionary with no serializer settings
	Then the result should have the properties
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	And the result should have the default serializer settings

	Scenario: Construct from a JSON string with no serializer settings
	Given I create a JSON string
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	When I construct a PropertyBag from the JSON string with no serializer settings
	Then the result should have the properties
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	And the result should have the default serializer settings

Scenario: Construct from a Dictionary with no serializer settings
	Given I create a Dictionary
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	When I construct a PropertyBag from the Dictionary with no serializer settings
	Then the result should have the properties
	| Property | Value | Type    |
	| hello    | world | string  |
	| number   | 3     | integer |
	And the result should have the default serializer settings

Scenario: Construct with no serializer settings
	When I construct a PropertyBag with no serializer settings
	Then the result should have the properties
	| Property | Value | Type    |
	And the result should have the default serializer settings

	
Scenario: Construct with no serializer settings and configured defaults
	When I construct a PropertyBag with no serializer settings
	Then the result should have the properties
	| Property | Value | Type    |
	And the result should have the default serializer settings