# Integrations

NuciWeb has no built-in external integrations. It returns XPath strings for a consumer to use with an independently selected browser automation or HTML-processing library.

| Integration boundary | Owned by | NuciWeb responsibility |
|----------------------|----------|------------------------|
| HTML acquisition | Consumer | None. |
| XPath evaluation | Consumer | Construct the expression. |
| Browser lifecycle | Consumer | None. |
| Network and authentication | Consumer | None. |
| Persistence and caching | Consumer | None. |

Adding an integration would introduce new lifecycle, failure, configuration, and security contracts that are absent from the current implementation.