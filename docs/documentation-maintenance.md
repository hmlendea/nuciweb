# Documentation maintenance

Update documentation when public methods, XPath templates, attribute constants, target frameworks, project boundaries, or test coverage change.

## Required checks

- Keep root policy documents at the repository root.
- Keep links relative and verify their targets exist.
- Update the API reference and affected component, behaviour, flow, invariant, and change-guide pages together.
- Record absent infrastructure as not applicable rather than inventing implementation details.
- Run `dotnet test` after production-code changes.
- Review this catalogue when adding or removing a documentation file.

The root `ARCHITECTURE.md` remains the authoritative high-level architecture document; `docs/architecture.md` complements it and must not become a duplicate.