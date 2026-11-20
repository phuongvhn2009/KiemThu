using NUnit.Framework;
using SauceDemo.Tests.Core;

namespace SauceDemo.Tests.Tests;

[TestFixture, Category("Cart")]
public class CartTests : BaseTest
{
    [Test, Category("Smoke"), Description("Kich ban them Sauce Labs Backpack vao gio hang va kiem tra chi tiet gio")]
    public void AddBackpack()
    {
        var inventory = Login.DangNhap("standard_user", "secret_sauce");
        Assert.That(inventory.Title, Is.EqualTo("Products"));
        inventory.AddBackpack();
        var cart = inventory.OpenCart();
        Assert.Multiple(() =>
        {
            Assert.That(cart.Url, Does.EndWith("/cart.html"));
            Assert.That(cart.Product, Is.EqualTo("Sauce Labs Backpack"));
            Assert.That(cart.Quantity, Is.EqualTo("1"));
        });
    }
}
