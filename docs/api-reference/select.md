# Select

`NuciWeb.Select` is a static class. It returns XPath strings and performs no document access.

## ByXPath (string xpath)

Returns `xpath` unchanged.

## ByClass (string className)

Returns an XPath expression using a whitespace-normalised `@class` token predicate:

```text
//*[contains(concat(' ', normalize-space(@class), ' '), ' {className} ')]
```

## ById (string id)

Returns an XPath expression using exact ID equality:

```text
//*[@id='{id}']
```

## ByName (string name)

Returns an XPath expression using exact name equality:

```text
//*[@name='{name}']
```

## ByExactValue (string value)

Returns an XPath expression matching exact element text or exact `value` attribute content:

```text
//*[text()='{value}' or @value='{value}']
```

## ByPartialValue (string value)

Returns an XPath expression matching element text or `value` attribute content containing the supplied value:

```text
//*[contains(text(), '{value}') or contains(@value, '{value}')]
```

## Input and error contract

The methods do not validate or escape values. Empty strings are accepted and interpolated. XPath evaluation errors are the consumer's responsibility.