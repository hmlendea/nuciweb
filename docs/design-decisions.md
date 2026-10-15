# Design decisions

## Static pure methods

The library uses static methods because selector construction has no required object state or lifecycle. This keeps calls direct and makes deterministic unit testing straightforward.

## Centralised attribute names

`HtmlAttributeNames` avoids repeating literal attribute names across selector templates and gives tests a single contract for those values.

## No parser integration

NuciWeb returns expressions instead of evaluating documents. This keeps browser, parser, network, authentication, and document-state concerns with the consuming application.

## Direct interpolation

Current implementation interpolates strings directly into XPath templates. This preserves a small API and exact caller-controlled output, but leaves escaping and validation to consumers. Any future escaping policy must be treated as a public behaviour change.