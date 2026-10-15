# Data model

NuciWeb has no domain entities, persistence models, serialised records, or database schema.

## Input and output values

The sole data type crossing the public API is `string`.

| Operation family | Input | Output | Transformation |
|------------------|-------|--------|----------------|
| `ByXPath` | XPath expression | Same string | None. |
| `ByClass` | Class token | XPath string | Inserts the token into a whitespace-normalised `@class` predicate. |
| `ById` | ID value | XPath string | Inserts the value into an exact `@id` predicate. |
| `ByName` | Name value | XPath string | Inserts the value into an exact `@name` predicate. |
| `ByExactValue` | Text or value | XPath string | Inserts the value into exact `text()` and `@value` predicates. |
| `ByPartialValue` | Text or value fragment | XPath string | Inserts the value into `contains(text(), ...)` and `contains(@value, ...)`. |

## State

Methods read only their arguments and static attribute-name properties. They mutate no state and retain no caller data after returning. The output string is owned by the caller.

## Invariants

- Attribute-name properties return the literal HTML names documented by `HtmlAttributeNamesTests`.
- `ByXPath` returns the exact input string.
- Selector output is deterministic for equal input strings.
- No HTML document is available inside the library during selector construction.