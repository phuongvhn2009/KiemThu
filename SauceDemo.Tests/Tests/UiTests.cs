using System.Text.Json;
using NUnit.Framework;
using SauceDemo.Tests.Framework;
using SauceDemo.Tests.Pages;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(2)]

namespace SauceDemo.Tests.Tests;

public record LoginCase(string Id, string User, string Password, string? Error);

public class LoginTests : BaseTest
{
    public static IEnumerable<TestCaseData> Cases() =>
        JsonSerializer.Deserialize<LoginCase[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data", "login.json")))!
            .Select(c => new TestCaseData(c).SetName("Login_" + c.Id));

    [Test]
    public void PageTitle() { TestContext.Out.WriteLine(Login.Title); Assert.That(Login.Title, Is.EqualTo("Swag Labs")); }

    [TestCaseSource(nameof(Cases))]
    public void Authenticate(LoginCase data)
    {
        Log("Login case: " + data.Id);
        Login.Login(data.User, data.Password);
        if (data.Error is null)
        {
            Assert.That(new InventoryPage(Driver, Config.TimeoutSeconds).Title, Is.EqualTo("Products"));
            Assert.That(Driver.Url, Does.EndWith("/inventory.html"));
        }
        else
            Assert.That(Login.Error, Is.EqualTo(data.Error));
    }
}

public class CartTests : BaseTest
{
    [Test]
    public void AddBackpack()
    {
        Login.Login("standard_user", "secret_sauce");
        var inventory = new InventoryPage(Driver, Config.TimeoutSeconds);
        Assert.That(inventory.Title, Is.EqualTo("Products"));
        inventory.AddBackpack();
        inventory.OpenCart();
        var cart = new CartPage(Driver, Config.TimeoutSeconds);
        Assert.Multiple(() =>
        {
            Assert.That(cart.Product, Is.EqualTo("Sauce Labs Backpack"));
            Assert.That(cart.Quantity, Is.EqualTo("1"));
        });
    }
}
