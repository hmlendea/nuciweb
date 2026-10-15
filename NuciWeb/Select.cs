namespace NuciWeb
{
    /// <summary>
    /// Provides XPath selector builders for HTML elements.
    /// </summary>
    public static class Select
    {
        /// <summary>
        /// Returns the provided XPath expression unchanged.
        /// </summary>
        /// <param name="xpath">The XPath expression.</param>
        /// <returns>The same XPath expression.</returns>
        public static string ByXPath(string xpath) => xpath;

        /// <summary>
        /// Builds an XPath selector for elements with the specified class name.
        /// </summary>
        /// <param name="className">The class name to match.</param>
        /// <returns>An XPath string that selects elements containing the class name.</returns>
        public static string ByClass(string className) =>
            ByXPath($"//*[contains(concat(' ', normalize-space(@{HtmlAttributeNames.Class}), ' '), ' {className} ')]");

        /// <summary>
        /// Builds an XPath selector for an element with the specified ID.
        /// </summary>
        /// <param name="id">The ID to match.</param>
        /// <returns>An XPath string that selects the element with the specified ID.</returns>
        public static string ById(string id) => ByXPath($"//*[@{HtmlAttributeNames.Id}='{id}']");

        /// <summary>
        /// Builds an XPath selector for elements with the specified name attribute.
        /// </summary>
        /// <param name="name">The name attribute value to match.</param>
        /// <returns>An XPath string that selects elements with the specified name.</returns>
        public static string ByName(string name) => ByXPath($"//*[@{HtmlAttributeNames.Name}='{name}']");

        /// <summary>
        /// Builds an XPath selector for elements with the exact text content or value attribute.
        /// </summary>
        /// <param name="value">The exact text content or value attribute to match.</param>
        /// <returns>An XPath string that selects elements with the exact text content or value attribute.</returns>
        public static string ByExactValue(string value) => ByXPath($"//*[text()='{value}' or @{HtmlAttributeNames.Value}='{value}']");

        /// <summary>
        /// Builds an XPath selector for elements containing the specified text content or value attribute.
        /// </summary>
        /// <param name="value">The text content or value attribute to search for.</param>
        /// <returns>An XPath string that selects elements containing the text content or value attribute.</returns>
        public static string ByPartialValue(string value) => ByXPath($"//*[contains(text(), '{value}') or contains(@{HtmlAttributeNames.Value}, '{value}')]");
    }
}