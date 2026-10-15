# Security model

This document complements the root [SECURITY.md](../SECURITY.md), which defines reporting and disclosure policy.

## Runtime exposure

Production code performs no network, file, database, process, browser, or logging operations. It receives caller strings and returns constructed XPath strings.

## Trust boundary

The caller controls selector input and the document against which the output may later be evaluated. Direct interpolation means untrusted values can alter XPath syntax or selection semantics if passed without suitable caller-side handling.

## Maintenance considerations

Changes involving escaping, validation, external integrations, or logging require security review and updated tests. Do not place secrets or personal data in selector examples or test output.