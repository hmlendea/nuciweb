# Build and deployment

## Build

The SDK-style solution targets .NET 10 and can be built from the repository root:

```bash
dotnet build
```

## Distribution

`NuciWeb.csproj` sets `IsPackable=false`. The repository does not contain a production host, installer, container image, deployment target, or release script. Consumers use the source project or an externally managed distribution process.

## Runtime requirements

The library has no native dependency or platform-specific code. Its declared target is `net10.0`.