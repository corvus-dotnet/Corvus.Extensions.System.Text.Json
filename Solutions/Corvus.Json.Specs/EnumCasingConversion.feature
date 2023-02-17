Feature: Enum type conversion with customized casing
	In order to support both camelCased and PascalCased representations of enumerations
	As a developer
	I want to use JsonConverters that give me control over enumeration serialization

Scenario Outline: Serialize an object with enum properties and a global enum policy
	Given I have registered a global enum policy of '<GlobalPolicy>'
	When I serialize an EnumPoco POCO with '<FirstValue>', '<SecondValue>'
	Then the serialized JSON text should be '<Content>'

	Examples:
		| GlobalPolicy | FirstValue | SecondValue | Content                                                 |
		| CamelCase    | First      | ValueOne    | {"firstEnumValue":"first","secondEnumValue":"valueOne"} |
		| PascalCase   | First      | ValueOne    | {"firstEnumValue":"First","secondEnumValue":"ValueOne"} |

Scenario Outline: Serialize an object with type-specific overrides
	Given I have registered an enum policy of '<LocalPolicy1>' for 'ExampleEnum'
	Given I have registered an enum policy of '<LocalPolicy2>' for 'ExampleEnum2'
	And I have registered a global enum policy of '<GlobalPolicy>'
	When I serialize an EnumPoco POCO with '<FirstValue>', '<SecondValue>'
	Then the serialized JSON text should be '<Content>'

	Examples:
		| LocalPolicy1 | LocalPolicy2 | FirstValue | SecondValue | Content                                                 |
		| CamelCase    | CamelCase    | First      | ValueOne    | {"firstEnumValue":"first","secondEnumValue":"valueOne"} |
		| PascalCase   | CamelCase    | First      | ValueOne    | {"firstEnumValue":"First","secondEnumValue":"valueOne"} |
		| CamelCase    | PascalCase   | First      | ValueOne    | {"firstEnumValue":"first","secondEnumValue":"ValueOne"} |
		| PascalCase   | PascalCase   | First      | ValueOne    | {"firstEnumValue":"First","secondEnumValue":"ValueOne"} |

Scenario Outline: Serialize an object with enum properties, a global enum policy, and a type-specific override
	Given I have registered an enum policy of '<LocalPolicy>' for 'ExampleEnum2'
	And I have registered a global enum policy of '<GlobalPolicy>'
	When I serialize an EnumPoco POCO with '<FirstValue>', '<SecondValue>'
	Then the serialized JSON text should be '<Content>'

	Examples:
		| GlobalPolicy | LocalPolicy | FirstValue | SecondValue | Content                                                 |
		| CamelCase    | CamelCase   | First      | ValueOne    | {"firstEnumValue":"first","secondEnumValue":"valueOne"} |
		| PascalCase   | CamelCase   | First      | ValueOne    | {"firstEnumValue":"First","secondEnumValue":"valueOne"} |
		| CamelCase    | PascalCase  | First      | ValueOne    | {"firstEnumValue":"first","secondEnumValue":"ValueOne"} |
		| PascalCase   | PascalCase  | First      | ValueOne    | {"firstEnumValue":"First","secondEnumValue":"ValueOne"} |

Scenario Outline: Deserialize an object with enum properties and a global enum policy
	Given I have registered a global enum policy of '<GlobalPolicy>'
	When I deserialize an EnumPoco POCO with the json string '<Content>'
	Then the result should have enum values '<FirstValue>', '<SecondValue>'

	Examples:
		| GlobalPolicy | FirstValue | SecondValue | Content                                                 |
		| CamelCase    | First      | ValueOne    | {"firstEnumValue":"first","secondEnumValue":"valueOne"} |
		| PascalCase   | First      | ValueOne    | {"firstEnumValue":"First","secondEnumValue":"ValueOne"} |

# This isn't strictly a casing scenario, but this test is here to clarify the default behaviour.
Scenario Outline: Serialize an object with enum properties with no global enum policy
	When I serialize an EnumPoco POCO with '<FirstValue>', '<SecondValue>'
	Then the serialized JSON text should be '<Content>'

	Examples:
		| FirstValue | SecondValue | Content                                  |
		| First      | ValueOne    | {"firstEnumValue":0,"secondEnumValue":0} |
		| Second     | ValueThree  | {"firstEnumValue":1,"secondEnumValue":2} |
