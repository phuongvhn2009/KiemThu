using NUnit.Framework;
using SauceDemo.Tests.Core;

namespace SauceDemo.Tests.Tests;

[TestFixture, Category("Inventory")]
public class InventoryTests : BaseTest
{
    [Test, Category("Smoke"), Description("Kich ban hien thi trang Inventory va thu thap 6 san pham")]
    public void Products()
    {
        var inventory = Login.DangNhap("standard_user", "secret_sauce");
        Assert.That(inventory.DaHienThi(), Is.True);
        Assert.That(inventory.Url, Does.EndWith("/inventory.html"));
        var products = inventory.Products();
        Assert.That(products, Has.Count.EqualTo(6));
        foreach (var item in products)
        {
            TestContext.Out.WriteLine($"{item.Name}: {item.Price}");
            Assert.That(item.Name, Is.Not.Empty);
            Assert.That(item.Price, Does.StartWith("$"));
        }
        Assert.That(products.Select(p => p.Name), Does.Contain("Sauce Labs Backpack"));
    }

    [Test, Description("Kich ban sap xep gia san pham tang dan (Price low to high)")]
    public void PricesAscending()
    {
        var prices = Login.DangNhap("standard_user", "secret_sauce")
            .SapXep("lohi").LayDanhSachGia();
        Assert.That(prices, Has.Count.EqualTo(6));
        Assert.That(prices, Is.Ordered.Ascending);
    }
}
