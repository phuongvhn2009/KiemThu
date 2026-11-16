using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using OpenQA.Selenium;
using SauceDemo.Tests.Config;
using SauceDemo.Tests.Core;
using SauceDemo.Tests.Pages;
using SauceDemo.Tests.Tests;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var config = ConfigReader.Load();
Check(config.WindowWidth > 0 && config.WindowHeight > 0, "Window configuration missing");
var invalid = new TestConfig { Browser = "UnsupportedTypo" };
try { DriverFactory.Create(invalid); throw new Exception("Unsupported browser accepted"); }
catch (ArgumentException e) { Check(e.Message.Contains(invalid.Browser), "Error omits invalid browser"); }

var driver = DispatchProxy.Create<IWebDriver, Stub>();
((Stub)(object)driver).Result = (name) => name == "get_Url" ? config.BaseUrl :
    name == "FindElements" ? new ReadOnlyCollection<IWebElement>([]) : throw new NotSupportedException(name);
Check(!new InventoryPage(driver, 1).DaHienThi(), "Negative login must return false without throwing");

var users = JsonSerializer.Deserialize<LoginCase[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data/users.json")))!;
Check(users.Length >= 5 && users.Any(u => u.ThanhCong) && users.Count(u => !u.ThanhCong && u.Username == "") >= 2,
    "Missing positive, negative or boundary data");
Check(users.All(u => u.Username is not null && u.Password is not null && !string.IsNullOrWhiteSpace(u.GhiChu)), "Invalid data schema");
Check(LoginTests.Cases().Count() == users.Length, "JSON rows must each generate one test");
Console.WriteLine($"PASS: config loaded from output directory; invalid browser rejected by name; negative inventory returns false; {users.Length} data rows generate {users.Length} cases.");

public class Stub : DispatchProxy
{
    public Func<string, object?> Result = null!;
    protected override object? Invoke(MethodInfo? method, object?[]? args) => Result(method!.Name);
}
