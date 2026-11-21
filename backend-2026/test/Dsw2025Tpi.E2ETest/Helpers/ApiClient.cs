using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dsw2025Tpi.E2ETest.Models;

namespace Dsw2025Tpi.E2ETest.Helpers;

public class ApiClient : IDisposable
{
    public HttpClient Http { get; }
    public LoginResponse? Session { get; private set; }

    public ApiClient()
    {
        var baseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = E2EApiHost.EnsureStarted();
        }

        Http = new HttpClient(new RateLimitRetryHandler())
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(90)
        };
    }

    public static async Task<ApiClient> CreateAuthenticatedAsync(string role)
    {
        var client = new ApiClient();
        var prefix = role.ToLowerInvariant();
        var username = TestData.Username(prefix);
        var password = TestData.Password();

        await client.RegisterAsync(username, TestData.Email(prefix), password, role);
        await client.LoginAsync(username, password);
        return client;
    }

    public async Task RegisterAsync(string username, string email, string password, string role)
    {
        var response = await Http.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(username, password, email, role));
        await EnsureSuccessAsync(response, "registro");
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var response = await Http.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(username, password));
        await EnsureSuccessAsync(response, "login");

        Session = await response.Content.ReadFromJsonAsync<LoginResponse>()
                  ?? throw new InvalidOperationException("El login no devolvió cuerpo.");

        Http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Session.Token);
        return Session;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string step)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(
            $"Falló el paso '{step}': {(int)response.StatusCode} {response.StatusCode}. Respuesta: {body}");
    }

    public void Dispose() => Http.Dispose();
}