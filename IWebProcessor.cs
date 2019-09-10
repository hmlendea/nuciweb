using System;
using System.Collections.Generic;

using OpenQA.Selenium;

namespace NuciWeb
{
    public interface IWebProcessor : IDisposable
    {
        string Name { get; }

        IList<string> Tabs { get; }

        IEnumerable<string> DriverWindowTabs { get; }

        void SwitchToTab(int index);
        void SwitchToTab(string tab);

        string NewTab();
        string NewTab(string url);

        void CloseTab(string tab);

        void GoToUrl(string url);
        void GoToUrl(string url, int httpRetries);
        void GoToUrl(string url, TimeSpan retryDelay);
        void GoToUrl(string url, int httpRetries, TimeSpan retryDelay);

        void GoToIframe(By selector);

        void SwitchToIframe(int index);
        void SwitchToIframe(By selector);
        void SwitchToIframe(By selector, TimeSpan timeout);

        void Refresh();

        void ExecuteScript(string script);
        string GetVariableValue(string variableName);

        void AcceptAlert();
        void AcceptAlert(TimeSpan timeout);

        void DismissAlert();
        void DismissAlert(TimeSpan timeout);

        string GetPageSource();

        IEnumerable<IWebElement> GetElements(By selector);
        int GetElementsCount(By selector);

        string GetAttribute(By selector, string attribute);
        string GetAttribute(By selector, string attribute, TimeSpan timeout);

        IEnumerable<string> GetAttributeOfMany(By selector, string attribute);
        IEnumerable<string> GetAttributeOfMany(By selector, string attribute, TimeSpan timeout);

        string GetClass(By selector);
        string GetClass(By selector, TimeSpan timeout);

        IEnumerable<string> GetClassOfMany(By selector);
        IEnumerable<string> GetClassOfMany(By selector, TimeSpan timeout);

        IEnumerable<string> GetClasses(By selector);
        IEnumerable<string> GetClasses(By selector, TimeSpan timeout);

        string GetHyperlink(By selector);
        string GetHyperlink(By selector, TimeSpan timeout);

        IEnumerable<string> GetHyperlinkOfMany(By selector);
        IEnumerable<string> GetHyperlinkOfMany(By selector, TimeSpan timeout);

        string GetSource(By selector);
        string GetSource(By selector, TimeSpan timeout);

        IEnumerable<string> GetSourceOfMany(By selector);
        IEnumerable<string> GetSourceOfMany(By selector, TimeSpan timeout);
        
        string GetStyle(By selector);
        string GetStyle(By selector, TimeSpan timeout);

        IEnumerable<string> GetStyleOfMany(By selector);
        IEnumerable<string> GetStyleOfMany(By selector, TimeSpan timeout);

        string GetId(By selector);
        string GetId(By selector, TimeSpan timeout);

        IEnumerable<string> GetIdOfMany(By selector);
        IEnumerable<string> GetIdOfMany(By selector, TimeSpan timeout);

        string GetValue(By selector);
        string GetValue(By selector, TimeSpan timeout);

        IEnumerable<string> GetValueOfMany(By selector);
        IEnumerable<string> GetValueOfMany(By selector, TimeSpan timeout);

        string GetText(By selector);
        string GetText(By selector, TimeSpan timeout);

        IEnumerable<string> GetTextOfMany(By selector);
        IEnumerable<string> GetTextOfMany(By selector, TimeSpan timeout);

        string GetSelectedText(By selector);
        string GetSelectedText(By selector, TimeSpan timeout);

        IEnumerable<string> GetSelectedTextOfMany(By selector);
        IEnumerable<string> GetSelectedTextOfMany(By selector, TimeSpan timeout);

        void SetText(By selector, string text);
        void SetText(By selector, string text, TimeSpan timeout);

        void AppendText(By selector, string text);
        void AppendText(By selector, string text, TimeSpan timeout);

        void ClearText(By selector);
        void ClearText(By selector, TimeSpan timeout);

        bool HasClass(By selector, string className);
        bool HasClass(By selector, string className, TimeSpan timeout);

        bool IsSelected(By selector);
        bool IsSelected(By selector, TimeSpan timeout);

        void Wait();
        void Wait(int milliseconds);
        void Wait(DateTime targetTime);
        void Wait(TimeSpan timeSpan);

        void WaitForTextLength(By selector, int length);
        void WaitForTextLength(By selector, int length, bool waitIndefinetely);
        void WaitForTextLength(By selector, int length, TimeSpan timeout);

        void WaitForAnyElementToExist(params By[] selectors);
        void WaitForAnyElementToExist(bool waitIndefinetely, params By[] selectors);
        void WaitForAnyElementToExist(TimeSpan timeout, params By[] selectors);

        void WaitForAllElementsToExist(params By[] selectors);
        void WaitForAllElementsToExist(bool waitIndefinetely, params By[] selectors);
        void WaitForAllElementsToExist(TimeSpan timeout, params By[] selectors);

        void WaitForAnyElementToBeVisible(params By[] selectors);
        void WaitForAnyElementToBeVisible(bool waitIndefinetely, params By[] selectors);
        void WaitForAnyElementToBeVisible(TimeSpan timeout, params By[] selectors);

        void WaitForAllElementsToBeVisible(params By[] selectors);
        void WaitForAllElementsToBeVisible(bool waitIndefinetely, params By[] selectors);
        void WaitForAllElementsToBeVisible(TimeSpan timeout, params By[] selectors);

        void WaitForElementToExist(By selector);
        void WaitForElementToExist(By selector, bool waitIndefinetely);
        void WaitForElementToExist(By selector, TimeSpan timeout);

        void WaitForElementToBeVisible(By selector);
        void WaitForElementToBeVisible(By selector, bool waitIndefinetely);
        void WaitForElementToBeVisible(By selector, TimeSpan timeout);

        bool DoAllElementsExist(params By[] selectors);
        bool DoesAnyElementExist(params By[] selectors);
        bool DoesElementExist(By selector);

        bool AreAllElementsVisible(params By[] selectors);
        bool IsAnyElementVisible(params By[] selectors);
        bool IsElementVisible(By selector);

        void ClickAny(params By[] selectors);

        void Click(By selector);
        void Click(By selector, TimeSpan timeout);

        void UpdateCheckbox(By selector, bool status);
        void UpdateCheckbox(By selector, bool status, TimeSpan timeout);

        void SelectOptionByIndex(By selector, int index);
        void SelectOptionByIndex(By selector, int index, TimeSpan timeout);

        void SelectOptionByValue(By selector, object value);
        void SelectOptionByValue(By selector, object value, TimeSpan timeout);

        void SelectOptionByText(By selector, string text);
        void SelectOptionByText(By selector, string text, TimeSpan timeout);

        void SelectRandomOption(By selector);
        void SelectRandomOption(By selector, TimeSpan timeout);
    }
}
