# Invariants

- Every public production member is static and stateless.
- Selector methods return strings and perform no HTML I/O.
- Equal inputs produce equal outputs.
- `HtmlAttributeNames` values are the attribute spellings used by selector templates.
- `ByClass` treats the requested class as a whitespace-delimited token in its XPath expression.
- `ById` and `ByName` use exact attribute equality.
- `ByExactValue` matches exact text or exact `value` attribute content.
- `ByPartialValue` matches contained text or contained `value` attribute content.
- The caller owns XPath escaping and evaluation concerns.
- The test project verifies output strings, not browser or parser behaviour.