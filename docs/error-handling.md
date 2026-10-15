# Error handling

Production methods do not throw library-defined exceptions and do not catch exceptions. They construct strings synchronously.

## Invalid or risky input

Inputs are interpolated directly into XPath literals. The library does not reject empty strings, quotes, or other special characters. Empty-string behaviour is explicitly tested for `ByClass`, `ById`, and `ByName`; quote handling is not tested or escaped.

If the returned expression is invalid for a consumer's XPath engine, the failure occurs when that consumer evaluates the expression. The consumer is responsible for validation, escaping, and handling evaluator-specific exceptions.

## Maintenance rule

Adding validation or escaping would change observable output and error semantics. Such a change requires explicit API-contract decisions and regression tests for accepted, rejected, and escaped values.