# Test verification flow

1. `dotnet test` builds the solution and discovers the NUnit test project.
2. The test adapter invokes each test method.
3. Tests call the static production methods with fixed strings.
4. NUnit compares returned strings with expected XPath or attribute literals.
5. A mismatch fails the test; no production state needs cleanup because methods are stateless.

This flow verifies deterministic string construction only. It does not verify XPath execution against a document or a browser.