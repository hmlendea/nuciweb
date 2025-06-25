using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NuciWeb.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace NuciWeb
{
    /// <summary>
    /// Represents a web processor that manages browser tabs and interactions with the web.
    /// This class provides methods to create, switch, and close tabs, navigate to URLs,
    /// execute scripts, handle alerts, and interact with web elements.
    /// It also provides functionality to manage iframes, retrieve page source, and get
    /// attributes of elements. The web processor is designed to work with a specific
    /// instance of <see cref="IWebDriver"/> and allows for easy management of multiple
    /// tabs within the same browser session.
    /// The processor maintains a list of tabs, the current tab, and provides methods
    /// to perform various operations on the web page, such as navigating to URLs,
    /// switching to iframes, and executing JavaScript. It also includes methods for
    /// retrieving element attributes, class names, hyperlinks, and handling alerts.
    /// The web processor is intended to be used in scenarios where browser automation
    /// is required, such as web scraping, automated testing, or browser-based tasks
    /// in a .NET application. It abstracts the complexity of managing browser tabs and
    /// provides a simple interface for developers to interact with web pages.
    /// The class implements the <see cref="IWebProcessor"/> interface, ensuring that it
    /// provides the necessary methods for managing web interactions in a consistent manner.
    /// </summary>
    /// <param name="driver">
    /// The <see cref="IWebDriver"/> instance that this processor will use to interact
    /// with the web browser. This driver is responsible for controlling the browser
    /// and executing commands such as navigating to URLs, switching tabs, and interacting
    /// with web elements.
    /// </param>
    public sealed class WebProcessor(IWebDriver driver) : IWebProcessor
    {
        /// <summary>
        /// Gets the name of the web processor.
        /// </summary>
        public string Name => GetType().Name.Replace("Processor", string.Empty);

        /// <summary>
        /// Gets the list of tabs currently managed by this web processor.
        /// Each tab is represented by its window handle.
        /// </summary>
        public IList<string> Tabs { get; private set; } = [];

        /// <summary>
        /// Gets the list of all tabs (window handles) currently managed by the driver.
        /// This includes all tabs, not just those managed by this processor.
        /// </summary>
        public IList<string> DriverWindowTabs => driver.WindowHandles;

        /// <summary>
        /// Gets the current tab (window handle) that this processor is working with.
        /// </summary>
        public string CurrentTab { get; private set; }

        /// <summary>
        /// Gets the random number generator used by this processor.
        /// This can be used for generating random values, such as for random delays or selections.
        ///
        /// </summary>
        public Random Random { get; private set; } = new Random();

        /// <summary>
        /// The default duration to wait for certain operations, such as element visibility or existence.
        /// </summary>
        static readonly TimeSpan DefaultWaitDuration = TimeSpan.FromMilliseconds(333);

        /// <summary>
        /// The default timeout for operations that require waiting, such as loading a page or waiting for an element.
        /// This is used to ensure that operations do not hang indefinitely and have a reasonable timeout.
        /// The default value is set to 20 seconds.
        /// </summary>
        static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

        /// <summary>
        /// The default number of attempts to retry HTTP requests when navigating to a URL.
        /// This is used to handle transient network issues or server errors that may occur when trying to
        /// load a page. The default value is set to 3 attempts.
        /// </summary>
        static readonly int DefaultHttpAttemptsAmount = 3;

        readonly IWebDriver driver = driver;

        ~WebProcessor() => Dispose(false);

        /// <summary>
        /// Disposes the web processor, closing all tabs and switching back to the first tab.
        /// This method is called when the processor is no longer needed, ensuring that all resources are
        /// released properly.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            foreach (string tab in Tabs.ToList())
            {
                CloseTab(tab);
            }

            driver.SwitchTo().Window(driver.WindowHandles[0]);
        }

        /// <summary>
        /// Switches to the specified tab.
        /// </summary>
        /// <param name="index">The index of the tab to switch to.</param>
        public void SwitchToTab(int index) => SwitchToTab(Tabs[index]);
        /// <summary>
        /// Switches to the specified tab.
        /// </summary>
        /// <param name="tab">The tab to switch to.</param>
        public void SwitchToTab(string tab)
        {
            if (tab.Equals(driver.CurrentWindowHandle))
            {
                return;
            }

            if (!Tabs.Contains(tab))
            {
                throw new ArgumentException("The specified tab does not belong to this processor");
            }

            CurrentTab = tab;
            driver.SwitchTo().Window(tab);
        }

        /// <summary>
        /// Creates a new tab in the web processor.
        /// </summary>
        /// <returns>The new tab.</returns>
        public string NewTab() => NewTab("about:blank");
        /// <summary>
        /// Creates a new tab in the web processor with the specified URL.
        /// </summary>
        /// <param name="url">The URL to open in the new tab.</param>
        /// <returns>The new tab.</returns>
        public string NewTab(string url)
        {
            driver.SwitchTo().Window(driver.WindowHandles[0]);

            // TODO: This is not covered by the retry mechanism
            string newTabScript =
                "var d=document,a=d.createElement('a');" +
                "a.target='_blank';a.href='" + url + "';" +
                "a.innerHTML='new tab';" +
                "d.body.appendChild(a);" +
                "a.click();" +
                "a.parentNode.removeChild(a);";

            IList<string> oldWindowTabs = [.. driver.WindowHandles];

            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            scriptExecutor.ExecuteScript(newTabScript);

            IList<string> newWindowTabs = [.. driver.WindowHandles];
            string openedWindowTabs = newWindowTabs.Except(oldWindowTabs).Single();

            Tabs.Add(openedWindowTabs);

            SwitchToTab(openedWindowTabs);

            return openedWindowTabs;
        }

        /// <summary>
        /// Closes the current tab in the web processor.
        /// </summary>
        public void CloseTab() => CloseTab(CurrentTab);

        /// <summary>
        /// Closes the specified tab in the web processor.
        /// </summary>
        /// <param name="tab">The tab to close.</param>
        public void CloseTab(string tab)
        {
            if (!Tabs.Contains(tab))
            {
                throw new ArgumentException("The specified tab does not belong to this processor");
            }

            driver.SwitchTo().Window(tab).Close();
            Tabs.Remove(tab);
        }

        /// <summary>
        /// Navigates to the specified URL in the current tab of the web processor.
        /// </summary>
        /// <param name="url">The URL to navigate to.</param>
        public void GoToUrl(string url) => GoToUrl(url, DefaultHttpAttemptsAmount);

        /// <summary>
        /// Navigates to the specified URL in the current tab of the web processor.
        /// </summary>
        /// <param name="url">The URL to navigate to.</param>
        /// <param name="httpRetries">The number of HTTP retries to attempt if the request fails.</param>
        public void GoToUrl(string url, int httpRetries) => GoToUrl(url, httpRetries, DefaultWaitDuration);

        /// <summary>
        /// Navigates to the specified URL in the current tab of the web processor.
        /// </summary>
        /// <param name="url">The URL to navigate to.</param>
        /// <param name="retryDelay">The delay to wait before retrying the request if it fails.</param>
        public void GoToUrl(string url, TimeSpan retryDelay) => GoToUrl(url, DefaultHttpAttemptsAmount, retryDelay);

        /// <summary>
        /// Navigates to the specified URL in the current tab of the web processor.
        /// </summary>
        /// <param name="url">The URL to navigate to.</param>
        /// <param name="httpRetries">The number of HTTP retries to attempt if the request fails.</param>
        /// <param name="retryDelay">The delay to wait before retrying the request if it fails.</param>
        public void GoToUrl(string url, int httpRetries, TimeSpan retryDelay)
        {
            if (string.IsNullOrWhiteSpace(CurrentTab))
            {
                NewTab();
            }
            else
            {
                SwitchToTab(CurrentTab);
            }

            if (driver.Url.Equals(url))
            {
                return;
            }

            By errorSelectorChrome = By.ClassName("error-code");
            By anythingSelector = By.XPath(@"/html/body/*");

            for (int attempt = 0; attempt < httpRetries; attempt++)
            {
                driver.Navigate().GoToUrl(url);

                for (int i = 0; i < 3; i++)
                {
                    WaitForElementToExist(anythingSelector);
                    if (DoesElementExist(anythingSelector))
                    {
                        break;
                    }

                    driver.Navigate().GoToUrl(url);
                }

                if (!IsAnyElementVisible(errorSelectorChrome))
                {
                    return;
                }

                GoToUrl("about:blank");
                Wait(retryDelay);
            }

            throw new Exception($"Failed to load the requested URL after {httpRetries} attempts");
        }

        /// <summary>
        /// Navigates to the specified iframe in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector for the iframe to navigate to.</param>
        public void GoToIframe(By selector)
            => GoToUrl(GetSource(selector));

        /// <summary>
        /// Switches to the specified iframe in the current tab of the web processor.
        /// </summary>
        /// <param name="index">The index of the iframe to switch to.</param>
        public void SwitchToIframe(int index)
        {
            SwitchToTab(CurrentTab);
            driver.SwitchTo().Frame(index);
        }

        /// <summary>
        /// Switches to the specified iframe in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector for the iframe to switch to.</param>
        public void SwitchToIframe(By selector) => SwitchToIframe(selector, DefaultTimeout);

        /// <summary>
        /// Switches to the specified iframe in the current tab of the web processor with a timeout.
        /// </summary>
        /// <param name="selector">The selector for the iframe to switch to.</param>
        /// <param name="timeout">The timeout for switching to the iframe.</param>
        public void SwitchToIframe(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    driver.SwitchTo().Frame(GetElement(selector, timeout));
                }
                finally
                {
                    Wait();
                }
            }
        }

        /// <summary>
        /// Refreshes the current tab in the web processor.
        /// </summary>
        public void Refresh()
            => driver.Navigate().Refresh();

        /// <summary>
        /// Executes a script in the context of the current tab in the web processor.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        public void ExecuteScript(string script)
        {
            SwitchToTab(CurrentTab);

            ((IJavaScriptExecutor)driver).ExecuteScript(script);
        }

        /// <summary>
        /// Gets the value of a variable in the context of the current tab in the web processor.
        /// </summary>
        /// <param name="variableName">The name of the variable to get the value of.</param>
        /// <returns>The value of the variable.</returns>
        public string GetVariableValue(string variableName)
        {
            string script = $"return {variableName};";

            Wait();

            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            return (string)scriptExecutor.ExecuteScript(script);
        }

        /// <summary>
        /// Accepts the current alert in the web processor.
        /// </summary>
        public void AcceptAlert()
            => AcceptAlert(DefaultTimeout);
        /// <summary>
        /// Accepts the current alert in the web processor.
        /// </summary>
        /// <param name="timeout">The timeout for accepting the alert.</param>
        public void AcceptAlert(TimeSpan timeout)
            => GetAlert(timeout).Accept();

        /// <summary>
        /// Dismisses the current alert in the web processor.
        /// </summary>
        public void DismissAlert()
            => DismissAlert(DefaultTimeout);
        /// <summary>
        /// Dismisses the current alert in the web processor.
        /// </summary>
        /// <param name="timeout">The timeout for dismissing the alert.</param>
        public void DismissAlert(TimeSpan timeout)
            => GetAlert(timeout).Dismiss();

        /// <summary>
        /// Gets the HTML source of the current page of the web processor.
        /// </summary>
        /// <returns>The HTML source of the current page.</returns>
        public string GetPageSource()
        {
            string oldHandle = driver.CurrentWindowHandle;

            SwitchToTab(CurrentTab);
            string source = driver.PageSource;

            driver.SwitchTo().Window(oldHandle);

            return source;
        }

        /// <summary>
        /// Gets the elements matching the specified selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <returns>A list of elements matching the selector.</returns>
        public IList<IWebElement> GetElements(By selector)
            => GetElements(selector, DefaultTimeout);

        /// <summary>
        /// Gets the elements matching the specified selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <returns>The number of elements matching the selector.</returns>
        public int GetElementsCount(By selector)
        {
            IList<IWebElement> elements = GetElements(selector);

            if (elements is null)
            {
                return 0;
            }

            return elements.Count;
        }

        /// <summary>
        /// Gets the value of the specified attribute for the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="attribute">The name of the attribute to get the value of.</param>
        /// <returns>The value of the attribute for the first matching element.</returns>
        public string GetAttribute(By selector, string attribute)
            => GetAttribute(selector, attribute, DefaultTimeout);

        /// <summary>
        /// Gets the value of the specified attribute for the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="attribute">The name of the attribute to get the value of.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The value of the attribute for the first matching element.</returns>
        public string GetAttribute(By selector, string attribute, bool retryOnDomFailure)
            => GetAttribute(selector, attribute, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the value of the specified attribute for the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="attribute">The name of the attribute to get the value of.</param>
        /// <param name="timeout">The timeout for getting the attribute value.</param>
        /// <returns>The value of the attribute for the first matching element.</returns>
        public string GetAttribute(By selector, string attribute, TimeSpan timeout)
            => GetAttribute(selector, attribute, DefaultTimeout, false);

        /// <summary>
        /// Gets the value of the specified attribute for the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="attribute">The name of the attribute to get the value of.</param>
        /// <param name="timeout">The timeout for getting the attribute value.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>The value of the attribute for the first matching element.</returns>
        public string GetAttribute(By selector, string attribute, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull(
                    this,
                    () => GetElement(selector, timeout).GetAttribute(attribute),
                    timeout);
            }

            return GetElement(selector, timeout).GetAttribute(attribute);
        }

        /// <summary>
        /// Gets the values of the specified attribute for all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="attribute">The name of the attribute to get the values of.</param>
        /// <returns>A list of values of the attribute for all matching elements.</returns>
        public IList<string> GetAttributeOfMany(By selector, string attribute)
            => GetAttributeOfMany(selector, attribute, DefaultTimeout);

        /// <summary>
        /// Gets the values of the specified attribute for all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="attribute">The name of the attribute to get the values of.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of values of the attribute for all matching elements.</returns>
        public IList<string> GetAttributeOfMany(By selector, string attribute, bool retryOnDomFailure)
            => GetAttributeOfMany(selector, attribute, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the values of the specified attribute for all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="attribute">The name of the attribute to get the values of.</param>
        /// <param name="timeout">The timeout for getting the attribute values.</param>
        /// <returns>A list of values of the attribute for all matching elements.</returns>
        public IList<string> GetAttributeOfMany(By selector, string attribute, TimeSpan timeout)
            => GetAttributeOfMany(selector, attribute, timeout, false);

        /// <summary>
        /// Gets the values of the specified attribute for all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="attribute">The name of the attribute to get the values of.</param>
        /// <param name="timeout">The timeout for getting the attribute values.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>A list of values of the attribute for all matching elements.</returns>
        public IList<string> GetAttributeOfMany(By selector, string attribute, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull<IList<string>>(
                    this,
                    () => [.. GetElements(selector, timeout).Select(x => x.GetAttribute(attribute))],
                    timeout);
            }

            return [.. GetElements(selector, timeout).Select(x => x.GetAttribute(attribute))];
        }

        /// <summary>
        /// Gets the class name of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>The class name of the first matching element.</returns>
        public string GetClass(By selector)
            => GetClass(selector, DefaultTimeout);

        /// <summary>
        /// Gets the class name of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The class name of the first matching element.</returns>
        public string GetClass(By selector, bool retryOnDomFailure)
            => GetClass(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the class name of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the class name.</param>
        /// <returns>The class name of the first matching element.</returns>
        public string GetClass(By selector, TimeSpan timeout)
            => GetClass(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the class name of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the class name.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>The class name of the first matching element.</returns>
        public string GetClass(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(selector, "class", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the class names of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <returns>A list of class names of all matching elements.</returns>
        public IList<string> GetClassOfMany(By selector)
            => GetClassOfMany(selector, DefaultTimeout);

        /// <summary>
        /// Gets the class names of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of class names of all matching elements.</returns>
        public IList<string> GetClassOfMany(By selector, bool retryOnDomFailure)
            => GetClassOfMany(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the class names of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the class names.</param>
        /// <returns>A list of class names of all matching elements.</returns>
        public IList<string> GetClassOfMany(By selector, TimeSpan timeout)
            => GetClassOfMany(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the class names of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the class names.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>A list of class names of all matching elements.</returns>
        public IList<string> GetClassOfMany(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(selector, "class", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the class names of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>A list of class names of the first matching element.</returns>
        public IList<string> GetClasses(By selector)
            => GetClasses(selector, DefaultTimeout);

        /// <summary>
        /// Gets the class names of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of class names of the first matching element.</returns>
        public IList<string> GetClasses(By selector, bool retryOnDomFailure)
            => GetClasses(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the class names of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the class names.</param>
        /// <returns>A list of class names of the first matching element.</returns>
        public IList<string> GetClasses(By selector, TimeSpan timeout)
            => GetClasses(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the class names of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the class names.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>A list of class names of the first matching element.</returns>
        public IList<string> GetClasses(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetClass(selector, timeout, retryOnDomFailure).Split(' ');

        /// <summary>
        /// Gets the hyperlink of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>The hyperlink of the first matching element.</returns>
        public string GetHyperlink(By selector)
            => GetHyperlink(selector, DefaultTimeout);

        /// <summary>
        /// Gets the hyperlink of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The hyperlink of the first matching element.</returns>
        public string GetHyperlink(By selector, bool retryOnDomFailure)
            => GetHyperlink(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the hyperlink of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the hyperlink.</param>
        /// <returns>The hyperlink of the first matching element.</returns>
        public string GetHyperlink(By selector, TimeSpan timeout)
            => GetHyperlink(selector, timeout, false);

        /// <summary>
        /// Gets the hyperlink of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the hyperlink.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>The hyperlink of the first matching element.</returns>
        public string GetHyperlink(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(selector, "href", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the hyperlinks of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <returns>A list of hyperlinks of all matching elements.</returns>
        public IList<string> GetHyperlinkOfMany(By selector)
            => GetHyperlinkOfMany(selector, DefaultTimeout);

        /// <summary>
        /// Gets the hyperlinks of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of hyperlinks of all matching elements.</returns>
        public IList<string> GetHyperlinkOfMany(By selector, bool retryOnDomFailure)
            => GetHyperlinkOfMany(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the hyperlinks of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the hyperlinks.</param>
        /// <returns>A list of hyperlinks of all matching elements.</returns>
        public IList<string> GetHyperlinkOfMany(By selector, TimeSpan timeout)
            => GetHyperlinkOfMany(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the hyperlinks of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the hyperlinks.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>A list of hyperlinks of all matching elements.</returns>
        public IList<string> GetHyperlinkOfMany(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(selector, "href", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the source of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>The source of the first matching element.</returns>
        public string GetSource(By selector)
            => GetSource(selector, DefaultTimeout);

        /// <summary>
        /// Gets the source of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The source of the first matching element.</returns>
        public string GetSource(By selector, bool retryOnDomFailure)
            => GetSource(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the source of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the source.</param>
        /// <returns>The source of the first matching element.</returns>
        public string GetSource(By selector, TimeSpan timeout)
            => GetSource(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the source of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the source.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>The source of the first matching element.</returns>
        public string GetSource(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(selector, "src", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the sources of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <returns>A list of sources of all matching elements.</returns>
        public IList<string> GetSourceOfMany(By selector)
            => GetSourceOfMany(selector, DefaultTimeout);

        /// <summary>
        /// Gets the sources of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of sources of all matching elements.</returns>
        public IList<string> GetSourceOfMany(By selector, bool retryOnDomFailure)
            => GetSourceOfMany(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the sources of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the sources.</param>
        /// <returns>A list of sources of all matching elements.</returns>
        public IList<string> GetSourceOfMany(By selector, TimeSpan timeout)
            => GetSourceOfMany(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the sources of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the sources.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>A list of sources of all matching elements.</returns>
        public IList<string> GetSourceOfMany(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(selector, "src", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the style of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>The style of the first matching element.</returns>
        public string GetStyle(By selector)
            => GetStyle(selector, DefaultTimeout);

        /// <summary>
        /// Gets the style of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The style of the first matching element.</returns>
        public string GetStyle(By selector, bool retryOnDomFailure)
            => GetStyle(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the style of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the style.</param>
        /// <returns>The style of the first matching element.</returns>
        public string GetStyle(By selector, TimeSpan timeout)
            => GetStyle(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the style of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the style.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>The style of the first matching element.</returns>
        public string GetStyle(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(selector, "style", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the styles of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <returns>A list of styles of all matching elements.</returns>
        public IList<string> GetStyleOfMany(By selector)
            => GetStyleOfMany(selector, DefaultTimeout);

        /// <summary>
        /// Gets the styles of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of styles of all matching elements.</returns>
        public IList<string> GetStyleOfMany(By selector, bool retryOnDomFailure)
            => GetStyleOfMany(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the styles of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the styles.</param>
        /// <returns>A list of styles of all matching elements.</returns>
        public IList<string> GetStyleOfMany(By selector, TimeSpan timeout)
            => GetStyleOfMany(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the styles of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the styles.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>A list of styles of all matching elements.</returns>
        public IList<string> GetStyleOfMany(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(selector, "style", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the ID of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>The ID of the first matching element.</returns>
        public string GetId(By selector)
            => GetId(selector, DefaultTimeout);

        /// <summary>
        /// Gets the ID of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The ID of the first matching element.</returns>
        public string GetId(By selector, bool retryOnDomFailure)
            => GetId(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the ID of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the ID.</param>
        /// <returns>The ID of the first matching element.</returns>
        public string GetId(By selector, TimeSpan timeout)
            => GetId(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the ID of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the ID.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>The ID of the first matching element.</returns>
        public string GetId(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(selector, "id", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the IDs of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <returns>A list of IDs of all matching elements.</returns>
        public IList<string> GetIdOfMany(By selector)
            => GetIdOfMany(selector, DefaultTimeout);

        /// <summary>
        /// Gets the IDs of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of IDs of all matching elements.</returns>
        public IList<string> GetIdOfMany(By selector, bool retryOnDomFailure)
            => GetIdOfMany(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the IDs of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the IDs.</param>
        /// <returns>A list of IDs of all matching elements.</returns>
        public IList<string> GetIdOfMany(By selector, TimeSpan timeout)
            => GetIdOfMany(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the IDs of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the IDs.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>A list of IDs of all matching elements.</returns>
        public IList<string> GetIdOfMany(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(selector, "id", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the value of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>The value of the first matching element.</returns>
        public string GetValue(By selector)
            => GetValue(selector, DefaultTimeout);

        /// <summary>
        /// Gets the value of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>The value of the first matching element.</returns>
        public string GetValue(By selector, bool retryOnDomFailure)
            => GetValue(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the value of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the value.</param>
        /// <returns>The value of the first matching element.</returns>
        public string GetValue(By selector, TimeSpan timeout)
            => GetValue(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the value of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the value.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>The value of the first matching element.</returns>
        public string GetValue(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(selector, "value", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the values of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <returns>A list of values of all matching elements.</returns>
        public IList<string> GetValueOfMany(By selector)
            => GetValueOfMany(selector, DefaultTimeout);

        /// <summary>
        /// Gets the values of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of values of all matching elements.</returns>
        public IList<string> GetValueOfMany(By selector, bool retryOnDomFailure)
            => GetValueOfMany(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the values of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the values.</param>
        /// <returns>A list of values of all matching elements.</returns>
        public IList<string> GetValueOfMany(By selector, TimeSpan timeout)
            => GetValueOfMany(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the values of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the values.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>A list of values of all matching elements.</returns>
        public IList<string> GetValueOfMany(By selector, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(selector, "value", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the text of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>The text of the first matching element.</returns>
        public string GetText(By selector)
            => GetText(selector, DefaultTimeout);

        /// <summary>
        /// Gets the text of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The text of the first matching element.</returns>
        public string GetText(By selector, bool retryOnDomFailure)
            => GetText(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the text of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the text.</param>
        /// <returns>The text of the first matching element.</returns>
        public string GetText(By selector, TimeSpan timeout)
            => GetText(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the text of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the text.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>The text of the first matching element.</returns>
        public string GetText(By selector, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull(
                    this,
                    () => GetElement(selector, timeout).Text,
                    timeout);
            }

            return GetElement(selector, timeout).Text;
        }

        /// <summary>
        /// Gets the text of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <returns>A list of text of all matching elements.</returns>
        public IList<string> GetTextOfMany(By selector)
            => GetTextOfMany(selector, DefaultTimeout);

        /// <summary>
        /// Gets the text of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of text of all matching elements.</returns>
        public IList<string> GetTextOfMany(By selector, bool retryOnDomFailure)
            => GetTextOfMany(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the text of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the text.</param>
        /// <returns>A list of text of all matching elements.</returns>
        public IList<string> GetTextOfMany(By selector, TimeSpan timeout)
            => GetTextOfMany(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the text of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the text.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>A list of text of all matching elements.</returns>
        public IList<string> GetTextOfMany(By selector, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull<IList<string>>(
                    this,
                    () => [.. GetElements(selector, timeout).Select(x => x.Text)],
                    timeout);
            }

            return [.. GetElements(selector, timeout).Select(x => x.Text)];
        }

        /// <summary>
        /// Gets the selected text of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>The selected text of the first matching element.</returns>
        public string GetSelectedText(By selector)
            => GetSelectedText(selector, DefaultTimeout);

        /// <summary>
        /// Gets the selected text of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The selected text of the first matching element.</returns>
        public string GetSelectedText(By selector, bool retryOnDomFailure)
            => GetSelectedText(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the selected text of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the selected text.</param>
        /// <returns>The selected text of the first matching element.</returns>
        public string GetSelectedText(By selector, TimeSpan timeout)
            => GetSelectedText(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the selected text of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for getting the selected text.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>The selected text of the first matching element.</returns>
        public string GetSelectedText(By selector, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull(
                    this,
                    () => GetSelectElement(selector, timeout).SelectedOption.Text,
                    timeout);
            }

            return GetSelectElement(selector, timeout).SelectedOption.Text;
        }

        /// <summary>
        /// Gets the selected text of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <returns>A list of selected text of all matching elements.</returns>
        public IList<string> GetSelectedTextOfMany(By selector)
            => GetSelectedTextOfMany(selector, DefaultTimeout);

        /// <summary>
        /// Gets the selected text of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of selected text of all matching elements.</returns>
        public IList<string> GetSelectedTextOfMany(By selector, bool retryOnDomFailure)
            => GetSelectedTextOfMany(selector, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the selected text of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the selected text.</param>
        /// <returns>A list of selected text of all matching elements.</returns>
        public IList<string> GetSelectedTextOfMany(By selector, TimeSpan timeout)
            => GetSelectedTextOfMany(selector, DefaultTimeout, false);

        /// <summary>
        /// Gets the selected text of all elements matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match elements against.</param>
        /// <param name="timeout">The timeout for getting the selected text.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param
        /// <returns>A list of selected text of all matching elements.</returns>
        public IList<string> GetSelectedTextOfMany(By selector, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull<IList<string>>(
                    this,
                    () => [.. GetSelectElements(selector, timeout).Select(x => x.SelectedOption.Text)],
                    timeout);
            }

            return [.. GetSelectElements(selector, timeout).Select(x => x.SelectedOption.Text)];
        }

        /// <summary>
        /// Sets the value of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="text">The text to set as the value.</param>
        public void SetText(By selector, string text)
            => SetText(selector, text, DefaultTimeout);

        /// <summary>
        /// Sets the value of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="text">The text to set as the value.</param>
        /// <param name="timeout">The timeout for setting the value.</param>
        public void SetText(By selector, string text, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);

            element.Clear();
            element.SendKeys(text);
        }

        /// <summary>
        /// Appends text to the value of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="text">The text to append to the value.</param>
        public void AppendText(By selector, string text)
            => AppendText(selector, text, DefaultTimeout);

        /// <summary>
        /// Appends text to the value of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="text">The text to append to the value.</param>
        /// <param name="timeout">The timeout for appending the text.</param>
        public void AppendText(By selector, string text, TimeSpan timeout)
            => GetElement(selector, timeout).SendKeys(text);

        /// <summary>
        /// Clears the text of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        public void ClearText(By selector)
            => ClearText(selector, DefaultTimeout);

        /// <summary>
        /// Clears the text of the first element matching the selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for clearing the text.</param>
        public void ClearText(By selector, TimeSpan timeout)
            => GetElement(selector, timeout).Clear();

        /// <summary>
        /// Checks if the first element matching the selector has a specific class in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="className">The class name to check for.</param>
        /// <returns>True if the element has the class, false otherwise.</returns>
        public bool HasClass(By selector, string className)
            => HasClass(selector, className, DefaultTimeout);

        /// <summary>
        /// Checks if the first element matching the selector has a specific class in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="className">The class name to check for.</param>
        /// <param name="timeout">The timeout for checking if the element has the class.</param>
        /// <returns>True if the element has the class, false otherwise.</returns>
        public bool HasClass(By selector, string className, TimeSpan timeout)
            => GetClasses(selector, timeout).Contains(className);

        /// <summary>
        /// Checks if the first element matching the selector is selected in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>True if the element is selected, false otherwise.</returns>
        public bool IsSelected(By selector)
            => IsSelected(selector, DefaultTimeout);

        /// <summary>
        /// Checks if the first element matching the selector is selected in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for checking if the element is selected.</param>
        /// <returns>True if the element is selected, false otherwise.</returns>
        public bool IsSelected(By selector, TimeSpan timeout)
            => GetElement(selector, timeout).Selected;

        /// <summary>
        /// Waits for the default amount of time.
        /// </summary>
        public void Wait()
            => Wait(DefaultWaitDuration);

        /// <summary>
        /// Waits for a specified number of milliseconds.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to wait.</param>
        public void Wait(int milliseconds)
            => Wait(TimeSpan.FromMilliseconds(milliseconds));

        /// <summary>
        /// Waits until the specified target time is reached.
        /// </summary>
        /// <param name="targetTime">The target time to wait until.</param>
        public void Wait(DateTime targetTime)
            => Wait(targetTime - DateTime.Now);

        /// <summary>
        /// Waits for a specified time span.
        /// </summary>
        /// <param name="timeSpan">The time span to wait for.</param>
        public void Wait(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMilliseconds <= 0)
            {
                return;
            }

            DateTime now = DateTime.Now;
            WebDriverWait wait = new(driver, timeSpan)
            {
                PollingInterval = TimeSpan.FromMilliseconds(10)
            };

            wait.Until(wd => DateTime.Now - now - timeSpan > TimeSpan.Zero);
        }

        /// <summary>
        /// Waits for the text length of the first element matching the selector to reach a specified length in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="length">The length to wait for.</param>
        public void WaitForTextLength(By selector, int length)
            => WaitForTextLength(selector, length, DefaultTimeout);

        /// <summary>
        /// Waits for the text length of the first element matching the selector to reach a specified length in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="length">The length to wait for.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForTextLength(By selector, int length, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForTextLength(selector, length, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForTextLength(selector, length, DefaultTimeout);
            }
        }

        /// <summary>
        /// Waits for the text length of the first element matching the selector to reach a specified length in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="length">The length to wait for.</param>
        /// <param name="timeout">The timeout for waiting for the text length.</param>
        public void WaitForTextLength(By selector, int length, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                bool conditionMet =
                    GetValue(selector, timeout).Length == length ||
                    GetText(selector, timeout).Length == length;

                if (conditionMet)
                {
                    break;
                }

                Wait();
            }
        }

        /// <summary>
        /// Waits for any element matching the provided selectors to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        public void WaitForAnyElementToExist(params By[] selectors)
            => WaitForAnyElementToExist(DefaultTimeout, selectors);

        /// <summary>
        /// Waits for any element matching the provided selectors to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForAnyElementToExist(bool waitIndefinetely, params By[] selectors)
        {
            if (waitIndefinetely)
            {
                WaitForAnyElementToExist(TimeSpan.FromDays(873), selectors);
            }
            else
            {
                WaitForAnyElementToExist(DefaultTimeout, selectors);
            }
        }

        /// <summary>
        /// Waits for any element matching the provided selectors to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <param name="timeout">The timeout for waiting for any element to exist.</param>
        public void WaitForAnyElementToExist(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoesAnyElementExist(selectors))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for all elements matching the provided selectors to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        public void WaitForAllElementsToExist(params By[] selectors)
            => WaitForAllElementsToExist(DefaultTimeout, selectors);

        /// <summary>
        /// Waits for all elements matching the provided selectors to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForAllElementsToExist(bool waitIndefinetely, params By[] selectors)
        {
            if (waitIndefinetely)
            {
                WaitForAllElementsToExist(TimeSpan.FromDays(873), selectors);
            }
            else
            {
                WaitForAllElementsToExist(DefaultTimeout, selectors);
            }
        }

        /// <summary>
        /// Waits for all elements matching the provided selectors to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <param name="timeout">The timeout for waiting for all elements to exist.</param>
        public void WaitForAllElementsToExist(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoAllElementsExist(selectors))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for any element matching the provided selectors to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        public void WaitForAnyElementToBeVisible(params By[] selectors)
            => WaitForAnyElementToBeVisible(DefaultTimeout, selectors);

        /// <summary>
        /// Waits for any element matching the provided selectors to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForAnyElementToBeVisible(bool waitIndefinetely, params By[] selectors)
        {
            if (waitIndefinetely)
            {
                WaitForAnyElementToBeVisible(TimeSpan.FromDays(873), selectors);
            }
            else
            {
                WaitForAnyElementToBeVisible(DefaultTimeout, selectors);
            }
        }

        /// <summary>
        /// Waits for any element matching the provided selectors to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <param name="timeout">The timeout for waiting for any element to be visible.</param>
        public void WaitForAnyElementToBeVisible(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !IsAnyElementVisible(selectors))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for all elements matching the provided selectors to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        public void WaitForAllElementsToBeVisible(params By[] selectors)
            => WaitForAllElementsToBeVisible(DefaultTimeout, selectors);

        /// <summary>
        /// Waits for all elements matching the provided selectors to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForAllElementsToBeVisible(bool waitIndefinetely, params By[] selectors)
        {
            if (waitIndefinetely)
            {
                WaitForAllElementsToBeVisible(TimeSpan.FromDays(873), selectors);
            }
            else
            {
                WaitForAllElementsToBeVisible(DefaultTimeout, selectors);
            }
        }

        /// <summary>
        /// Waits for all elements matching the provided selectors to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <param name="timeout">The timeout for waiting for all elements to be visible.</param>
        public void WaitForAllElementsToBeVisible(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !AreAllElementsVisible(selectors))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for an element matching the provided selector to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        public void WaitForElementToExist(By selector)
            => WaitForElementToExist(selector, DefaultTimeout);

        /// <summary>
        /// Waits for an element matching the provided selector to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForElementToExist(By selector, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForElementToExist(selector, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForElementToExist(selector, DefaultTimeout);
            }
        }

        /// <summary>
        /// Waits for an element matching the provided selector to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for waiting for the element to exist.</param>
        public void WaitForElementToExist(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoesElementExist(selector))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for an element matching the provided selector to disappear in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        public void WaitForElementToDisappear(By selector)
            => WaitForElementToDisappear(selector, DefaultTimeout);

        /// <summary>
        /// Waits for an element matching the provided selector to disappear in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForElementToDisappear(By selector, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForElementToDisappear(selector, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForElementToDisappear(selector, DefaultTimeout);
            }
        }

        /// <summary>
        /// Waits for an element matching the provided selector to disappear in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for waiting for the element to disappear.</param>
        public void WaitForElementToDisappear(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && DoesElementExist(selector))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for an element matching the provided selector to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        public void WaitForElementToBeVisible(By selector)
            => WaitForElementToBeVisible(selector, DefaultTimeout);

        /// <summary>
        /// Waits for an element matching the provided selector to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForElementToBeVisible(By selector, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForElementToBeVisible(selector, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForElementToBeVisible(selector, DefaultTimeout);
            }
        }

        /// <summary>
        /// Waits for an element matching the provided selector to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for waiting for the element to be visible.</param>
        public void WaitForElementToBeVisible(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !IsElementVisible(selector))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for an element matching the provided selector to be invisible in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        public void WaitForElementToBeInvisible(By selector)
            => WaitForElementToBeInvisible(selector, DefaultTimeout);

        /// <summary>
        /// Waits for an element matching the provided selector to be invisible in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForElementToBeInvisible(By selector, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForElementToBeInvisible(selector, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForElementToBeInvisible(selector, DefaultTimeout);
            }
        }

        /// <summary>
        /// Waits for an element matching the provided selector to be invisible in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for waiting for the element to be invisible.</param>
        public void WaitForElementToBeInvisible(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && IsElementVisible(selector))
            {
                Wait();
            }
        }

        /// <summary>
        /// Checks if all elements matching the provided selectors exist in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <returns>True if all elements exist, false otherwise.</returns>
        public bool DoAllElementsExist(params By[] selectors)
            => selectors.All(DoesElementExist);

        /// <summary>
        /// Checks if any element matching the provided selectors exists in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <returns>True if any element exists, false otherwise.</returns>
        public bool DoesAnyElementExist(params By[] selectors)
            => selectors.Any(DoesElementExist);

        /// <summary>
        /// Checks if an element matching the provided selector exists in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>True if the element exists, false otherwise.</returns>
        public bool DoesElementExist(By selector)
        {
            SwitchToTab(CurrentTab);

            try
            {
                IWebElement element = driver.FindElement(selector);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if all elements matching the provided selectors are visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <returns>True if all elements are visible, false otherwise.</returns>
        public bool AreAllElementsVisible(params By[] selectors)
            => selectors.All(IsElementVisible);

        /// <summary>
        /// Checks if any element matching the provided selectors is visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        /// <returns>True if any element is visible, false otherwise.</returns>
        public bool IsAnyElementVisible(params By[] selectors)
            => selectors.Any(IsElementVisible);

        /// <summary>
        /// Checks if an element matching the provided selector is visible in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <returns>True if the element is visible, false otherwise.</returns>
        public bool IsElementVisible(By selector)
        {
            SwitchToTab(CurrentTab);

            try
            {
                IWebElement element = driver.FindElement(selector);
                return element.Displayed;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Moves the mouse cursor to the first element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        public void MoveToElement(By selector)
            => MoveToElement(selector, DefaultTimeout);

        /// <summary>
        /// Moves the mouse cursor to the first element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for moving to the element.</param>
        public void MoveToElement(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);

            Actions actions = new(driver);
            actions.MoveToElement(element);
            actions.Perform();
        }

        /// <summary>
        /// Clicks on any of the elements matching the provided selectors in the current tab of the web processor.
        /// </summary>
        /// <param name="selectors">The selectors to match elements against.</param>
        public void ClickAny(params By[] selectors)
        {
            bool clicked = false;

            foreach (By selector in selectors)
            {
                if (IsElementVisible(selector))
                {
                    Click(selector);

                    clicked = true;
                    break;
                }
            }

            if (!clicked)
            {
                // TODO: Use a proper message
                throw new NoSuchElementException("No element to click");
            }
        }

        /// <summary>
        /// Clicks on the first element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        public void Click(By selector)
            => Click(selector, DefaultTimeout);

        /// <summary>
        /// Clicks on the first element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="timeout">The timeout for clicking the element.</param>
        public void Click(By selector, TimeSpan timeout)
            => GetElement(selector, timeout).Click();

        /// <summary>
        /// Clicks on the first element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="status">The status to wait for after clicking.</param>
        public void UpdateCheckbox(By selector, bool status)
            => UpdateCheckbox(selector, status, DefaultTimeout);

        /// <summary>
        /// Clicks on the first element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the element against.</param>
        /// <param name="status">The status to wait for after clicking.</param>
        /// <param name="timeout">The timeout for clicking the element.</param>
        public void UpdateCheckbox(By selector, bool status, TimeSpan timeout)
        {
            if (!GetElement(selector, timeout).Selected.Equals(status))
            {
                Click(selector, timeout);
            }
        }

        /// <summary>
        /// Selects an option by index in the first select element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the select element against.</param>
        /// <param name="index">The index of the option to select.</param>
        public void SelectOptionByIndex(By selector, int index)
            => SelectOptionByIndex(selector, index, DefaultTimeout);

        /// <summary>
        /// Selects an option by index in the first select element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the select element against.</param>
        /// <param name="index">The index of the option to select.</param>
        /// <param name="timeout">The timeout for selecting the option.</param>
        public void SelectOptionByIndex(By selector, int index, TimeSpan timeout)
            => GetSelectElement(selector, timeout).SelectByIndex(index);

        /// <summary>
        /// Selects an option by value in the first select element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the select element against.</param>
        /// <param name="value">The value of the option to select.</param>
        public void SelectOptionByValue(By selector, object value)
            => SelectOptionByValue(selector, value, DefaultTimeout);

        /// <summary>
        /// Selects an option by value in the first select element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the select element against.</param>
        /// <param name="value">The value of the option to select.</param>
        /// <param name="timeout">The timeout for selecting the option.</param>
        public void SelectOptionByValue(By selector, object value, TimeSpan timeout)
        {
            SelectElement element = GetSelectElement(selector, timeout);

            string stringValue;

            if (value is string valueAsString)
            {
                stringValue = valueAsString;
            }
            else
            {
                stringValue = value.ToString();
            }

            element.SelectByValue(stringValue);
        }

        /// <summary>
        /// Selects an option by text in the first select element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the select element against.</param>
        /// <param name="text">The text of the option to select.</param>
        public void SelectOptionByText(By selector, string text)
            => SelectOptionByText(selector, text, DefaultTimeout);

        /// <summary>
        /// Selects an option by text in the first select element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the select element against.</param>
        /// <param name="text">The text of the option to select.</param>
        /// <param name="timeout">The timeout for selecting the option.</param>
        public void SelectOptionByText(By selector, string text, TimeSpan timeout)
            => GetSelectElement(selector, timeout).SelectByText(text);

        /// <summary>
        /// Selects a random option in the first select element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the select element against.</param>
        public void SelectRandomOption(By selector)
            => SelectRandomOption(selector, DefaultTimeout);

        /// <summary>
        /// Selects a random option in the first select element matching the provided selector in the current tab of the web processor.
        /// </summary>
        /// <param name="selector">The selector to match the select element against.</param>
        /// <param name="timeout">The timeout for selecting the option.</param>
        public void SelectRandomOption(By selector, TimeSpan timeout)
        {
            SelectElement element = GetSelectElement(selector, timeout);

            int option = Random.Next(0, element.Options.Count);
            element.SelectByIndex(option);
        }

        IWebElement GetElement(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    IWebElement element = driver.FindElement(selector);

                    if (element is not null && element.Displayed)
                    {
                        return element;
                    }
                }
                catch { }
                finally
                {
                    Wait();
                }
            }

            throw new NotFoundException($"No element with the `{selector.Mechanism} {selector.Criteria}` exists!");
        }

        SelectElement GetSelectElement(By selector, TimeSpan timeout)
            => new(GetElement(selector, timeout));

        IList<SelectElement> GetSelectElements(By selector, TimeSpan timeout)
        {
            IList<IWebElement> elements = GetElements(selector, timeout);
            IList<SelectElement> selectElements = [];

            foreach (IWebElement element in elements)
            {
                SelectElement selectElement = new(element);
                selectElements.Add(selectElement);
            }

            return selectElements;
        }

        ReadOnlyCollection<IWebElement> GetElements(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    ReadOnlyCollection<IWebElement> elements = driver.FindElements(selector);

                    if (elements is not null && elements.Count > 0)
                    {
                        return elements;
                    }
                }
                catch { }
                finally
                {
                    Wait();
                }
            }

            throw new NotFoundException($"No elements with the `{selector.Mechanism} {selector.Criteria}` exist!");
        }

        IAlert GetAlert(TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    return driver.SwitchTo().Alert();
                }
                catch { }
                finally
                {
                    Wait();
                }
            }

            return null;
        }
    }
}
