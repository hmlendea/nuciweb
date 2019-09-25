using System;
using System.Collections.Generic;
using System.Linq;

using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace NuciWeb
{
    public sealed class WebProcessor : IWebProcessor
    {
        public string Name => GetType().Name.Replace("Processor", string.Empty);

        public IList<string> Tabs { get; private set; }

        public IEnumerable<string> DriverWindowTabs => driver.WindowHandles;

        public string CurrentTab { get; private set; }

        public Random Random { get; private set; }

        static readonly TimeSpan DefaultWaitDuration = TimeSpan.FromMilliseconds(333);

        static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

        static readonly int DefaultHttpAttemptsAmount = 3;

        readonly IWebDriver driver;

        public WebProcessor(IWebDriver driver)
        {
            this.driver = driver;

            Random = new Random();
            Tabs = new List<string>();
        }

        ~WebProcessor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach(string tab in Tabs.ToList())
                {
                    CloseTab(tab);
                }

                driver.SwitchTo().Window(driver.WindowHandles[0]);
            }
        }

        public void SwitchToTab(int index) => SwitchToTab(Tabs[index]);
        public void SwitchToTab(string tab)
        {
            if (tab == driver.CurrentWindowHandle)
            {
                return;
            }

            if (!Tabs.Contains(tab))
            {
                throw new ArgumentOutOfRangeException("The specified tab does not belong to this processor");
            }
            
            CurrentTab = tab;
            driver.SwitchTo().Window(tab);
        }

        public string NewTab() => NewTab("about:blank");
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

            List<string> oldWindowTabs = driver.WindowHandles.ToList();

            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            scriptExecutor.ExecuteScript(newTabScript);

            List<string> newWindowTabs = driver.WindowHandles.ToList();
            string openedWindowTabs = newWindowTabs.Except(oldWindowTabs).Single();

            Tabs.Add(openedWindowTabs);

            SwitchToTab(openedWindowTabs);

            return openedWindowTabs;
        }

        public void CloseTab(string tab)
        {
            if (!Tabs.Contains(tab))
            {
                throw new ArgumentOutOfRangeException("The specified tab does not belong to this processor");
            }
            
            driver.SwitchTo().Window(tab).Close();
            Tabs.Remove(tab);
        }

        public void GoToUrl(string url) => GoToUrl(url, DefaultHttpAttemptsAmount);
        public void GoToUrl(string url, int httpRetries) => GoToUrl(url, httpRetries, DefaultWaitDuration);
        public void GoToUrl(string url, TimeSpan retryDelay) => GoToUrl(url, DefaultHttpAttemptsAmount, retryDelay);
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

            if (driver.Url == url)
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

        public void GoToIframe(By selector)
        {
            string iframeSource = GetSource(selector);
            GoToUrl(iframeSource);
        }

        public void SwitchToIframe(int index)
        {
            SwitchToTab(CurrentTab);
            driver.SwitchTo().Frame(index);
        }
        public void SwitchToIframe(By selector) => SwitchToIframe(selector, DefaultTimeout);
        public void SwitchToIframe(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    IWebElement iframe = GetElement(selector, timeout);
                    driver.SwitchTo().Frame(iframe);
                }
                finally
                {
                    Wait();
                }
            }
        }

        public void Refresh()
        {
            driver.Navigate().Refresh();
        }

        public void ExecuteScript(string script)
        {
            SwitchToTab(CurrentTab);

            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            scriptExecutor.ExecuteScript(script);
        }
        public string GetVariableValue(string variableName)
        {
            string script = $"return {variableName};";
            
            Wait();
            
            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            return (string)scriptExecutor.ExecuteScript(script);
        }

        public void AcceptAlert() => AcceptAlert(DefaultTimeout);
        public void AcceptAlert(TimeSpan timeout)
        {
            IAlert alert = GetAlert(timeout);
            alert.Accept();
        }

        public void DismissAlert() => DismissAlert(DefaultTimeout);
        public void DismissAlert(TimeSpan timeout)
        {
            IAlert alert = GetAlert(timeout);
            alert.Dismiss();
        }

        public string GetPageSource()
        {
            string oldHandle = driver.CurrentWindowHandle;
            
            SwitchToTab(CurrentTab);
            string source = driver.PageSource;

            driver.SwitchTo().Window(oldHandle);

            return source;
        }

        public IEnumerable<IWebElement> GetElements(By selector) => GetElements(selector, DefaultTimeout);
        public int GetElementsCount(By selector)
        {
            IEnumerable<IWebElement> elements = GetElements(selector);

            if (elements is null)
            {
                return 0;
            }

            return elements.Count();
        }

        public string GetAttribute(By selector, string attribute) => GetAttribute(selector, attribute, DefaultTimeout);
        public string GetAttribute(By selector, string attribute, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            return element.GetAttribute(attribute);
        }

        public IEnumerable<string> GetAttributeOfMany(By selector, string attribute) => GetAttributeOfMany(selector, attribute, DefaultTimeout);
        public IEnumerable<string> GetAttributeOfMany(By selector, string attribute, TimeSpan timeout)
        {
            IEnumerable<IWebElement> elements = GetElements(selector, timeout);
            IEnumerable<string> attributes = elements.Select(x => x.GetAttribute(attribute));

            return attributes;
        }

        public string GetClass(By selector) => GetClass(selector, DefaultTimeout);
        public string GetClass(By selector, TimeSpan timeout) => GetAttribute(selector, "class", timeout);

        public IEnumerable<string> GetClassOfMany(By selector) => GetClassOfMany(selector, DefaultTimeout);
        public IEnumerable<string> GetClassOfMany(By selector, TimeSpan timeout) => GetAttributeOfMany(selector, "class", timeout);

        public IEnumerable<string> GetClasses(By selector) => GetClasses(selector, DefaultTimeout);
        public IEnumerable<string> GetClasses(By selector, TimeSpan timeout) => GetAttribute(selector, "class", timeout).Split(' ');

        public string GetHyperlink(By selector) => GetHyperlink(selector, DefaultTimeout);
        public string GetHyperlink(By selector, TimeSpan timeout) => GetAttribute(selector, "href", timeout);

        public IEnumerable<string> GetHyperlinkOfMany(By selector) => GetHyperlinkOfMany(selector, DefaultTimeout);
        public IEnumerable<string> GetHyperlinkOfMany(By selector, TimeSpan timeout) => GetAttributeOfMany(selector, "href", timeout);

        public string GetSource(By selector) => GetSource(selector, DefaultTimeout);
        public string GetSource(By selector, TimeSpan timeout) => GetAttribute(selector, "src", timeout);

        public IEnumerable<string> GetSourceOfMany(By selector) => GetSourceOfMany(selector, DefaultTimeout);
        public IEnumerable<string> GetSourceOfMany(By selector, TimeSpan timeout) => GetAttributeOfMany(selector, "src", timeout);

        public string GetStyle(By selector) => GetStyle(selector, DefaultTimeout);
        public string GetStyle(By selector, TimeSpan timeout) => GetAttribute(selector, "style", timeout);

        public IEnumerable<string> GetStyleOfMany(By selector) => GetStyleOfMany(selector, DefaultTimeout);
        public IEnumerable<string> GetStyleOfMany(By selector, TimeSpan timeout) => GetAttributeOfMany(selector, "style", timeout);

        public string GetId(By selector) => GetId(selector, DefaultTimeout);
        public string GetId(By selector, TimeSpan timeout) => GetAttribute(selector, "id", timeout);

        public IEnumerable<string> GetIdOfMany(By selector) => GetIdOfMany(selector, DefaultTimeout);
        public IEnumerable<string> GetIdOfMany(By selector, TimeSpan timeout) => GetAttributeOfMany(selector, "id", timeout);

        public string GetValue(By selector) => GetValue(selector, DefaultTimeout);
        public string GetValue(By selector, TimeSpan timeout) => GetAttribute(selector, "value", timeout);

        public IEnumerable<string> GetValueOfMany(By selector) => GetValueOfMany(selector, DefaultTimeout);
        public IEnumerable<string> GetValueOfMany(By selector, TimeSpan timeout) => GetAttributeOfMany(selector, "value", timeout);

        public string GetText(By selector) => GetText(selector, DefaultTimeout);
        public string GetText(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            return element.Text;
        }

        public IEnumerable<string> GetTextOfMany(By selector) => GetTextOfMany(selector, DefaultTimeout);
        public IEnumerable<string> GetTextOfMany(By selector, TimeSpan timeout)
        {
            IEnumerable<IWebElement> elements = GetElements(selector, timeout);
            IEnumerable<string> texts = elements.Select(x => x.Text);

            return texts;
        }

        public string GetSelectedText(By selector) => GetSelectedText(selector, DefaultTimeout);
        public string GetSelectedText(By selector, TimeSpan timeout)
        {
            SelectElement element = GetSelectElement(selector, timeout);
            return element.SelectedOption.Text;
        }

        public IEnumerable<string> GetSelectedTextOfMany(By selector) => GetSelectedTextOfMany(selector, DefaultTimeout);
        public IEnumerable<string> GetSelectedTextOfMany(By selector, TimeSpan timeout)
        {
            IEnumerable<SelectElement> elements = GetSelectElements(selector, timeout);
            return elements.Select(x => x.SelectedOption.Text);
        }

        public void SetText(By selector, string text) => SetText(selector, text, DefaultTimeout);
        public void SetText(By selector, string text, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);

            element.Clear();
            element.SendKeys(text);
        }

        public void AppendText(By selector, string text) => AppendText(selector, text, DefaultTimeout);
        public void AppendText(By selector, string text, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            element.SendKeys(text);
        }

        public void ClearText(By selector) => ClearText(selector, DefaultTimeout);
        public void ClearText(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            element.Clear();
        }

        public bool HasClass(By selector, string className) => HasClass(selector, className, DefaultTimeout);
        public bool HasClass(By selector, string className, TimeSpan timeout)
        {
            IEnumerable<string> classes = GetClasses(selector, timeout);
            return classes.Contains(className);
        }

        public bool IsSelected(By selector) => IsSelected(selector, DefaultTimeout);
        public bool IsSelected(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            return element.Selected;
        }

        public void Wait() => Wait(DefaultWaitDuration);
        public void Wait(int milliseconds) => Wait(TimeSpan.FromMilliseconds(milliseconds));
        public void Wait(DateTime targetTime) => Wait(targetTime - DateTime.Now);
        public void Wait(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMilliseconds <= 0)
            {
                return;
            }

            DateTime now = DateTime.Now;
            WebDriverWait wait = new WebDriverWait(driver, timeSpan);
            wait.PollingInterval = TimeSpan.FromMilliseconds(10);
            wait.Until(wd=> (DateTime.Now - now) - timeSpan > TimeSpan.Zero);
        }

        public void WaitForTextLength(By selector, int length) => WaitForTextLength(selector, length, DefaultTimeout);
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

        public void WaitForAnyElementToExist(params By[] selectors) => WaitForAnyElementToExist(DefaultTimeout, selectors);
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
        public void WaitForAnyElementToExist(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoesAnyElementExist(selectors))
            {
                Wait();
            }
        }

        public void WaitForAllElementsToExist(params By[] selectors) => WaitForAllElementsToExist(DefaultTimeout, selectors);
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
        public void WaitForAllElementsToExist(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoAllElementsExist(selectors))
            {
                Wait();
            }
        }

        public void WaitForAnyElementToBeVisible(params By[] selectors) => WaitForAnyElementToBeVisible(DefaultTimeout, selectors);
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
        public void WaitForAnyElementToBeVisible(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !IsAnyElementVisible(selectors))
            {
                Wait();
            }
        }

        public void WaitForAllElementsToBeVisible(params By[] selectors) => WaitForAllElementsToBeVisible(DefaultTimeout, selectors);
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
        public void WaitForAllElementsToBeVisible(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !AreAllElementsVisible(selectors))
            {
                Wait();
            }
        }

        public void WaitForElementToExist(By selector) => WaitForElementToExist(selector, DefaultTimeout);
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
        public void WaitForElementToExist(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoesElementExist(selector))
            {
                Wait();
            }
        }

        public void WaitForElementToDisappear(By selector) => WaitForElementToDisappear(selector, DefaultTimeout);
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
        public void WaitForElementToDisappear(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && DoesElementExist(selector))
            {
                Wait();
            }
        }

        public void WaitForElementToBeVisible(By selector) => WaitForElementToBeVisible(selector, DefaultTimeout);
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
        public void WaitForElementToBeVisible(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !IsElementVisible(selector))
            {
                Wait();
            }
        }

        public void WaitForElementToBeInvisible(By selector) => WaitForElementToBeInvisible(selector, DefaultTimeout);
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
        public void WaitForElementToBeInvisible(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && IsElementVisible(selector))
            {
                Wait();
            }
        }

        public bool DoAllElementsExist(params By[] selectors) => selectors.All(DoesElementExist);
        public bool DoesAnyElementExist(params By[] selectors) => selectors.Any(DoesElementExist);
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

        public bool AreAllElementsVisible(params By[] selectors) => selectors.All(IsElementVisible);
        public bool IsAnyElementVisible(params By[] selectors) => selectors.Any(IsElementVisible);
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

        public void MoveToElement(By selector) => MoveToElement(selector, DefaultTimeout);
        public void MoveToElement(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);

            Actions actions = new Actions(driver);
            actions.MoveToElement(element);
            actions.Perform();
        }

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
                throw new ElementNotVisibleException("No element to click");
            }
        }

        public void Click(By selector) => Click(selector, DefaultTimeout);
        public void Click(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            element.Click();
        }

        public void UpdateCheckbox(By selector, bool status) => UpdateCheckbox(selector, status, DefaultTimeout);
        public void UpdateCheckbox(By selector, bool status, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);

            if (element.Selected != status)
            {
                Click(selector, timeout);
            }
        }

        public void SelectOptionByIndex(By selector, int index) => SelectOptionByIndex(selector, index, DefaultTimeout);
        public void SelectOptionByIndex(By selector, int index, TimeSpan timeout)
        {
            SelectElement element = GetSelectElement(selector, timeout);
            element.SelectByIndex(index);
        }

        public void SelectOptionByValue(By selector, object value) => SelectOptionByValue(selector, value, DefaultTimeout);
        public void SelectOptionByValue(By selector, object value, TimeSpan timeout)
        {
            SelectElement element = GetSelectElement(selector, timeout);

            string stringValue;

            if (value is string)
            {
                stringValue = (string)value;
            }
            else
            {
                stringValue = value.ToString();
            }

            element.SelectByValue(stringValue);
        }

        public void SelectOptionByText(By selector, string text) => SelectOptionByText(selector, text, DefaultTimeout);
        public void SelectOptionByText(By selector, string text, TimeSpan timeout)
        {
            SelectElement element = GetSelectElement(selector, timeout);

            element.SelectByText(text);
        }

        public void SelectRandomOption(By selector) => SelectRandomOption(selector, DefaultTimeout);
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
                    
                    if (element != null && element.Displayed)
                    {
                        return element;
                    }
                }
                catch {  }
                finally
                {
                    Wait();
                }
            }

            return null;
        }
        
        SelectElement GetSelectElement(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            SelectElement selectElement = new SelectElement(element);

            return selectElement;
        }

        IEnumerable<SelectElement> GetSelectElements(By selector, TimeSpan timeout)
        {
            IEnumerable<IWebElement> elements = GetElements(selector, timeout);
            IList<SelectElement> selectElements = new List<SelectElement>();

            foreach (IWebElement element in elements)
            {
                SelectElement selectElement = new SelectElement(element);
                selectElements.Add(selectElement);
            }

            return selectElements;
        }
        
        List<IWebElement> GetElements(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    List<IWebElement> elements = driver.FindElements(selector).ToList();

                    if (elements != null && elements.Count > 0)
                    {
                        return elements;
                    }
                }
                catch {  }
                finally
                {
                    Wait();
                }
            }

            return null;
        }
 
        IAlert GetAlert(TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    IAlert alert = driver.SwitchTo().Alert();
                    return alert;
                }
                catch {  }
                finally
                {
                    Wait();
                }
            }

            return null;
        }
    }
}
