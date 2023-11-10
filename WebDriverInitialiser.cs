using System;
using System.IO;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace NuciWeb
{
    public sealed class WebDriverInitialiser
    {
        public static IWebDriver InitialiseAvailableWebDriver(
            bool isDebugModeEnabled = true,
            int pageLoadTimeout = 90)
        {
            if (File.Exists("/usr/bin/geckodriver"))
            {
                return InitialiseFirefoxDriver(isDebugModeEnabled, pageLoadTimeout);
            }
            else
            {
                return InitialiseChromeDriver(isDebugModeEnabled, pageLoadTimeout);
            }
        }

        public static IWebDriver InitialiseFirefoxDriver(
            bool isDebugModeEnabled = true,
            int pageLoadTimeout = 90)
        {
            FirefoxOptions options = new FirefoxOptions
            {
                PageLoadStrategy = PageLoadStrategy.None
            };

            options.AddArgument("--disable-save-password-bubble");
            options.SetPreference("privacy.firstparty.isolate", false);

            if (isDebugModeEnabled)
            {
                options.AddArgument("--headless");
                options.SetPreference("permissions.default.image", 2);
            }

            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            service.LogLevel = FirefoxDriverLogLevel.Error;

            IWebDriver driver = new FirefoxDriver(service, options, TimeSpan.FromSeconds(pageLoadTimeout));

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);
            driver.Manage().Window.Maximize();

            return driver;
        }

        public static IWebDriver InitialiseChromeDriver(
            bool isDebugModeEnabled = true,
            int pageLoadTimeout = 90)
        {
            ChromeOptions options = new ChromeOptions
            {
                PageLoadStrategy = PageLoadStrategy.None
            };

            options.AddExcludedArgument("--enable-logging");
            options.AddArgument("--silent");
            options.AddArgument("--no-sandbox");
			options.AddArgument("--disable-translate");
			options.AddArgument("--disable-infobars");
			options.AddArgument("--disable-logging");

            if (isDebugModeEnabled)
            {
                options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1366,768");
                options.AddArgument("--start-maximized");
                options.AddArgument("--blink-settings=imagesEnabled=false");
                options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            }

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            ChromeDriver webDriver = new ChromeDriver(service, options, TimeSpan.FromSeconds(pageLoadTimeout));
            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)webDriver;
            string userAgent = (string)scriptExecutor.ExecuteScript("return navigator.userAgent;");

            if (userAgent.Contains("Headless"))
            {
                userAgent = userAgent.Replace("Headless", "");
                options.AddArgument($"--user-agent={userAgent}");

                webDriver.Quit();
                webDriver = new ChromeDriver(service, options);
            }

            webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);
            webDriver.Manage().Window.Maximize();

            return webDriver;
        }
    }
}
