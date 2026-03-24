namespace NuciWeb
{
    public static class Select
    {
        public static string ByClass(string className) =>
            ByXpath($"//*[contains(concat(' ', normalize-space(@class), ' '), ' {className} ')]");

        public static string ById(string id) => ByXpath($"//*[@id='{id}']");

        public static string ByName(string name) => ByXpath($"//*[@name='{name}']");

        public static string ByXpath(string xpath) => xpath;
    }
}