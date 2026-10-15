# Repository overview

## Purpose

NuciWeb is a .NET 10 library for constructing XPath expressions that target HTML elements. It provides static selector builders and HTML attribute-name constants; it does not load, parse, or evaluate HTML.

## Public entry points

- `NuciWeb.Select` builds XPath strings for raw XPath, class, ID, name, exact value, and partial value queries.
- `NuciWeb.HtmlAttributeNames` exposes the attribute names used by the selector templates.

## Runtime model

A consumer supplies a string to a static method. The method interpolates that value into a fixed XPath template and returns the resulting string. The consumer evaluates the returned expression with its own HTML parser or browser automation library.

## Repository documents

- [Architecture](./architecture.md) complements the root [ARCHITECTURE.md](../ARCHITECTURE.md).
- [Repository structure](./repository-structure.md) maps source and test files.
- [API reference](./api-reference/INDEX.md) lists the public API.
- [Quick start](./quick-start.md) shows the normal consumer and contributor paths.
- [Testing](./testing.md) describes the NUnit test suite.
- [Security](./security.md) complements the root [SECURITY.md](../SECURITY.md).
- [Privacy](../PRIVACY.md), [Code of Conduct](../CODE_OF_CONDUCT.md), [Contributing](../CONTRIBUTING.md), and [License](../LICENSE) remain root-level policies.

## Evidence boundary

The repository contains no application host, service, database, configuration file, network endpoint, logging framework, scheduler, or persistence implementation. Documents for those topics record that they are not applicable rather than describing hypothetical behaviour.