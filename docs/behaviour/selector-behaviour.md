# Selector behaviour

## Supported selector forms

- Raw XPath is passed through unchanged.
- Class selection uses a normalised-space token predicate, preventing a class name from matching only as a substring of another token.
- ID and name selection use exact attribute equality.
- Exact value selection checks exact element text or exact `value` attribute content.
- Partial value selection checks whether element text or `value` attribute content contains the supplied fragment.

## Input semantics

The methods accept empty strings and interpolate them. They do not trim, normalise, validate, or escape caller input. The caller must provide values compatible with the intended XPath literal syntax.