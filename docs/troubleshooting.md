# Troubleshooting

| Symptom | Probable cause | Resolution |
|---------|----------------|------------|
| XPath evaluation fails | A supplied value contains syntax-sensitive characters because NuciWeb does not escape input. | Validate or escape input in the consumer before constructing a selector. |
| Selector returns no elements | The generated predicate does not match the document or the consumer's XPath context. | Inspect the returned string and the document; NuciWeb does not evaluate either. |
| Tests are not discovered | The .NET SDK or test adapter is unavailable. | Restore dependencies and run `dotnet test` with the .NET 10 SDK. |
| A method's output changed unexpectedly | A selector template or attribute-name property changed. | Compare the exact-output tests and update the documented contract deliberately. |