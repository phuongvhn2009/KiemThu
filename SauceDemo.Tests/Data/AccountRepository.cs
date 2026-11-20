using System.Text.Json;
using SauceDemo.Tests.Tests;

namespace SauceDemo.Tests.Data;

public static class AccountRepository
{
    public static IReadOnlyList<LoginCase> LoadAll()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "users.json");
        return JsonSerializer.Deserialize<LoginCase[]>(File.ReadAllText(path))
            ?? throw new InvalidDataException("Missing login data");
    }
}
