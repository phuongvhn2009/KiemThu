using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SauceDemo.Tests.Pages;

public class InventoryPage(IWebDriver driver, int timeout) : BasePage(driver, timeout)
{
    private readonly By title = By.CssSelector("[data-test='title']");
    private readonly By backpack = By.Id("add-to-cart-sauce-labs-backpack");
    private readonly By cart = By.CssSelector("[data-test='shopping-cart-link']");

    private readonly By items = By.CssSelector("[data-test='inventory-item']");
    private readonly By itemName = By.CssSelector("[data-test='inventory-item-name']");
    private readonly By itemPrice = By.CssSelector("[data-test='inventory-item-price']");
    private readonly By sort = By.CssSelector("[data-test='product-sort-container']");

    public IReadOnlyList<(string Name, string Price)> Products()
    {
        Tim(title);
        return Driver.FindElements(items)
            .Select(item => (
                item.FindElement(itemName).Text,
                item.FindElement(itemPrice).Text))
            .ToArray();
    }

    public bool DaHienThi() => Title == "Products" && Url.EndsWith("/inventory.html", StringComparison.Ordinal);
    public string Url => Driver.Url;
    public InventoryPage SapXep(string value)
    {
        new SelectElement(Tim(sort)).SelectByValue(value);
        return this;
    }
    public IReadOnlyList<decimal> LayDanhSachGia() => Products()
        .Select(p => decimal.Parse(p.Price.TrimStart('$'), CultureInfo.InvariantCulture)).ToArray();
    public string Title => Tim(title).Text;
    public void AddBackpack() => Tim(backpack).Click();
    public CartPage OpenCart()
    {
        Tim(cart).Click();
        return new CartPage(Driver, Timeout);
    }
}
