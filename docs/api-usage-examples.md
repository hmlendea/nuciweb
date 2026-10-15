# API usage examples

## Build a selector

```csharp
string selector = NuciWeb.Select.ByClass("primary-button");
// //*[contains(concat(' ', normalize-space(@class), ' '), ' primary-button ')]
```

## Pass through raw XPath

```csharp
string selector = NuciWeb.Select.ByXPath("//button[@type='submit']");
```

## Use an attribute name

```csharp
string attributeName = NuciWeb.HtmlAttributeNames.Value;
// value
```

These examples construct strings only. Document retrieval and XPath evaluation are deliberately omitted because they belong to the consumer.