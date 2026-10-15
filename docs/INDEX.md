# Documentation index

## Repository summary

NuciWeb is a .NET 10 static library that constructs XPath strings for HTML selectors. It owns deterministic string construction only; consumers own HTML acquisition, XPath evaluation, browser state, and all external effects.

## Start here

1. [Repository overview](./repository-overview.md)
2. [Quick start](./quick-start.md)
3. [API reference](./api-reference/INDEX.md)
4. [Testing](./testing.md)

## Deep dive

- [Architecture](./architecture.md)
- [Repository structure](./repository-structure.md)
- [Data model](./data-model.md)
- [Dependencies](./dependencies.md)
- [Design decisions](./design-decisions.md)
- [Invariants](./invariants.md)
- [Error handling](./error-handling.md)
- [Security model](./security.md)
- [Authentication](./authentication.md)
- [Configuration](./configuration.md)
- [Concurrency and scheduling](./concurrency-and-scheduling.md)
- [State and persistence](./state-and-persistence.md)
- [Logging](./logging.md)
- [Integrations](./integrations.md)

## Components

- [Component index](./components/INDEX.md)
- [Select](./components/select.md)
- [HtmlAttributeNames](./components/html-attribute-names.md)
- [Testing component](./components/testing.md)
- [Presentation](./components/presentation.md)
- [Host and composition](./components/host-and-composition.md)
- [Application services](./components/application-services.md)
- [Browser state and localisation](./components/browser-state-and-localisation.md)
- [Integration models](./components/integration-models.md)

## Flows

- [Selector construction](./flows/selector-construction.md)
- [Test verification](./flows/test-verification.md)
- [Startup and rendering](./flows/startup-and-rendering.md)
- [Calendar search](./flows/calendar-search.md)
- [Detail mutation](./flows/detail-mutation.md)

## Behaviour

- [Selector behaviour](./behaviour/selector-behaviour.md)
- [Consumer integration](./behaviour/consumer-integration.md)

## API usage

- [API usage examples](./api-usage-examples.md)

## Build and maintenance

- [Build and deployment](./build-and-deployment.md)
- [Change guide](./change-guide.md)
- [Documentation maintenance](./documentation-maintenance.md)
- [Troubleshooting](./troubleshooting.md)
- [FAQ](./faq.md)
- [Ambiguities and open questions](./ambiguities-and-open-questions.md)

## Root documents

- [README](../README.md)
- [Architecture](../ARCHITECTURE.md)
- [Contributing](../CONTRIBUTING.md)
- [Code of Conduct](../CODE_OF_CONDUCT.md)
- [Privacy](../PRIVACY.md)
- [Security](../SECURITY.md)
- [License](../LICENSE)

## Maintenance metadata

- Last reviewed: 2026-10-15
- Owner: NuciWeb maintainers
- Coverage status: All current production files and test files inspected; absent infrastructure is explicitly marked not applicable.