# Configuration

NuciWeb has no runtime configuration system.

- No configuration files are consumed by production code.
- No environment variables are read.
- No command-line options are exposed.
- No profiles, precedence rules, secrets, ports, or reload behaviour exist.

Build configuration is declared in the two project files: both target `net10.0`, disable implicit usings, enable nullable analysis, and use the latest language version. The production project is marked `IsPackable=false`; the test project is marked as a test project and excluded from coverage.