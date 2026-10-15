# Testing component

The test project is `NuciWeb.UnitTests`. It references `NuciWeb` and uses NUnit assertions.

## Organisation

- `HtmlAttributeNamesTests` covers all seven attribute-name properties.
- `SelectTests` covers `ByXPath`, `ByClass`, `ById`, and `ByName`, with empty-string cases for the latter three.

Tests assert exact output strings. They do not launch a browser, parse an HTML document, or exercise an external XPath engine.

## Coverage boundary

`ByExactValue` and `ByPartialValue` are public methods but currently lack dedicated test methods, as recorded in `ARCHITECTURE.md`. Any change to those methods should add exact-output tests before implementation changes are accepted.