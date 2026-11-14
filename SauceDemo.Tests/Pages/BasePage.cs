using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SauceDemo.Tests.Pages;

public abstract class BasePage(IWebDriver driver, int timeout)
{
    protected int Timeout { get; } = timeout;
    protected WebDriverWait Wait { get; } = new(driver, TimeSpan.FromSeconds(timeout));
    protected IWebDriver Driver { get; } = driver;
    protected IWebElement Tim(By locator) => Wait.Until(d =>
    {
        try
        {
            var element = d.FindElement(locator);
            return element.Displayed && element.Enabled ? element : null;
        }
        catch (NoSuchElementException) { return null; }
        catch (StaleElementReferenceException) { return null; }
    })!;
}
