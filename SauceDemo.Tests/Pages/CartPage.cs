using OpenQA.Selenium;

namespace SauceDemo.Tests.Pages;

public class CartPage(IWebDriver driver, int timeout) : BasePage(driver, timeout)
{
    private readonly By name = By.CssSelector("[data-test='cart-list'] [data-test='inventory-item-name']");
    private readonly By quantity = By.CssSelector("[data-test='item-quantity']");
    public string Product => Tim(name).Text;
    public string Quantity => Tim(quantity).Text;
}
