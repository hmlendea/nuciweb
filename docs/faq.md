# FAQ

## Does NuciWeb evaluate XPath?

No. It constructs and returns XPath strings. The consumer evaluates them.

## Does it parse HTML?

No. No HTML parser or browser driver is referenced by production code.

## Are selector values escaped?

No. Callers must supply values that are safe for the desired XPath literal syntax.

## Is the library stateful?

No. Production classes and methods are static and do not retain data.

## How is it tested?

NUnit tests compare exact returned strings. They do not require a browser or HTML document.