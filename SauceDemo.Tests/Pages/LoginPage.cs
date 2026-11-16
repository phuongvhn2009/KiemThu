using OpenQA.Selenium;

namespace SauceDemo.Tests.Pages;

public class LoginPage(IWebDriver driver, int timeout) : BasePage(driver, timeout)
{
    private readonly By username = By.Id("user-name");
    private readonly By password = By.Id("password");
    private readonly By submit = By.Id("login-button");
    private readonly By error = By.CssSelector("[data-test='error']");
    public string Title => Driver.Title;
    public void Open(string url) { Driver.Navigate().GoToUrl(url); Tim(username); }
    public InventoryPage DangNhap(string user, string secret)
    {
        Tim(username).SendKeys(user);
        Tim(password).SendKeys(secret);
        Tim(submit).Click();
        return new InventoryPage(Driver, Timeout);
    }
    public string LayThongBaoLoi() => Tim(error).Text;
}
