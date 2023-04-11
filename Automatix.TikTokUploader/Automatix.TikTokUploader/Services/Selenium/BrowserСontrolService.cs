using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Automatix.TikTokUploader.Constants;
using OpenQA.Selenium;

namespace Automatix.TikTokUploader.Services.Selenium;

public class BrowserСontrolService
{
    private readonly IWebDriver _driver;

    public BrowserСontrolService(IWebDriver driver)
    {
        _driver = driver;
    }

    public void Quit()
    {
        _driver.Quit();
    }

    public ReadOnlyCollection<IWebElement> FindElementsByXpath(string elementXpath)
    {
        var elements = _driver.FindElements(By.XPath(elementXpath));

        return elements;
    }

    public IWebElement FindElementByXpath(string elementXpath)
    {
        var element = _driver.FindElement(By.XPath(elementXpath));

        return element;
    }

    public IWebElement FindElementById(string elementId)
    {
        var element = _driver.FindElement(By.Id(elementId));

        return element;
    }

    public void ClickOnElementById(string elementId)
    {
        var element = FindElementById(elementId);

        element.Click();
    }

    public void ClickOnElementByXpath(string elementXpath)
    {
        var element = _driver.FindElement(By.XPath(elementXpath));

        element.Click();
    }

    public void WriteToInputByXpath(string elementXpath, string text)
    {
        var input = _driver.FindElement(By.XPath(elementXpath));

        input.SendKeys(text);
    }

    public void EnterImgToInputTextByXpath(string elementXpath, string imagePath)
    {
        var input = _driver.FindElement(By.XPath(elementXpath));

        input.SendKeys(Keys.Control + "v");

        Thread.Sleep(2000);
    }

    public void SetOptionInSelectByIdByValue(string elementId, string optionValue)
    {
        var selectElement = FindElementById(elementId);

        var option = selectElement.FindElement(By.CssSelector($"option[value='{optionValue}']"));

        option.Click();
    }


    public void WriteToInputTextByXpath(string elementXpath, string[] text)
    {
        var input = _driver.FindElement(By.XPath(elementXpath));
        foreach (var t in text)
        {
            input.SendKeys(t);
            input.SendKeys(Keys.Shift + Keys.Enter);
        }

    }

    public string GetElementAttributeByXpath(string elementXpath, string attributeName)
    {
        var elem = FindElementByXpath(elementXpath);

        return elem.GetAttribute(attributeName);
    }

    public string GetElementAttributeById(string elementId, string attributeName)
    {
        var elem = FindElementById(elementId);

        return elem.GetAttribute(attributeName);
    }

    public void WriteToInputById(string elementId, string text)
    {
        var input = _driver.FindElement(By.Id(elementId));

        input.SendKeys(text);
    }

    public void RewriteToInputByXpath(string elementXpath, string text)
    {
        var input = _driver.FindElement(By.XPath(elementXpath));

        ClearInput(input);

        input.SendKeys(text);
    }

    public void RewriteToInputById(string elementId, string text)
    {
        var input = _driver.FindElement(By.Id(elementId));

        ClearInput(input);

        input.SendKeys(text);
    }

    private void ClearInput(IWebElement input)
    {
        input.Clear();
        input.SendKeys(Keys.Control + "a");
        input.SendKeys(Keys.Delete);
    }

    public bool WaitWhenElementDisplayByXpath(string elementXpath, int timeout = 5)
    {
        var i = 0;
        while (!IsDisplayedByXpath(elementXpath))
        {
            Thread.Sleep(1000);
            i++;
            if (i > timeout)
            {
                return false;
            }
        }
        return true;
    }

    public bool WaitWhenElementEnableByXpath(string elementXpath, int timeout = 5)
    {
        var i = 0;
        while (!IsEnabledByXpath(elementXpath))
        {
            Thread.Sleep(1000);
            i++;
            if (i > timeout)
            {
                return false;
            }
        }
        return true;
    }

    public bool SwitchToFrameByXpath(string elementXpath)
    {
        try
        {
            var iframeElement = FindElementByXpath(elementXpath);

            _driver.SwitchTo().Frame(iframeElement);

            return true;
        }
        catch
        {
            return false;
        }
    }


    public bool DeleteElementByXpath(string elementXpath)
    {
        try
        {
            var element = _driver.FindElement(By.XPath(elementXpath));

            var js = (IJavaScriptExecutor)_driver;

            js.ExecuteScript("arguments[0].remove()", element);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DeleteElementById(string elementId)
    {
        try
        {
            var element = _driver.FindElement(By.Id(elementId));

            var js = (IJavaScriptExecutor)_driver;

            js.ExecuteScript("arguments[0].remove()", element);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsDisplayedByXpath(string elementXpath)
    {
        try
        {
            var result = _driver.FindElement(By.XPath(elementXpath)).Displayed;

            return result;
        }
        catch
        {
            return false;
        }
    }

    public bool IsEnabledByXpath(string elementXpath)
    {
        try
        {
            var result = _driver.FindElement(By.XPath(elementXpath)).Enabled;

            return result;
        }
        catch
        {
            return false;
        }
    }



    public bool WaitForElementByXpath(string elementXpath, int timeout = 5)
    {
        var i = 0;
        while (!IsFindByXpath(elementXpath))
        {
            Thread.Sleep(1000);
            i++;
            if (i > timeout)
            {
                return false;
            }
        }
        return true;
    }

    public bool WaitForElementById(string elementId = null, int timeout = 5)
    {
        var i = 0;
        while (!IsFindById(elementId))
        {
            Thread.Sleep(1000);
            i++;
            if (i > timeout)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsFindByXpath(string elementXpath)
    {
        try
        {
            var elem = FindElementByXpath(elementXpath);

            return true;
        }
        catch
        {
            return false;
        }
    }
    public bool IsFindById(string elementId)
    {
        try
        {
            var elem = _driver.FindElement(By.Id(elementId));
            return true;
        }
        catch
        {
            return false;
        }
    }


    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;
    public void HideWindow()
    {
        var hWnd = FindWindow(null, _driver.Title);

        ShowWindow(hWnd, SW_HIDE);
    }
}