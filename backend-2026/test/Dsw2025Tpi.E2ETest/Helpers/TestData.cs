namespace Dsw2025Tpi.E2ETest.Helpers;

public static class TestData
{
    private static string FourDigits() => Random.Shared.Next(0, 10000).ToString("D4");
    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    public static string Sku() => $"SKU-{FourDigits()}";
    public static string InternalCode() => $"INT-{FourDigits()}";

    public static string Username(string prefix) => $"{prefix}{Suffix()}";
    public static string Email(string prefix) => $"{prefix}{Suffix()}@e2e.test";

    public static string Password() => "Aa1!" + Guid.NewGuid().ToString("N");
}