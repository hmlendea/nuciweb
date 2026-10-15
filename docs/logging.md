# Logging

NuciWeb has no logging framework, log configuration, log levels, sinks, correlation identifiers, audit events, retention policy, sampling, or runtime logging controls.

Production methods do not emit diagnostics. Consumers are responsible for logging their own document access, XPath evaluation, and errors. Selector arguments should be treated as potentially sensitive by consumers and should not be logged without a justified data-handling policy.