# Quick start

## Use the library

Add a project reference or use the documented package installation path in the root [README.md](../README.md), then call a static selector method:

```csharp
string selector = NuciWeb.Select.ById("submit");
```

The result is an XPath string. Pass it to the consumer's browser or HTML-processing API.

## Contribute

Install the .NET 10 SDK, restore dependencies, and run:

```bash
dotnet restore
dotnet test
```

See [CONTRIBUTING.md](../CONTRIBUTING.md) for the repository workflow.