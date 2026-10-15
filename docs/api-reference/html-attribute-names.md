# HtmlAttributeNames

`NuciWeb.HtmlAttributeNames` is a static class exposing read-only string properties.

| Property | Return value |
|----------|--------------|
| `Class` | `class` |
| `Hyperlink` | `href` |
| `Id` | `id` |
| `Name` | `name` |
| `Source` | `src` |
| `Style` | `style` |
| `Value` | `value` |

The properties are consumed by `Select` when forming attribute predicates. They do not read or mutate HTML.