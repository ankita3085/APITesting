using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TeachingAPI.Dtos;
using TeachingAPI.Models;

namespace TeachingAPI.Tests;

/// <summary>End-to-end tests through the real pipeline (JWT auth, validation, routing).</summary>
public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Products = "/api/ankita/products";
    private readonly WebApplicationFactory<Program> _factory;

    public ApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static object Valid(string name = "Gadget") =>
        new { name, description = "d", price = 10.5m, stock = 2 };

    private async Task<HttpClient> LoggedIn(string username = "admin", string password = "Admin123!")
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        res.EnsureSuccessStatusCode();
        var token = (await res.Content.ReadFromJsonAsync<TokenResponse>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<HttpClient> RegularUser()
    {
        var name = "user" + Guid.NewGuid().ToString("N")[..8];
        var anon = _factory.CreateClient();
        var reg = await anon.PostAsJsonAsync("/api/auth/register", new { username = name, password = "secret123" });
        Assert.Equal(HttpStatusCode.Created, reg.StatusCode);
        return await LoggedIn(name, "secret123");
    }

    // ---- Auth ----

    [Fact]
    public async Task Register_NewUser_Returns201WithoutPasswordHash()
    {
        var res = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register",
            new { username = "reg" + Guid.NewGuid().ToString("N")[..8], password = "secret123" });

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"role\":\"User\"", body);
    }

    [Fact]
    public async Task Register_DuplicateUsername_Returns409()
    {
        var res = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register",
            new { username = "admin", password = "secret123" });
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    [Theory]
    [InlineData("ab", "secret123")]       // username too short
    [InlineData("validuser", "123")]      // password too short
    [InlineData("", "")]                  // both missing
    public async Task Register_InvalidInput_Returns400(string username, string password)
    {
        var res = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register", new { username, password });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var res = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "Admin123!" });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var token = await res.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrEmpty(token!.Token));
        Assert.True(token.ExpiresAtUtc > DateTime.UtcNow);

        // IsNullOrEmpty: It is a method in the System.String class that checks whether a given string is null or an empty string (""). It returns true if the string is null or empty, and false otherwise.
        // IsNullOrWhiteSpace: It is a method in the System.String class that checks whether a given string is null, empty, or consists only of white-space characters. It returns true if the string is null, empty, or contains only white-space characters, and false otherwise.
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var res = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "WrongPass1" });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    // ---- Products: public reads ----

    [Fact]
    public async Task GetAll_IsPublic_AndReturnsSeedData()
    {
        var res = await _factory.CreateClient().GetAsync(Products);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var items = await res.Content.ReadFromJsonAsync<List<Product>>();
        Assert.True(items!.Count >= 2);
        // We use ! like in items!.Count because we know the response is a list of products, so it won't be null. The ! operator tells the compiler to ignore nullability warnings here.
        // We use ? like in items?.Count because we don't know if the response is null, so we want to avoid a NullReferenceException. The ? operator tells the compiler to check for null before accessing the property.
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var res = await _factory.CreateClient().GetAsync($"{Products}/1");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(1, (await res.Content.ReadFromJsonAsync<Product>())!.Id);
    }

    [Fact]
    public async Task GetById_Missing_Returns404()
    {
        var res = await _factory.CreateClient().GetAsync($"{Products}/99999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task GetById_NonIntegerId_Returns404()
    {
        var res = await _factory.CreateClient().GetAsync($"{Products}/abc");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    // ---- Products: authentication ----

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        var res = await _factory.CreateClient().PostAsJsonAsync(Products, Valid());
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Create_WithGarbageToken_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not.a.token");
        var res = await client.PostAsJsonAsync(Products, Valid());
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Update_WithoutToken_Returns401()
    {
        var res = await _factory.CreateClient().PutAsJsonAsync($"{Products}/1", Valid());
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    // ---- Products: CRUD as logged-in user ----

    [Fact]
    public async Task Create_ValidRequest_Returns201WithLocation_AndSetsCreatedBy()
    {
        var client = await RegularUser();
        var res = await client.PostAsJsonAsync(Products, Valid("Created"));

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var product = (await res.Content.ReadFromJsonAsync<Product>())!;
        Assert.EndsWith($"{Products}/{product.Id}", res.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Created", product.Name);
        Assert.StartsWith("user", product.CreatedBy);

        var fetched = await _factory.CreateClient().GetAsync(res.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
    }

    [Theory]
    [InlineData("x", 10, 1)]          // name too short
    [InlineData("Good", 0, 1)]        // price below minimum
    [InlineData("Good", 10, -1)]      // negative stock
    public async Task Create_InvalidRequest_Returns400(string name, decimal price, int stock)
    {
        var client = await RegularUser();
        var res = await client.PostAsJsonAsync(Products, new { name, price, stock });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Update_Existing_Returns200WithNewValues()
    {
        var client = await RegularUser();
        var created = await (await client.PostAsJsonAsync(Products, Valid("Before"))).Content.ReadFromJsonAsync<Product>();

        var res = await client.PutAsJsonAsync($"{Products}/{created!.Id}", Valid("After"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("After", (await res.Content.ReadFromJsonAsync<Product>())!.Name);
    }

    [Fact]
    public async Task Update_Missing_Returns404()
    {
        var client = await RegularUser();
        var res = await client.PutAsJsonAsync($"{Products}/99999", Valid());
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    // ---- Products: role-based delete ----

    [Fact]
    public async Task Delete_AsRegularUser_Returns403()
    {
        var client = await RegularUser();
        var res = await client.DeleteAsync($"{Products}/1");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutToken_Returns401()
    {
        var res = await _factory.CreateClient().DeleteAsync($"{Products}/1");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Delete_AsAdmin_Returns204_ThenGetReturns404_ThenDeleteAgainReturns404()
    {
        var admin = await LoggedIn();
        var created = await (await admin.PostAsJsonAsync(Products, Valid("Doomed"))).Content.ReadFromJsonAsync<Product>();
        var url = $"{Products}/{created!.Id}";

        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.DeleteAsync(url)).StatusCode);
    }
}
