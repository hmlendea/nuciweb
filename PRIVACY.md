# Privacy and Personal Data

NuciWeb is a .NET library providing XPath selector builders for HTML elements. It is distributed as a NuGet package and consumed as a project dependency. The library itself does not collect, process, store, or transmit any personal data.

## 📑 Table of Contents

- [What This Document Covers](#-what-this-document-covers)
- [Data We Handle](#-data-we-handle)
- [Processing and Use](#-processing-and-use)
- [Storage, Retention, and Deletion](#-storage-retention-and-deletion)
- [External Processing and Integrations](#-external-processing-and-integrations)
- [Document Changes](#-document-changes)
- [Contact](#-contact)

## 🔎 What This Document Covers

This document describes how NuciWeb at https://github.com/hmlendea/nuciweb handles personal data. It covers the library behaviour and verified integrations described below. NuciWeb is a library, not a service; it has no self-hosted deployment model.

## 📥 Data We Handle

### Data Provided to the Application

No personal data is requested or accepted by the library. Consumers pass string arguments to `Select` methods (e.g., class names, IDs, values) which are interpolated into XPath strings. These arguments are application-defined and not personal data unless the consumer chooses to pass such data.

### Data Generated or Collected by the Application

No personal data is generated or collected automatically by the library. The library produces only XPath strings as return values.

### Data Received from Integrations

No personal data is received from integrations or third parties. The library has no built-in integrations.

## 🧭 Processing and Use

The library processes the data described above for these verified functions:
- **XPath string generation** — Consumer-provided string arguments are interpolated into XPath templates using `HtmlAttributeNames` constants.

## 🗄️ Storage, Retention, and Deletion

The library does not store any data. It has no databases, files, logs, caches, or backups. Returned XPath strings exist only in the consumer's memory for the lifetime the consumer chooses.

## 🔗 External Processing and Integrations

The application has no built-in external data transfer. No external services, recipients, or integrations process or receive data from the library.

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| None | N/A | N/A | N/A |

## 🔄 Document Changes

Update this document when library data flows, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/nuciweb/blob/main/PRIVACY.md.

## 📬 Contact

For questions about library data handling, contact the project maintainers via GitHub issues at https://github.com/hmlendea/nuciweb/issues. Do not send passwords, access tokens, or other secrets.