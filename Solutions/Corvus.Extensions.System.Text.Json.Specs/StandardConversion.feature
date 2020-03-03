@setupContainer
Feature: StandardConversion for SystemTextJson
	In order to ensure common serialization specifications
	As a developer
	I want to be provided with standard serializer settings

Scenario Outline: Serialize an object with convertible properties
	Given I serialize a POCO with "<SomeValue>", "<SomeDateTime>", "<SomeNullableDateTime>", "<SomeCulture>", "<SomeEnum>"
	Then the result should be "<Content>"

	Examples:
		| SomeValue   | SomeDateTime                  | SomeNullableDateTime          | SomeCulture | SomeEnum | Content                                                                                                                                                                                                                                                             |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someNullableDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someCulture":"en-US","someEnum":"second"} |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someNullableDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someCulture":"en-US","someEnum":"second"} |
		| Hello there | 2018-04-15T09:09:31.234+01:00 |                               |             | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someEnum":"second"}                                                                                                                          |

Scenario Outline: Deserialize an object with convertible properties
	Given I deserialize a POCO with the json string "<Content>"
	Then the result should have values "<SomeValue>", "<SomeDateTime>", "<SomeNullableDateTime>", "<SomeCulture>", "<SomeEnum>"

	Examples:
		| SomeValue   | SomeDateTime                  | SomeNullableDateTime          | SomeCulture | SomeEnum | Content                                                                                                                                                                                                                                                             |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someNullableDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someCulture":"en-US","someEnum":"second"} |
		| Hello there | 2018-04-15T09:09:31.234+01:00 | 2018-04-15T09:09:31.234+01:00 | en-US       | Second   | {"someValue":"Hello there","someDateTime":"2018-04-15T09:09:31.2340000+01:00","someNullableDateTime":"2018-04-15T09:09:31.234+01:00","someCulture":"en-US","someEnum":"second"}                                                                                     |
		| Hello there | 2018-04-15T09:09:31.234+01:00 |                               | en-US       | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someCulture":"en-US","someEnum":"second"}                                                                                                    |
		| Hello there | 2018-04-15T09:09:31.234+01:00 |                               |             | Second   | {"someValue":"Hello there","someDateTime":{"dateTimeOffset":"2018-04-15T09:09:31.234+01:00","unixTime":1523779771234},"someCulture":null,"someEnum":"second"}                                                                                                       |