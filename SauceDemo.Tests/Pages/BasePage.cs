using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SauceDemo.Tests.Pages;

public abstract class BasePage(IWebDriver driver, int timeout)
{
    protected int Timeout { get; } = timeout;
    protected IWebDriver Driver { get; } = driver;
    protected IWebElement Find(By locator) => new WebDriverWait(Driver, TimeSpan.FromSeconds(Timeout)).Until(d =>
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
