# HtmlAttributeNames component

## Responsibility

`NuciWeb.HtmlAttributeNames` centralises the literal HTML attribute names used by selector templates.

## Public values

| Property | Value |
|----------|-------|
| `Class` | `class` |
| `Hyperlink` | `href` |
| `Id` | `id` |
| `Name` | `name` |
| `Source` | `src` |
| `Style` | `style` |
| `Value` | `value` |

The properties are static, read-only, and return strings. `Select` uses the relevant properties when constructing attribute predicates.

## Tests

`NuciWeb.UnitTests/HtmlAttributeNamesTests.cs` verifies every property value. Changing a value changes selector output where that attribute is used and therefore affects the public contract.