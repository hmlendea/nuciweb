# Change guide

## Changing selector output

1. Inspect the relevant method in `NuciWeb/Select.cs`.
2. Confirm the intended XPath contract and input semantics.
3. Update or add exact-output tests in `NuciWeb.UnitTests/SelectTests.cs`.
4. Check whether `ARCHITECTURE.md`, API reference pages, behaviour pages, or flow pages describe the changed template.
5. Run `dotnet test`.

Changing template punctuation, predicates, attribute names, or escaping is observable API behaviour.

## Changing attribute names

Update `HtmlAttributeNames.cs`, its tests, and every selector/API document that lists or relies on the property. Verify the resulting selector strings.

## Adding runtime integrations

A browser driver, parser, service, configuration source, or persistence layer would cross the current architecture boundary. Add focused documentation for its lifecycle, failures, state, security, and tests rather than folding it into the current static-library description.