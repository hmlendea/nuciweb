using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NuciWeb.Utils;

namespace NuciWeb
{
    public abstract class WebProcessor() : IWebProcessor
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
            if (tab.Equals(CurrentTab))
            {
                return;
            }

            if (!Tabs.Contains(tab))
            {
                throw new ArgumentException("The specified tab does not belong to this processor.");
            }

            CurrentTab = tab;
            PerformSwitchToTab(tab);
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
            string tab = PerformNewTab(url);

            Tabs.Add(tab);

            SwitchToTab(tab);

            return tab;
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
                throw new ArgumentException("The specified tab does not belong to this processor.");
            }

            PerformCloseTab(tab);
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

            PerformGoToUrl(url, httpRetries, retryDelay);
        }

        /// <summary>
        /// Navigates to the specified iframe in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath for the iframe to navigate to.</param>
        public void GoToIframe(string xpath)
            => GoToUrl(GetSource(xpath));

        /// <summary>
        /// Switches to the specified iframe in the current tab of the web processor.
        /// </summary>
        /// <param name="index">The index of the iframe to switch to.</param>
        public void SwitchToIframe(int index)
        {
            SwitchToTab(CurrentTab);
            PerformSwitchToIframe(index);
        }

        /// <summary>
        /// Switches to the specified iframe in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath for the iframe to switch to.</param>
        public void SwitchToIframe(string xpath) => SwitchToIframe(xpath, DefaultTimeout);
        /// <summary>
        /// Switches to the specified iframe in the current tab of the web processor with a timeout.
        /// </summary>
        /// <param name="xpath">The XPath for the iframe to switch to.</param>
        /// <param name="timeout">The timeout for switching to the iframe.</param>
        public void SwitchToIframe(string xpath, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    PerformSwitchToIframe(xpath);
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
            => PerformRefresh();

        /// <summary>
        /// Executes a script in the context of the current tab in the web processor.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        public void ExecuteScript(string script)
        {
            SwitchToTab(CurrentTab);
            PerformExecuteScript(script);
        }

        /// <summary>
        /// Gets the value of a variable in the context of the current tab in the web processor.
        /// </summary>
        /// <param name="variableName">The name of the variable to get the value of.</param>
        /// <returns>The value of the variable.</returns>
        public string GetVariableValue(string variableName)
        {
            Wait();

            return PerformExecuteScript($"return {variableName};");
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
            => PerformAcceptAlert(timeout);

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
            => PerformDismissAlert(timeout);

        /// <summary>
        /// Gets the HTML source of the current page of the web processor.
        /// </summary>
        /// <returns>The HTML source of the current page.</returns>
        public string GetPageSource()
            => PerformGetPageSource();

        /// <summary>
        /// Gets the value of the specified attribute for the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="attribute">The name of the attribute to get the value of.</param>
        /// <returns>The value of the attribute for the first matching element.</returns>
        public string GetAttribute(string xpath, string attribute)
            => GetAttribute(xpath, attribute, DefaultTimeout);
        /// <summary>
        /// Gets the value of the specified attribute for the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="attribute">The name of the attribute to get the value of.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The value of the attribute for the first matching element.</returns>
        public string GetAttribute(string xpath, string attribute, bool retryOnDomFailure)
            => GetAttribute(xpath, attribute, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the value of the specified attribute for the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="attribute">The name of the attribute to get the value of.</param>
        /// <param name="timeout">The timeout for getting the attribute value.</param>
        /// <returns>The value of the attribute for the first matching element.</returns>
        public string GetAttribute(string xpath, string attribute, TimeSpan timeout)
            => GetAttribute(xpath, attribute, timeout, false);
        /// <summary>
        /// Gets the value of the specified attribute for the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="attribute">The name of the attribute to get the value of.</param>
        /// <param name="timeout">The timeout for getting the attribute value.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The value of the attribute for the first matching element.</returns>
        public string GetAttribute(string xpath, string attribute, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull(
                    this,
                    () => PerformGetAttribute(xpath, attribute, timeout).FirstOrDefault(),
                    timeout);
            }

            return PerformGetAttribute(xpath, attribute, timeout).FirstOrDefault();
        }

        /// <summary>
        /// Gets the values of the specified attribute for all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="attribute">The name of the attribute to get the values of.</param>
        /// <returns>A list of values of the attribute for all matching elements.</returns>
        public IList<string> GetAttributeOfMany(string xpath, string attribute)
            => GetAttributeOfMany(xpath, attribute, DefaultTimeout);
        /// <summary>
        /// Gets the values of the specified attribute for all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="attribute">The name of the attribute to get the values of.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of values of the attribute for all matching elements.</returns>
        public IList<string> GetAttributeOfMany(string xpath, string attribute, bool retryOnDomFailure)
            => GetAttributeOfMany(xpath, attribute, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the values of the specified attribute for all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="attribute">The name of the attribute to get the values of.</param>
        /// <param name="timeout">The timeout for getting the attribute values.</param>
        /// <returns>A list of values of the attribute for all matching elements.</returns>
        public IList<string> GetAttributeOfMany(string xpath, string attribute, TimeSpan timeout)
            => GetAttributeOfMany(xpath, attribute, timeout, retryOnDomFailure: false);
        /// <summary>
        /// Gets the values of the specified attribute for all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="attribute">The name of the attribute to get the values of.</param>
        /// <param name="timeout">The timeout for getting the attribute values.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of values of the attribute for all matching elements.</returns>
        public IList<string> GetAttributeOfMany(
            string xpath,
            string attribute,
            TimeSpan timeout,
            bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull<IList<string>>(
                    this,
                    () => [.. PerformGetAttribute(xpath, attribute, timeout)],
                    timeout);
            }

            return [.. PerformGetAttribute(xpath, attribute, timeout)];
        }

        /// <summary>
        /// Gets the class name of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>The class name of the first matching element.</returns>
        public string GetClass(string xpath)
            => GetClass(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the class name of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The class name of the first matching element.</returns>
        public string GetClass(string xpath, bool retryOnDomFailure)
            => GetClass(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the class name of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the class name.</param>
        /// <returns>The class name of the first matching element.</returns>
        public string GetClass(string xpath, TimeSpan timeout)
            => GetClass(xpath, timeout, retryOnDomFailure: false);
        /// <summary>
        /// Gets the class name of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the class name.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The class name of the first matching element.</returns>
        public string GetClass(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(xpath, "class", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the class names of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <returns>A list of class names of all matching elements.</returns>
        public IList<string> GetClassOfMany(string xpath)
            => GetClassOfMany(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the class names of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of class names of all matching elements.</returns>
        public IList<string> GetClassOfMany(string xpath, bool retryOnDomFailure)
            => GetClassOfMany(xpath, DefaultTimeout, retryOnDomFailure);

        /// <summary>
        /// Gets the class names of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the class names.</param>
        /// <returns>A list of class names of all matching elements.</returns>
        public IList<string> GetClassOfMany(string xpath, TimeSpan timeout)
            => GetClassOfMany(xpath, timeout, retryOnDomFailure: false);
        /// <summary>
        /// Gets the class names of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the class names.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of class names of all matching elements.</returns>
        public IList<string> GetClassOfMany(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(xpath, "class", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the class names of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>A list of class names of the first matching element.</returns>
        public IList<string> GetClasses(string xpath)
            => GetClasses(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the class names of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of class names of the first matching element.</returns>
        public IList<string> GetClasses(string xpath, bool retryOnDomFailure)
            => GetClasses(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the class names of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the class names.</param>
        /// <returns>A list of class names of the first matching element.</returns>
        public IList<string> GetClasses(string xpath, TimeSpan timeout)
            => GetClasses(xpath, timeout, retryOnDomFailure: false);
        /// <summary>
        /// Gets the class names of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the class names.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of class names of the first matching element.</returns>
        public IList<string> GetClasses(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetClass(xpath, timeout, retryOnDomFailure).Split(' ');

        /// <summary>
        /// Gets the hyperlink of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>The hyperlink of the first matching element.</returns>
        public string GetHyperlink(string xpath)
            => GetHyperlink(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the hyperlink of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The hyperlink of the first matching element.</returns>
        public string GetHyperlink(string xpath, bool retryOnDomFailure)
            => GetHyperlink(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the hyperlink of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the hyperlink.</param>
        /// <returns>The hyperlink of the first matching element.</returns>
        public string GetHyperlink(string xpath, TimeSpan timeout)
            => GetHyperlink(xpath, timeout, retryOnDomFailure: false);
        /// <summary>
        /// Gets the hyperlink of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the hyperlink.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The hyperlink of the first matching element.</returns>
        public string GetHyperlink(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(xpath, "href", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the hyperlinks of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <returns>A list of hyperlinks of all matching elements.</returns>
        public IList<string> GetHyperlinkOfMany(string xpath)
            => GetHyperlinkOfMany(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the hyperlinks of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of hyperlinks of all matching elements.</returns>
        public IList<string> GetHyperlinkOfMany(string xpath, bool retryOnDomFailure)
            => GetHyperlinkOfMany(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the hyperlinks of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the hyperlinks.</param>
        /// <returns>A list of hyperlinks of all matching elements.</returns>
        public IList<string> GetHyperlinkOfMany(string xpath, TimeSpan timeout)
            => GetHyperlinkOfMany(xpath, timeout, retryOnDomFailure: false);
        /// <summary>
        /// Gets the hyperlinks of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the hyperlinks.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of hyperlinks of all matching elements.</returns>
        public IList<string> GetHyperlinkOfMany(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(xpath, "href", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the source of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>The source of the first matching element.</returns>
        public string GetSource(string xpath)
            => GetSource(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the source of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The source of the first matching element.</returns>
        public string GetSource(string xpath, bool retryOnDomFailure)
            => GetSource(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the source of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the source.</param>
        /// <returns>The source of the first matching element.</returns>
        public string GetSource(string xpath, TimeSpan timeout)
            => GetSource(xpath, timeout, retryOnDomFailure: false);
        /// <summary>
        /// Gets the source of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the source.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The source of the first matching element.</returns>
        public string GetSource(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(xpath, "src", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the sources of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <returns>A list of sources of all matching elements.</returns>
        public IList<string> GetSourceOfMany(string xpath)
            => GetSourceOfMany(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the sources of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of sources of all matching elements.</returns>
        public IList<string> GetSourceOfMany(string xpath, bool retryOnDomFailure)
            => GetSourceOfMany(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the sources of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the sources.</param>
        /// <returns>A list of sources of all matching elements.</returns>
        public IList<string> GetSourceOfMany(string xpath, TimeSpan timeout)
            => GetSourceOfMany(xpath, timeout, retryOnDomFailure: false);
        /// <summary>
        /// Gets the sources of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the sources.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of sources of all matching elements.</returns>
        public IList<string> GetSourceOfMany(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(xpath, "src", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the style of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>The style of the first matching element.</returns>
        public string GetStyle(string xpath)
            => GetStyle(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the style of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The style of the first matching element.</returns>
        public string GetStyle(string xpath, bool retryOnDomFailure)
            => GetStyle(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the style of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the style.</param>
        /// <returns>The style of the first matching element.</returns>
        public string GetStyle(string xpath, TimeSpan timeout)
            => GetStyle(xpath, timeout, retryOnDomFailure: false);

        /// <summary>
        /// Gets the style of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the style.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The style of the first matching element.</returns>
        public string GetStyle(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(xpath, "style", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the styles of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <returns>A list of styles of all matching elements.</returns>
        public IList<string> GetStyleOfMany(string xpath)
            => GetStyleOfMany(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the styles of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of styles of all matching elements.</returns>
        public IList<string> GetStyleOfMany(string xpath, bool retryOnDomFailure)
            => GetStyleOfMany(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the styles of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the styles.</param>
        /// <returns>A list of styles of all matching elements.</returns>
        public IList<string> GetStyleOfMany(string xpath, TimeSpan timeout)
            => GetStyleOfMany(xpath, timeout, retryOnDomFailure: false);
        /// <summary>
        /// Gets the styles of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the styles.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of styles of all matching elements.</returns>
        public IList<string> GetStyleOfMany(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(xpath, "style", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the ID of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>The ID of the first matching element.</returns>
        public string GetId(string xpath)
            => GetId(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the ID of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The ID of the first matching element.</returns>
        public string GetId(string xpath, bool retryOnDomFailure)
            => GetId(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the ID of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the ID.</param>
        /// <returns>The ID of the first matching element.</returns>
        public string GetId(string xpath, TimeSpan timeout)
            => GetId(xpath, timeout, false);
        /// <summary>
        /// Gets the ID of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the ID.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The ID of the first matching element.</returns>
        public string GetId(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(xpath, "id", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the IDs of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <returns>A list of IDs of all matching elements.</returns>
        public IList<string> GetIdOfMany(string xpath)
            => GetIdOfMany(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the IDs of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of IDs of all matching elements.</returns>
        public IList<string> GetIdOfMany(string xpath, bool retryOnDomFailure)
            => GetIdOfMany(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the IDs of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the IDs.</param>
        /// <returns>A list of IDs of all matching elements.</returns>
        public IList<string> GetIdOfMany(string xpath, TimeSpan timeout)
            => GetIdOfMany(xpath, timeout, false);
        /// <summary>
        /// Gets the IDs of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the IDs.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of IDs of all matching elements.</returns>
        public IList<string> GetIdOfMany(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(xpath, "id", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the value of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>The value of the first matching element.</returns>
        public string GetValue(string xpath)
            => GetValue(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the value of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>The value of the first matching element.</returns>
        public string GetValue(string xpath, bool retryOnDomFailure)
            => GetValue(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the value of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the value.</param>
        /// <returns>The value of the first matching element.</returns>
        public string GetValue(string xpath, TimeSpan timeout)
            => GetValue(xpath, timeout, retryOnDomFailure: false);
        /// <summary>
        /// Gets the value of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the value.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The value of the first matching element.</returns>
        public string GetValue(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttribute(xpath, "value", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the values of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <returns>A list of values of all matching elements.</returns>
        public IList<string> GetValueOfMany(string xpath)
            => GetValueOfMany(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the values of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of values of all matching elements.</returns>
        public IList<string> GetValueOfMany(string xpath, bool retryOnDomFailure)
            => GetValueOfMany(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the values of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the values.</param>
        /// <returns>A list of values of all matching elements.</returns>
        public IList<string> GetValueOfMany(string xpath, TimeSpan timeout)
            => GetValueOfMany(xpath, timeout, false);
        /// <summary>
        /// Gets the values of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the values.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of values of all matching elements.</returns>
        public IList<string> GetValueOfMany(string xpath, TimeSpan timeout, bool retryOnDomFailure)
            => GetAttributeOfMany(xpath, "value", timeout, retryOnDomFailure);

        /// <summary>
        /// Gets the text of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>The text of the first matching element.</returns>
        public string GetText(string xpath)
            => GetText(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the text of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The text of the first matching element.</returns>
        public string GetText(string xpath, bool retryOnDomFailure)
            => GetText(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the text of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the text.</param>
        /// <returns>The text of the first matching element.</returns>
        public string GetText(string xpath, TimeSpan timeout)
            => GetText(xpath, timeout, false);
        /// <summary>
        /// Gets the text of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the text.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The text of the first matching element.</returns>
        public string GetText(string xpath, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull(
                    this,
                    () => PerformGetText(xpath, timeout).FirstOrDefault(),
                    timeout);
            }

            return PerformGetText(xpath, timeout).FirstOrDefault();
        }

        /// <summary>
        /// Gets the text of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <returns>A list of text of all matching elements.</returns>
        public IList<string> GetTextOfMany(string xpath)
            => GetTextOfMany(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the text of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of text of all matching elements.</returns>
        public IList<string> GetTextOfMany(string xpath, bool retryOnDomFailure)
            => GetTextOfMany(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the text of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the text.</param>
        /// <returns>A list of text of all matching elements.</returns>
        public IList<string> GetTextOfMany(string xpath, TimeSpan timeout)
            => GetTextOfMany(xpath, timeout, false);
        /// <summary>
        /// Gets the text of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the text.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of text of all matching elements.</returns>
        public IList<string> GetTextOfMany(string xpath, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull<IList<string>>(
                    this,
                    () => [.. PerformGetText(xpath, timeout)],
                    timeout);
            }

            return [.. PerformGetText(xpath, timeout)];
        }

        /// <summary>
        /// Gets the selected text of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>The selected text of the first matching element.</returns>
        public string GetSelectedText(string xpath)
            => GetSelectedText(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the selected text of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The selected text of the first matching element.</returns>
        public string GetSelectedText(string xpath, bool retryOnDomFailure)
            => GetSelectedText(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the selected text of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the selected text.</param>
        /// <returns>The selected text of the first matching element.</returns>
        public string GetSelectedText(string xpath, TimeSpan timeout)
            => GetSelectedText(xpath, timeout, false);
        /// <summary>
        /// Gets the selected text of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for getting the selected text.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>The selected text of the first matching element.</returns>
        public string GetSelectedText(string xpath, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull(
                    this,
                    () => PerformGetSelectedText(xpath, timeout).FirstOrDefault(),
                    timeout);
            }

            return PerformGetSelectedText(xpath, timeout).FirstOrDefault();
        }

        /// <summary>
        /// Gets the selected text of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <returns>A list of selected text of all matching elements.</returns>
        public IList<string> GetSelectedTextOfMany(string xpath)
            => GetSelectedTextOfMany(xpath, DefaultTimeout);
        /// <summary>
        /// Gets the selected text of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of selected text of all matching elements.</returns>
        public IList<string> GetSelectedTextOfMany(string xpath, bool retryOnDomFailure)
            => GetSelectedTextOfMany(xpath, DefaultTimeout, retryOnDomFailure);
        /// <summary>
        /// Gets the selected text of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the selected text.</param>
        /// <returns>A list of selected text of all matching elements.</returns>
        public IList<string> GetSelectedTextOfMany(string xpath, TimeSpan timeout)
            => GetSelectedTextOfMany(xpath, DefaultTimeout, false);
        /// <summary>
        /// Gets the selected text of all elements matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match elements against.</param>
        /// <param name="timeout">The timeout for getting the selected text.</param>
        /// <param name="retryOnDomFailure">Whether to retry on DOM failure.</param>
        /// <returns>A list of selected text of all matching elements.</returns>
        public IList<string> GetSelectedTextOfMany(string xpath, TimeSpan timeout, bool retryOnDomFailure)
        {
            if (retryOnDomFailure)
            {
                return ExecutionUtils.RetryUntilTheResultIsNotNull<IList<string>>(
                    this,
                    () => [.. PerformGetSelectedText(xpath, timeout)],
                    timeout);
            }

            return [.. PerformGetSelectedText(xpath, timeout)];
        }

        /// <summary>
        /// Sets the value of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="text">The text to set as the value.</param>
        public void SetText(string xpath, string text)
            => SetText(xpath, text, DefaultTimeout);
        /// <summary>
        /// Sets the value of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="text">The text to set as the value.</param>
        /// <param name="timeout">The timeout for setting the value.</param>
        public void SetText(string xpath, string text, TimeSpan timeout)
            => PerformSetText(xpath, text, timeout);

        /// <summary>
        /// Appends text to the value of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="text">The text to append to the value.</param>
        public void AppendText(string xpath, string text)
            => AppendText(xpath, text, DefaultTimeout);
        /// <summary>
        /// Appends text to the value of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="text">The text to append to the value.</param>
        /// <param name="timeout">The timeout for appending the text.</param>
        public void AppendText(string xpath, string text, TimeSpan timeout)
            => SetText(xpath, GetValue(xpath, timeout) + text, timeout);

        /// <summary>
        /// Clears the text of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        public void ClearText(string xpath)
            => ClearText(xpath, DefaultTimeout);
        /// <summary>
        /// Clears the text of the first element matching the XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for clearing the text.</param>
        public void ClearText(string xpath, TimeSpan timeout)
            => SetText(xpath, string.Empty, timeout);

        /// <summary>
        /// Checks if the first element matching the XPath has a specific class in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="className">The class name to check for.</param>
        /// <returns>True if the element has the class, false otherwise.</returns>
        public bool HasClass(string xpath, string className)
            => HasClass(xpath, className, DefaultTimeout);
        /// <summary>
        /// Checks if the first element matching the XPath has a specific class in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="className">The class name to check for.</param>
        /// <param name="timeout">The timeout for checking if the element has the class.</param>
        /// <returns>True if the element has the class, false otherwise.</returns>
        public bool HasClass(string xpath, string className, TimeSpan timeout)
            => GetClasses(xpath, timeout).Contains(className);

        /// <summary>
        /// Checks if the first element matching the XPath is selected in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>True if the element is selected, false otherwise.</returns>
        public bool IsSelected(string xpath)
            => IsSelected(xpath, DefaultTimeout);
        /// <summary>
        /// Checks if the first element matching the XPath is selected in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for checking if the element is selected.</param>
        /// <returns>True if the element is selected, false otherwise.</returns>
        public bool IsSelected(string xpath, TimeSpan timeout)
            => PerformIsSelected(xpath, timeout);

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

            Thread.Sleep(timeSpan);
        }

        /// <summary>
        /// Waits for the text length of the first element matching the XPath to reach a specified length in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="length">The length to wait for.</param>
        public void WaitForTextLength(string xpath, int length)
            => WaitForTextLength(xpath, length, DefaultTimeout);
        /// <summary>
        /// Waits for the text length of the first element matching the XPath to reach a specified length in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="length">The length to wait for.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForTextLength(string xpath, int length, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForTextLength(xpath, length, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForTextLength(xpath, length, DefaultTimeout);
            }
        }
        /// <summary>
        /// Waits for the text length of the first element matching the XPath to reach a specified length in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="length">The length to wait for.</param>
        /// <param name="timeout">The timeout for waiting for the text length.</param>
        public void WaitForTextLength(string xpath, int length, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                bool conditionMet =
                    GetValue(xpath, timeout).Length == length ||
                    GetText(xpath, timeout).Length == length;

                if (conditionMet)
                {
                    break;
                }

                Wait();
            }
        }

        /// <summary>
        /// Waits for any element matching the provided XPaths to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        public void WaitForAnyElementToExist(params string[] xpaths)
            => WaitForAnyElementToExist(DefaultTimeout, xpaths);
        /// <summary>
        /// Waits for any element matching the provided XPaths to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForAnyElementToExist(bool waitIndefinetely, params string[] xpaths)
        {
            if (waitIndefinetely)
            {
                WaitForAnyElementToExist(TimeSpan.FromDays(873), xpaths);
            }
            else
            {
                WaitForAnyElementToExist(DefaultTimeout, xpaths);
            }
        }
        /// <summary>
        /// Waits for any element matching the provided XPaths to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <param name="timeout">The timeout for waiting for any element to exist.</param>
        public void WaitForAnyElementToExist(TimeSpan timeout, params string[] xpaths)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoesAnyElementExist(xpaths))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for all elements matching the provided XPaths to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        public void WaitForAllElementsToExist(params string[] xpaths)
            => WaitForAllElementsToExist(DefaultTimeout, xpaths);
        /// <summary>
        /// Waits for all elements matching the provided XPaths to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForAllElementsToExist(bool waitIndefinetely, params string[] xpaths)
        {
            if (waitIndefinetely)
            {
                WaitForAllElementsToExist(TimeSpan.FromDays(873), xpaths);
            }
            else
            {
                WaitForAllElementsToExist(DefaultTimeout, xpaths);
            }
        }
        /// <summary>
        /// Waits for all elements matching the provided XPaths to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <param name="timeout">The timeout for waiting for all elements to exist.</param>
        public void WaitForAllElementsToExist(TimeSpan timeout, params string[] xpaths)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoAllElementsExist(xpaths))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for any element matching the provided XPaths to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        public void WaitForAnyElementToBeVisible(params string[] xpaths)
            => WaitForAnyElementToBeVisible(DefaultTimeout, xpaths);
        /// <summary>
        /// Waits for any element matching the provided XPaths to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForAnyElementToBeVisible(bool waitIndefinetely, params string[] xpaths)
        {
            if (waitIndefinetely)
            {
                WaitForAnyElementToBeVisible(TimeSpan.FromDays(873), xpaths);
            }
            else
            {
                WaitForAnyElementToBeVisible(DefaultTimeout, xpaths);
            }
        }
        /// <summary>
        /// Waits for any element matching the provided XPaths to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <param name="timeout">The timeout for waiting for any element to be visible.</param>
        public void WaitForAnyElementToBeVisible(TimeSpan timeout, params string[] xpaths)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !IsAnyElementVisible(xpaths))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for all elements matching the provided XPaths to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        public void WaitForAllElementsToBeVisible(params string[] xpaths)
            => WaitForAllElementsToBeVisible(DefaultTimeout, xpaths);
        /// <summary>
        /// Waits for all elements matching the provided XPaths to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForAllElementsToBeVisible(bool waitIndefinetely, params string[] xpaths)
        {
            if (waitIndefinetely)
            {
                WaitForAllElementsToBeVisible(TimeSpan.FromDays(873), xpaths);
            }
            else
            {
                WaitForAllElementsToBeVisible(DefaultTimeout, xpaths);
            }
        }
        /// <summary>
        /// Waits for all elements matching the provided XPaths to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <param name="timeout">The timeout for waiting for all elements to be visible.</param>
        public void WaitForAllElementsToBeVisible(TimeSpan timeout, params string[] xpaths)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !AreAllElementsVisible(xpaths))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for an element matching the provided XPath to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        public void WaitForElementToExist(string xpath)
            => WaitForElementToExist(xpath, DefaultTimeout);
        /// <summary>
        /// Waits for an element matching the provided XPath to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForElementToExist(string xpath, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForElementToExist(xpath, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForElementToExist(xpath, DefaultTimeout);
            }
        }
        /// <summary>
        /// Waits for an element matching the provided XPath to exist in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for waiting for the element to exist.</param>
        public void WaitForElementToExist(string xpath, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoesElementExist(xpath))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for an element matching the provided XPath to disappear in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        public void WaitForElementToDisappear(string xpath)
            => WaitForElementToDisappear(xpath, DefaultTimeout);
        /// <summary>
        /// Waits for an element matching the provided XPath to disappear in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForElementToDisappear(string xpath, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForElementToDisappear(xpath, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForElementToDisappear(xpath, DefaultTimeout);
            }
        }
        /// <summary>
        /// Waits for an element matching the provided XPath to disappear in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for waiting for the element to disappear.</param>
        public void WaitForElementToDisappear(string xpath, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && DoesElementExist(xpath))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for an element matching the provided XPath to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        public void WaitForElementToBeVisible(string xpath)
            => WaitForElementToBeVisible(xpath, DefaultTimeout);
        /// <summary>
        /// Waits for an element matching the provided XPath to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForElementToBeVisible(string xpath, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForElementToBeVisible(xpath, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForElementToBeVisible(xpath, DefaultTimeout);
            }
        }
        /// <summary>
        /// Waits for an element matching the provided XPath to be visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for waiting for the element to be visible.</param>
        public void WaitForElementToBeVisible(string xpath, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !IsElementVisible(xpath))
            {
                Wait();
            }
        }

        /// <summary>
        /// Waits for an element matching the provided XPath to be invisible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        public void WaitForElementToBeInvisible(string xpath)
            => WaitForElementToBeInvisible(xpath, DefaultTimeout);
        /// <summary>
        /// Waits for an element matching the provided XPath to be invisible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="waitIndefinetely">Whether to wait indefinitely.</param>
        public void WaitForElementToBeInvisible(string xpath, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForElementToBeInvisible(xpath, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForElementToBeInvisible(xpath, DefaultTimeout);
            }
        }
        /// <summary>
        /// Waits for an element matching the provided XPath to be invisible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for waiting for the element to be invisible.</param>
        public void WaitForElementToBeInvisible(string xpath, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && IsElementVisible(xpath))
            {
                Wait();
            }
        }

        /// <summary>
        /// Checks if all elements matching the provided XPaths exist in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <returns>True if all elements exist, false otherwise.</returns>
        public bool DoAllElementsExist(params string[] xpaths)
            => xpaths.All(DoesElementExist);
        /// <summary>
        /// Checks if any element matching the provided XPaths exists in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <returns>True if any element exists, false otherwise.</returns>
        public bool DoesAnyElementExist(params string[] xpaths)
            => xpaths.Any(DoesElementExist);

        /// <summary>
        /// Checks if an element matching the provided XPath exists in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>True if the element exists, false otherwise.</returns>
        public bool DoesElementExist(string xpath)
        {
            SwitchToTab(CurrentTab);

            return PerformDoesElementExist(xpath);
        }

        /// <summary>
        /// Checks if all elements matching the provided XPaths are visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <returns>True if all elements are visible, false otherwise.</returns>
        public bool AreAllElementsVisible(params string[] xpaths)
            => xpaths.All(IsElementVisible);

        /// <summary>
        /// Checks if any element matching the provided XPaths is visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        /// <returns>True if any element is visible, false otherwise.</returns>
        public bool IsAnyElementVisible(params string[] xpaths)
            => xpaths.Any(IsElementVisible);

        /// <summary>
        /// Checks if an element matching the provided XPath is visible in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <returns>True if the element is visible, false otherwise.</returns>
        public bool IsElementVisible(string xpath)
        {
            SwitchToTab(CurrentTab);

            if (!DoesElementExist(xpath))
            {
                return false;
            }

            return PerformIsElementVisible(xpath);
        }

        /// <summary>
        /// Moves the mouse cursor to the first element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        public void MoveToElement(string xpath)
            => MoveToElement(xpath, DefaultTimeout);
        /// <summary>
        /// Moves the mouse cursor to the first element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for moving to the element.</param>
        public void MoveToElement(string xpath, TimeSpan timeout)
            => PerformMoveToElement(xpath, timeout);

        /// <summary>
        /// Clicks on any of the elements matching the provided XPaths in the current tab of the web processor.
        /// </summary>
        /// <param name="xpaths">The XPaths to match elements against.</param>
        public void ClickAny(params string[] xpaths)
        {
            bool clicked = false;

            foreach (string xpath in xpaths)
            {
                if (IsElementVisible(xpath))
                {
                    Click(xpath);

                    clicked = true;
                    break;
                }
            }

            if (!clicked)
            {
                // TODO: Use a proper message
                throw new ArgumentException("No element to click.");
            }
        }

        /// <summary>
        /// Clicks on the first element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        public void Click(string xpath)
            => Click(xpath, DefaultTimeout);
        /// <summary>
        /// Clicks on the first element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="timeout">The timeout for clicking the element.</param>
        public void Click(string xpath, TimeSpan timeout)
            => PerformClick(xpath, timeout);

        /// <summary>
        /// Clicks on the first element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="status">The status to wait for after clicking.</param>
        public void UpdateCheckbox(string xpath, bool status)
            => UpdateCheckbox(xpath, status, DefaultTimeout);
        /// <summary>
        /// Clicks on the first element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the element against.</param>
        /// <param name="status">The status to wait for after clicking.</param>
        /// <param name="timeout">The timeout for clicking the element.</param>
        public void UpdateCheckbox(string xpath, bool status, TimeSpan timeout)
        {
            if (!PerformIsCheckboxChecked(xpath, timeout) == status)
            {
                Click(xpath, timeout);
            }
        }

        /// <summary>
        /// Selects an option by index in the first select element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the select element against.</param>
        /// <param name="index">The index of the option to select.</param>
        public void SelectOptionByIndex(string xpath, int index)
            => SelectOptionByIndex(xpath, index, DefaultTimeout);
        /// <summary>
        /// Selects an option by index in the first select element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the select element against.</param>
        /// <param name="index">The index of the option to select.</param>
        /// <param name="timeout">The timeout for selecting the option.</param>
        public void SelectOptionByIndex(string xpath, int index, TimeSpan timeout)
            => PerformSelectOptionByIndex(xpath, index, timeout);

        /// <summary>
        /// Selects an option by value in the first select element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the select element against.</param>
        /// <param name="value">The value of the option to select.</param>
        public void SelectOptionByValue(string xpath, object value)
            => SelectOptionByValue(xpath, value, DefaultTimeout);
        /// <summary>
        /// Selects an option by value in the first select element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the select element against.</param>
        /// <param name="value">The value of the option to select.</param>
        /// <param name="timeout">The timeout for selecting the option.</param>
        public void SelectOptionByValue(string xpath, object value, TimeSpan timeout)
        {
            string stringValue;

            if (value is string valueAsString)
            {
                stringValue = valueAsString;
            }
            else
            {
                stringValue = value.ToString();
            }

            PerformSelectOptionByValue(xpath, stringValue, timeout);
        }

        /// <summary>
        /// Selects an option by text in the first select element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the select element against.</param>
        /// <param name="text">The text of the option to select.</param>
        public void SelectOptionByText(string xpath, string text)
            => SelectOptionByText(xpath, text, DefaultTimeout);
        /// <summary>
        /// Selects an option by text in the first select element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the select element against.</param>
        /// <param name="text">The text of the option to select.</param>
        /// <param name="timeout">The timeout for selecting the option.</param>
        public void SelectOptionByText(string xpath, string text, TimeSpan timeout)
            => PerformSelectOptionByText(xpath, text, timeout);

        /// <summary>
        /// Selects a random option in the first select element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the select element against.</param>
        public void SelectRandomOption(string xpath)
            => SelectRandomOption(xpath, DefaultTimeout);
        /// <summary>
        /// Selects a random option in the first select element matching the provided XPath in the current tab of the web processor.
        /// </summary>
        /// <param name="xpath">The XPath to match the select element against.</param>
        /// <param name="timeout">The timeout for selecting the option.</param>
        public void SelectRandomOption(string xpath, TimeSpan timeout)
        {
            int optionsCount = PerformGetSelectOptionsCount(xpath, timeout);
            int option = Random.Next(0, optionsCount);

            SelectOptionByIndex(xpath, option, timeout);
        }

        protected abstract bool PerformDoesElementExist(string xpath);
        protected abstract bool PerformIsCheckboxChecked(string xpath, TimeSpan timeout);
        protected abstract bool PerformIsElementVisible(string xpath);
        protected abstract bool PerformIsSelected(string xpath, TimeSpan timeout);
        protected abstract IEnumerable<string> PerformGetAttribute(string xpath, string attribute, TimeSpan timeout);
        protected abstract IEnumerable<string> PerformGetSelectedText(string xpath, TimeSpan timeout);
        protected abstract IEnumerable<string> PerformGetText(string xpath, TimeSpan timeout);
        protected abstract int PerformGetSelectOptionsCount(string xpath, TimeSpan timeout);
        protected abstract string PerformExecuteScript(string script);
        protected abstract string PerformGetPageSource();
        protected abstract string PerformNewTab(string url);
        protected abstract void PerformAcceptAlert(TimeSpan timeout);
        protected abstract void PerformClick(string xpath, TimeSpan timeout);
        protected abstract void PerformCloseTab(string tab);
        protected abstract void PerformDismissAlert(TimeSpan timeout);
        protected abstract void PerformGoToUrl(string url, int httpRetries, TimeSpan retryDelay);
        protected abstract void PerformMoveToElement(string xpath, TimeSpan timeout);
        protected abstract void PerformRefresh();
        protected abstract void PerformSelectOptionByIndex(string xpath, int index, TimeSpan timeout);
        protected abstract void PerformSelectOptionByText(string xpath, string text, TimeSpan timeout);
        protected abstract void PerformSelectOptionByValue(string xpath, object value, TimeSpan timeout);
        protected abstract void PerformSetText(string xpath, string text, TimeSpan timeout);
        protected abstract void PerformSwitchToIframe(int index);
        protected abstract void PerformSwitchToIframe(string xpath);
        protected abstract void PerformSwitchToTab(string tab);
    }
}
