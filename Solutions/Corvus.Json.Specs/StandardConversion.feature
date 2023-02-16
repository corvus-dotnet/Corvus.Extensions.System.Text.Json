Feature: StandardConversion for SystemTextJson
	In order to ensure common serialization specifications
	As a developer
	I want to be provided with standard serializer settings

Scenario Outline: Serialize an object with convertible properties
	Given I have registered a global enum policy of 'CamelCase'
	And I serialize a POCO with '<SomeValue>', '<SomeDateTime>', '<SomeNullableDateTime>', '<SomeCulture>', '<SomeEnum>'
	Then the serialized JSON text should be '<Content>'

	Examples:
		| SomeValue   | SomeDateTime                  | SomeNullableDateTime          | SomeCulture | SomeEnum | Content                                                                                                                                                                     |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":"2018-04-15T09:09:31.234+01:00","someNullableDateTime":"2018-04-15T09:09:31.234+01:00","someCulture":"en-US","someEnum":"second"} |
		| Hello there | 2018-04-15T09:09:31.234+01:00 |                               |             | Second   | {"someValue":"Hello there","someDateTime":"2018-04-15T09:09:31.234+01:00","someEnum":"second"}                                                                              |


Scenario Outline: Deserialize an object with convertible properties
	Given I have registered a global enum policy of 'CamelCase'
	And I deserialize a POCO with the json string '<Content>'
	Then the deserialized POCO should have values '<SomeValue>', '<SomeDateTime>', '<SomeNullableDateTime>', '<SomeCulture>', '<SomeEnum>'

	Examples:
		| SomeValue   | SomeDateTime                  | SomeNullableDateTime          | SomeCulture | SomeEnum | Content                                                                                                                                                                         |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":"2018-04-15T09:09:31.234+01:00","someNullableDateTime":"2018-04-15T09:09:31.234+01:00","someCulture":"en-US","someEnum":"second"}     |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":"2018-04-15T09:09:31.2340000+01:00","someNullableDateTime":"2018-04-15T09:09:31.234+01:00","someCulture":"en-US","someEnum":"second"} |
		| Hello there | 2018-04-15T09:09:31.234+01:00 |                               | en-US       | Second   | {"someValue":"Hello there","someDateTime":"2018-04-15T09:09:31.234+01:00","someCulture":"en-US","someEnum":"second"}                                                            |
		| Hello there | 2018-04-15T09:09:31.234+01:00 |                               |             | Second   | {"someValue":"Hello there","someDateTime":"2018-04-15T09:09:31.234+01:00","someCulture":null,"someEnum":"second"}                                                               |

# The DateTimeOffsetToIso8601AndUnixTimeConverter was added by default in earlier versions of the library, because at
# one point something we used relied on this (possibly searchability in CosmosDB, but nobody can remember for sure) but
# the normal date conversions are more useful most of the time. So we separate out the test that expects this
# unusual conversion - it uses a different DI setup from everything else.

Scenario Outline: Serialize an object with convertible properties using the DateTimeOffsetToIso8601AndUnixTimeConverter
	Given I have registered a global enum policy of 'CamelCase'
	And I have registered the DateTimeOffsetToIso8601AndUnixTimeConverter
	Given I serialize a POCO with '<SomeValue>', '<SomeDateTime>', '<SomeNullableDateTime>', '<SomeCulture>', '<SomeEnum>'
	Then the serialized JSON text should be '<Content>'

	Examples:
		| SomeValue   | SomeDateTime                  | SomeNullableDateTime          | SomeCulture | SomeEnum | Content                                                                                                                                                                                                                                                             |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someNullableDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someCulture":"en-US","someEnum":"second"} |
		| Hello there | 2018-04-15T09:09:31.234+01:00 |                               |             | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someEnum":"second"}                                                                                                                          |

Scenario Outline: Deserialize an object with convertible properties using the DateTimeOffsetToIso8601AndUnixTimeConverter
	Given I have registered a global enum policy of 'CamelCase'
	And I have registered the DateTimeOffsetToIso8601AndUnixTimeConverter
	And I deserialize a POCO with the json string '<Content>'
	Then the deserialized POCO should have values '<SomeValue>', '<SomeDateTime>', '<SomeNullableDateTime>', '<SomeCulture>', '<SomeEnum>'

	Examples:
		| SomeValue   | SomeDateTime                  | SomeNullableDateTime          | SomeCulture | SomeEnum | Content                                                                                                                                                                                                                                                             |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someNullableDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someCulture":"en-US","someEnum":"second"} |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":{"unixTime":1523779771234,"dateTimeOffset":"2018-04-15T09:09:31.234+01:00"},"someNullableDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someCulture":"en-US","someEnum":"second"} |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":"2018-04-15T09:09:31.2340000+01:00","someNullableDateTime":"2018-04-15T09:09:31.234+01:00","someCulture":"en-US","someEnum":"second"}                                                                                     |
		| Hello there | 2018-04-15T09:09:31.234+01:00 |                               | en-US       | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someCulture":"en-US","someEnum":"second"}                                                                                                    |
		| Hello there | 2018-04-15T09:09:31.234+01:00 |                               |             | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someCulture":null,"someEnum":"second"}                                                                                                       |