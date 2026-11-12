using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SauceDemo.Tests.Pages;

public class InventoryPage(IWebDriver driver, int timeout) : BasePage(driver, timeout)
{
    private readonly By title = By.CssSelector("[data-test='title']");
    private readonly By backpack = By.Id("add-to-cart-sauce-labs-backpack");
    private readonly By cart = By.CssSelector("[data-test='shopping-cart-link']");

    public IReadOnlyList<(string Name, string Price)> Products()
    {
        Find(title);
        return Driver.FindElements(By.CssSelector("[data-test='inventory-item']"))
            .Select(item => (
                item.FindElement(By.CssSelector("[data-test='inventory-item-name']")).Text,
                item.FindElement(By.CssSelector("[data-test='inventory-item-price']")).Text))
            .ToArray();
    }

    public void SortPrice() => new SelectElement(Find(By.CssSelector("[data-test='product-sort-container']"))).SelectByValue("lohi");
    public string Title => Find(title).Text;
    public void AddBackpack() => Find(backpack).Click();
    public void OpenCart() => Find(cart).Click();
}
