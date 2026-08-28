using System.Text;
using ContainerFlow.Container.Api.Persistence;
using ContainerFlow.Container.Api.Services;
using ContainerFlow.Shared.Auth;
using ContainerFlow.Shared.Correlation;
using ContainerFlow.Shared.Health;
using ContainerFlow.Shared.Logging;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = ContainerFlowLoggingExtensions.CreateLogger(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddControllers()
        .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── Database (container_db — owned by this service alone) ──────────────
    builder.Services.AddDbContext<ContainerDbContext>(o =>
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ── Shared health checks + PostgreSQL connectivity ────────────────────
    builder.Services.AddContainerFlowHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

    // ── JWT authentication (tokens issued by the Gateway) ─────────────────
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            var jwt = builder.Configuration.GetSection("Jwt");
			o.MapInboundClaims = false;
			o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwt["Audience"],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SigningKey"]!)),

                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });
    builder.Services.AddAuthorization();

    // ── Shared current-user + correlation plumbing ────────────────────────
    builder.Services.AddContainerFlowCurrentUser();

    // ── MassTransit / RabbitMQ (publisher only — no consumers) ────────────
    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost",
                builder.Configuration.GetValue<ushort>("RabbitMQ:Port", 5672),
                "/",
                h =>
                {
                    h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
                });
        });
    });

    builder.Services.AddScoped<IContainerService, ContainerService>();

    var app = builder.Build();

    app.UseCorrelationId();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();

    // Ensure the schema exists (each service owns its database).
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ContainerDbContext>();
        db.Database.EnsureCreated();
        ContainerSeedData.Seed(db);
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Container API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}