using System;
using System.IO;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace NuciWeb
{
    public sealed class WebDriverInitialiser
    {
        /// <summary>
        /// Initialises a web driver based on the available browser drivers on the system.
        /// If geckodriver is found, it will initialise a Firefox driver; otherwise,
        /// it will initialise a Chrome driver.
        /// The driver will be configured based on the debug mode and page load timeout settings.
        /// </summary>
        /// <param name="isDebugModeEnabled">
        /// Indicates whether the driver should run in debug mode.
        /// If false, the driver will run in headless mode with image loading disabled.
        /// </param>
        /// <param name="pageLoadTimeout">
        /// The maximum time to wait for a page to load, in seconds.
        /// This will be applied to the driver settings.
        /// </param>
        /// <returns>
        /// An instance of <see cref="IWebDriver"/> that is configured based on the
        /// available browser drivers and the provided settings.
        /// If geckodriver is available, a Firefox driver will be returned; otherwise,
        /// a Chrome driver will be returned.
        /// </returns>
        public static IWebDriver InitialiseAvailableWebDriver(
            bool isDebugModeEnabled = true,
            int pageLoadTimeout = 90)
        {
            if (File.Exists("/usr/bin/geckodriver"))
            {
                return InitialiseFirefoxDriver(isDebugModeEnabled, pageLoadTimeout);
            }

            return InitialiseChromeDriver(isDebugModeEnabled, pageLoadTimeout);
        }

        /// <summary>
        /// Initialises a Firefox web driver with specified settings.
        /// The driver will be configured to run in headless mode if debug mode is disabled,
        /// and it will have a specified page load timeout.
        /// The driver will also be set to maximize the browser window upon initialization.
        /// </summary>
        /// <param name="isDebugModeEnabled">
        /// Indicates whether the driver should run in debug mode.
        /// If false, the driver will run in headless mode with image loading disabled.
        /// </param>
        /// <param name="pageLoadTimeout">
        /// The maximum time to wait for a page to load, in seconds.
        /// This will be applied to the driver settings.
        /// </param>
        /// <returns>
        /// An instance of <see cref="IWebDriver"/> configured for Firefox.
        /// The driver will be set to maximize the browser window and will have the specified
        /// page load timeout.
        /// If debug mode is disabled, the driver will run in headless mode with image loading
        /// disabled.
        /// </returns>
        public static IWebDriver InitialiseFirefoxDriver(
            bool isDebugModeEnabled = true,
            int pageLoadTimeout = 90)
        {
            FirefoxOptions options = new()
            {
                PageLoadStrategy = PageLoadStrategy.None
            };

            options.AddArgument("--disable-save-password-bubble");
            options.SetPreference("privacy.firstparty.isolate", false);

            if (!isDebugModeEnabled)
            {
                options.AddArgument("--headless");
                options.SetPreference("permissions.default.image", 2);
            }

            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            service.LogLevel = FirefoxDriverLogLevel.Error;

            FirefoxDriver driver = new(service, options, TimeSpan.FromSeconds(pageLoadTimeout));

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);
            driver.Manage().Window.Maximize();

            return driver;
        }

        /// <summary>
        /// Initialises a Chrome web driver with specified settings.
        /// The driver will be configured to run in headless mode if debug mode is disabled,
        /// and it will have a specified page load timeout.
        /// The driver will also be set to maximize the browser window upon initialization.
        /// Additionally, it will ensure that the user agent does not contain "Headless" to
        /// avoid issues with certain websites that may block headless browsers.
        /// </summary>
        /// <param name="isDebugModeEnabled">
        /// Indicates whether the driver should run in debug mode.
        /// If false, the driver will run in headless mode with image loading disabled.
        /// </param>
        /// <param name="pageLoadTimeout">
        /// The maximum time to wait for a page to load, in seconds.
        /// This will be applied to the driver settings.
        /// </param>
        /// <returns>
        /// An instance of <see cref="IWebDriver"/> configured for Chrome.
        /// The driver will be set to maximize the browser window and will have the specified
        /// page load timeout.
        /// If debug mode is disabled, the driver will run in headless mode with image loading
        /// disabled. Additionally, it will ensure that the user agent does not contain "Headless"
        /// to avoid issues with certain websites that may block headless browsers.
        /// </returns>
        public static IWebDriver InitialiseChromeDriver(
            bool isDebugModeEnabled = true,
            int pageLoadTimeout = 90)
        {
            ChromeOptions options = new()
            {
                PageLoadStrategy = PageLoadStrategy.None
            };

            options.AddExcludedArgument("--enable-logging");
            options.AddArgument("--silent");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-translate");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-logging");

            if (!isDebugModeEnabled)
            {
                options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--start-maximized");
                options.AddArgument("--blink-settings=imagesEnabled=false");
                options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            }

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            ChromeDriver webDriver = new(service, options, TimeSpan.FromSeconds(pageLoadTimeout));
            IJavaScriptExecutor scriptExecutor = webDriver;
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
