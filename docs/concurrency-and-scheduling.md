# Concurrency and scheduling

There are no asynchronous methods, background workers, timers, queues, locks, schedulers, or mutable shared state.

Because production methods only read arguments and static read-only properties, concurrent calls have no library-owned state interaction. Thread-safety of the consumer's XPath evaluator, browser driver, or parser is outside this repository.