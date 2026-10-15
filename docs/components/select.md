# Select component

## Responsibility

`NuciWeb.Select` is the public selector-builder surface. Each method maps one caller-provided string to a deterministic XPath expression.

## Methods

- `ByXPath` returns the expression unchanged.
- `ByClass` emits a whitespace-normalised class-token predicate.
- `ById` emits exact `@id` equality.
- `ByName` emits exact `@name` equality.
- `ByExactValue` emits exact `text()` or `@value` equality.
- `ByPartialValue` emits `contains` predicates over `text()` or `@value`.

## Boundaries

The component does not find elements, access browsers, parse HTML, validate arguments, escape XPath literals, or retain state. Its output contract is the exact string returned to the caller.

## Tests

`NuciWeb.UnitTests/SelectTests.cs` verifies raw XPath pass-through, class, ID, and name output, including empty-string cases. The root architecture document records that exact and partial value methods need dedicated coverage.