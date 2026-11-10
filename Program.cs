// Program.cs is the entry point. It has two phases:
//   1. BUILD   - register services (things the app needs) in the DI container.
//   2. CONFIGURE - set up the middleware pipeline (what happens to each HTTP request), then Run().

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TeachingAPI.Services;
using TeachingAPI.Settings;

// Creates the builder: loads appsettings.json, environment variables, command-line args, logging, etc.
var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// PHASE 1: register services (Dependency Injection)
// ---------------------------------------------------------------------------

// Discover [ApiController] classes in /Controllers and make them reachable via routes.
builder.Services.AddControllers();

// Generates an OpenAPI (JSON) description of the API at /openapi/v1.json (development only, see below).
// The transformer adds a "Bearer" security scheme so Swagger UI shows an "Authorize" button
// where you can paste your JWT.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            }
        };
        document.Security = [new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        }];
        return Task.CompletedTask;
    });
});

// Read the "Jwt" section of appsettings.json into JwtSettings and make it injectable as IOptions<JwtSettings>.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Lifetimes:
//   Singleton = one instance for the whole app (needed here, otherwise the in-memory data would reset every request)
//   Scoped    = one instance per HTTP request
//   Transient = a new instance every time it is requested
builder.Services.AddSingleton<IProductService, InMemoryProductService>();
builder.Services.AddSingleton<IUserService, InMemoryUserService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<AnkitaDetailsService>();

// Authentication: "who are you?" We use JWT bearer tokens.
// We read the settings now (not via DI) because we need them while configuring the services.
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
          ?? throw new InvalidOperationException("Missing 'Jwt' section in configuration.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)   // default scheme = JWT
    .AddJwtBearer(options =>
    {
        // Keep claim names exactly as they appear in the token ("sub", "name", "role").
        options.MapInboundClaims = false;

        // Rules every incoming token must pass, otherwise the request gets 401 Unauthorized.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,                    // reject expired tokens
            ValidateIssuerSigningKey = true,            // reject tokens not signed with our key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            NameClaimType = "name",                     // User.Identity.Name reads the "name" claim
            RoleClaimType = "role"                      // [Authorize(Roles = ...)] reads the "role" claim
        };
    });

    // Test@123 -- PlainText
    // xcbd934nsdfhdfkwfdi345945034xx939xkdfkdf - Encrypted Password
    //if (CryptPassword.Verify("Test@123", "$2a$11$xcbd934nsdfhdfkwfdi345945034xx939xkdfkdf"))
    //{
    //    Console.WriteLine("Password is valid.");
    //}
    //else
    //{
    //    Console.WriteLine("Password is valid.");
    //}
    //else
    //{
    //    Console.WriteLine("Password is invalid.");
    //}

// Authorization: "what are you allowed to do?" ([Authorize], roles, policies).
builder.Services.AddAuthorization();

// Builds the app. After this line no more services can be registered.
var app = builder.Build();



// ---------------------------------------------------------------------------
// PHASE 2: middleware pipeline. ORDER MATTERS - a request flows through these top to bottom.
// ---------------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    // Exposes the OpenAPI JSON document at /openapi/v1.json
    app.MapOpenApi();

    // Swagger UI: an interactive web page at /swagger that reads the OpenAPI JSON above.
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "TeachingAPI v1"));
}

// Redirects http:// requests to https://
app.UseHttpsRedirection();

// 1) Authentication first: read the "Authorization: Bearer ..." header and build the User.
app.UseAuthentication();
// 2) Then authorization: check that User is allowed to call the endpoint.
app.UseAuthorization();

// Connects incoming URLs to the controller actions.
app.MapControllers();

// Starts the web server and blocks until the app is stopped.
app.Run();

//http://localhost:5000/swagger/index.html

// Makes Program visible to the test project (WebApplicationFactory<Program>).
public partial class Program { }
