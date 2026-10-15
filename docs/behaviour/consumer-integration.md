# Consumer integration behaviour

NuciWeb is a construction library, not an HTML client. A consumer typically calls a static selector method and passes the returned XPath to an independent browser automation or HTML-processing API.

The consumer retains responsibility for:

- Obtaining the HTML or browser document.
- Selecting an XPath engine and its supported XPath version.
- Escaping values before passing them to NuciWeb when required.
- Handling no-match results and evaluator exceptions.
- Managing browser, network, authentication, and document state.

No integration adapter is present in this repository.