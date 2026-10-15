# API reference

## Summary

NuciWeb exposes static utility classes rather than HTTP controllers or endpoints. All public methods return `string` and accept `string` input.

| Class | Method family | Purpose | Authentication |
|-------|---------------|---------|----------------|
| `NuciWeb.Select` | `ByXPath`, `ByClass`, `ById`, `ByName`, `ByExactValue`, `ByPartialValue` | Construct XPath expressions. | None. |
| `NuciWeb.HtmlAttributeNames` | `Class`, `Hyperlink`, `Id`, `Name`, `Source`, `Style`, `Value` | Provide HTML attribute-name literals. | None. |

## Reference files

- [Select](./select.md)
- [HtmlAttributeNames](./html-attribute-names.md)

## Global contracts

- No authentication or authorisation exists.
- No rate limiting or throttling exists.
- No API versioning or network protocol exists.
- Methods are synchronous, static, and stateless.
- XPath evaluation is performed by the consumer, not by NuciWeb.

See [selector construction](../flows/selector-construction.md), [data model](../data-model.md), and [architecture](../architecture.md) for cross-cutting details.