using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ContainerFlow.Shared.Auth;
using ContainerFlow.Shared.Correlation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ════════════════════════════════════════════════════════════════
// JWT AUTHENTICATION
// ════════════════════════════════════════════════════════════════
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSection["Issuer"] ?? "ContainerFlow";
var jwtAudience = jwtSection["Audience"] ?? "ContainerFlow";
var jwtSigningKey = jwtSection["SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = AuthConstants.ClaimRole
        };
    });

// ════════════════════════════════════════════════════════════════
// AUTHORIZATION POLICIES
// ════════════════════════════════════════════════════════════════
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BookingCreate", policy =>
        policy.RequireAuthenticatedUser()
            .RequireAssertion(ctx =>
                ctx.User.IsInRole(AuthConstants.RoleAdmin) ||
                ctx.User.IsInRole(AuthConstants.RoleStaff) ||
                ctx.User.IsInRole(AuthConstants.RoleCustomer)));

    options.AddPolicy("BookingManage", policy =>
        policy.RequireAuthenticatedUser()
            .RequireAssertion(ctx =>
                ctx.User.IsInRole(AuthConstants.RoleAdmin) ||
                ctx.User.IsInRole(AuthConstants.RoleStaff)));

    options.AddPolicy("ContainerManage", policy =>
        policy.RequireAuthenticatedUser()
            .RequireAssertion(ctx =>
                ctx.User.IsInRole(AuthConstants.RoleAdmin) ||
                ctx.User.IsInRole(AuthConstants.RoleStaff)));

    options.AddPolicy("NotificationView", policy =>
        policy.RequireAuthenticatedUser().
            RequireAssertion(ctx =>
                ctx.User.IsInRole(AuthConstants.RoleAdmin) ||
                ctx.User.IsInRole(AuthConstants.RoleStaff) ||
                ctx.User.IsInRole(AuthConstants.RoleCustomer)));
});

// ════════════════════════════════════════════════════════════════
// YARP REVERSE PROXY
// ════════════════════════════════════════════════════════════════
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ════════════════════════════════════════════════════════════════
// HEALTH CHECKS
// ════════════════════════════════════════════════════════════════
builder.Services.AddHealthChecks();

// ════════════════════════════════════════════════════════════════
// SWAGGER / OPENAPI
// ════════════════════════════════════════════════════════════════
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "ContainerFlow API Gateway",
        Version = "v1",
        Description = "Public entry point for ContainerFlow. Routes: /api/bookings, /api/containers, /api/notifications."
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste your JWT token here. Obtain one from POST /api/auth/login."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ════════════════════════════════════════════════════════════════
// CURRENT USER & HTTP CONTEXT
// ════════════════════════════════════════════════════════════════
builder.Services.AddHttpContextAccessor();

// ════════════════════════════════════════════════════════════════
// CORS
// ════════════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// ════════════════════════════════════════════════════════════════
// MIDDLEWARE PIPELINE
// ════════════════════════════════════════════════════════════════
app.UseCorrelationId();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ContainerFlow API Gateway v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

// ════════════════════════════════════════════════════════════════
// HEALTH ENDPOINT (Gateway liveness, independent of backends)
// ════════════════════════════════════════════════════════════════
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "ContainerFlow.Gateway",
    Timestamp = DateTime.UtcNow
}));

// ════════════════════════════════════════════════════════════════
// LOGIN ENDPOINT — issues JWT tokens (dev credentials)
// ════════════════════════════════════════════════════════════════
app.MapPost("/api/auth/login", async (HttpContext httpContext) =>
{
    using var reader = new StreamReader(httpContext.Request.Body);
    var body = await reader.ReadToEndAsync();
    LoginRequest? login;

    try { login = JsonSerializer.Deserialize<LoginRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
    catch { login = null; }

    if (login is null || string.IsNullOrWhiteSpace(login.Username))
        return Results.BadRequest(new { Error = "Invalid request body. Expected: { \"username\": \"...\" }" });

    // Dev credentials — username determines role
    var username = login.Username.Trim().ToLowerInvariant();

    string userId;
    string userRole;
    string userName;
    string? customerId = null;

    switch (username)
    {
        case "admin":
            userId = "00000000-0000-0000-0000-000000000001";
            userRole = "admin";
            userName = "Admin User";
            break;
        case "staff":
            userId = "00000000-0000-0000-0000-000000000002";
            userRole = "staff";
            userName = "Staff User";
            break;
        case "customer":
            userId = "00000000-0000-0000-0000-000000000003";
            userRole = "customer";
            userName = "Customer User";
            customerId = "10000000-0000-0000-0000-000000000001";
            break;
        default:
            return Results.Unauthorized();
    }

    var tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey));
    var credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new(AuthConstants.ClaimSub, userId),
        new(AuthConstants.ClaimRole, userRole),
        new(AuthConstants.ClaimName, userName),
        new(ClaimTypes.Name, userName),
    };

    if (customerId is not null)
        claims.Add(new(AuthConstants.ClaimCustomerId, customerId));

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: credentials
    );

    var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new
    {
        Token = tokenStr,
        User = new
        {
            Id = userId,
            Name = userName,
            Role = userRole,
            CustomerId = customerId
        }
    });
}).WithName("Login");

// ════════════════════════════════════════════════════════════════
// YARP REVERSE PROXY
// ════════════════════════════════════════════════════════════════
app.MapReverseProxy();

app.Run();

// ════════════════════════════════════════════════════════════════
// DTO
// ════════════════════════════════════════════════════════════════
internal sealed record LoginRequest(string Username);