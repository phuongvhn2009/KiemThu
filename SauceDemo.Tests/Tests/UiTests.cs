using System.Text.Json;
using NUnit.Framework;
using SauceDemo.Tests.Core;
using SauceDemo.Tests.Pages;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(2)]

namespace SauceDemo.Tests.Tests;

public record LoginCase(string Username, string Password, bool ThanhCong, string GhiChu, string? Error);

public class LoginTests : BaseTest
{
    public static IEnumerable<TestCaseData> Cases() =>
        (JsonSerializer.Deserialize<LoginCase[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data", "users.json")))
            ?? throw new InvalidDataException("Missing login data"))
            .Select((u, i) => new TestCaseData(u).SetName($"DangNhap_{i:00}_{u.Username}"));

    [Test]
    public void PageTitle() { TestContext.Out.WriteLine(Login.Title); Assert.That(Login.Title, Is.EqualTo("Swag Labs")); }

    [TestCaseSource(nameof(Cases))]
    public void Authenticate(LoginCase data)
    {
        Log("Login case: " + data.GhiChu);
        Assert.That(data.Username, Is.Not.Null);
        Assert.That(data.Password, Is.Not.Null);
        var inventory = Login.DangNhap(data.Username, data.Password);
        Assert.That(inventory.DaHienThi(), Is.EqualTo(data.ThanhCong));
        if (data.ThanhCong)
        {
            Assert.That(inventory.Title, Is.EqualTo("Products"));
            Assert.That(inventory.Url, Does.EndWith("/inventory.html"));
            Assert.That(inventory.DaHienThi(), Is.True);
        }
        else if (data.Error is not null)
            Assert.That(Login.LayThongBaoLoi(), Is.EqualTo(data.Error));
    }
}

public class CartTests : BaseTest
{
    [Test]
    public void AddBackpack()
    {
        var inventory = Login.DangNhap("standard_user", "secret_sauce");
        Assert.That(inventory.Title, Is.EqualTo("Products"));
        inventory.AddBackpack();
        var cart = inventory.OpenCart();
        Assert.Multiple(() =>
        {
            Assert.That(cart.Product, Is.EqualTo("Sauce Labs Backpack"));
            Assert.That(cart.Quantity, Is.EqualTo("1"));
        });
    }
}
