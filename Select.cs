namespace NuciWeb
{
    public static class Select
    {
        public static string ByClass(string className) =>
            ByXPath($"//*[contains(concat(' ', normalize-space(@class), ' '), ' {className} ')]");

        public static string ById(string id) => ByXPath($"//*[@id='{id}']");

        public static string ByName(string name) => ByXPath($"//*[@name='{name}']");

        public static string ByXPath(string xpath) => xpath;
    }
}