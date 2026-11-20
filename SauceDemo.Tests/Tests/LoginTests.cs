using NUnit.Framework;
using SauceDemo.Tests.Core;
using SauceDemo.Tests.Data;

namespace SauceDemo.Tests.Tests;

public record LoginCase(string Username, string Password, bool ThanhCong, string GhiChu, string? Error);

[TestFixture, Category("Login")]
public class LoginTests : BaseTest
{
    public static IEnumerable<TestCaseData> Cases() =>
        AccountRepository.LoadAll().Select((u, i) =>
        {
            var tc = new TestCaseData(u)
                .SetName($"DangNhap_{i:00}_{u.Username}")
                .SetDescription($"UseCase Dang nhap: {u.GhiChu} (Expected={(u.ThanhCong ? "PASS/Inventory" : "FAIL/Error")})");
            if (u.Username == "standard_user" && u.ThanhCong)
                tc.SetCategory("Smoke");
            if (u.Username == "performance_glitch_user")
                tc.SetCategory("Slow");
            return tc;
        });

    [Test, Category("Smoke"), Description("Kiem tra tieu de va URL trang dang nhap SauceDemo")]
    public void PageTitle()
    {
        TestContext.Out.WriteLine(Login.Title);
        Assert.That(Login.Title, Is.EqualTo("Swag Labs"));
        Assert.That(Login.Url, Does.StartWith("https://www.saucedemo.com"));
    }

    [TestCaseSource(nameof(Cases))]
    public void Authenticate(LoginCase data)
    {
        Assert.That(data.Username, Is.Not.Null);
        Assert.That(data.Password, Is.Not.Null);
        var inventory = Login.DangNhap(data.Username, data.Password);
        Assert.That(inventory.DaHienThi(), Is.EqualTo(data.ThanhCong));
        if (data.ThanhCong)
        {
            Assert.That(inventory.Title, Is.EqualTo("Products"));
            Assert.That(inventory.Url, Does.EndWith("/inventory.html"));
            Assert.That(inventory.Products(), Has.Count.EqualTo(6));
        }
        else if (data.Error is not null)
        {
            Assert.That(Login.LayThongBaoLoi(), Is.EqualTo(data.Error));
        }
    }
}
