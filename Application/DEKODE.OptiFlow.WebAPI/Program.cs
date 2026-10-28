using System.Text;
using DEKODE.OptiFlow.Infrastructure;
using DEKODE.OptiFlow.WebAPI.Configuration;
using DEKODE.OptiFlow.WebAPI.Middleware;
using DEKODE.OptiFlow.WebAPI.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers, OpenAPI, and Swagger
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Register OptiFlow Infrastructure (Persistence + Business Services via Composition Root)
builder.Services.AddOptiFlowInfrastructure(builder.Configuration);

// 3. Application-layer services & JWT Options (Item 9: Fail-Fast & Strongly-Typed Validation)
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(opts =>
    {
        if (builder.Environment.IsProduction() && string.IsNullOrWhiteSpace(opts.SecretKey))
        {
            return false;
        }
        return true;
    }, "CRITICAL: JWT SecretKey must be explicitly provided in production.")
    .ValidateOnStart();

// Fallback configuration for Development / Testing environments if omitted
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var secretKey = jwtSection["SecretKey"];
if (string.IsNullOrWhiteSpace(secretKey))
{
    if (builder.Environment.IsProduction())
    {
        throw new InvalidOperationException("CRITICAL: 'Jwt:SecretKey' configuration is required in Production environment.");
    }
    secretKey = "Dekode_OptiFlow_SuperSecretKey_For_JWT_Auth_2026_Minimum_32_Chars!";
}

var jwtIssuer = jwtSection["Issuer"] ?? "OptiFlowAPI";
var jwtAudience = jwtSection["Audience"] ?? "OptiFlowClients";

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Item 8: FluentValidation pipeline
builder.Services.AddValidatorsFromAssemblyContaining<DEKODE.OptiFlow.DTO.Validators.AllocationExecutionRequestDtoValidator>();

// 4. Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. CORS policy for external frontends
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OptiFlow API v1");
        c.RoutePrefix = "swagger";
    });
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("OptiFlow Allocation Web API");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Execute PostgreSQL migrations automatically on startup if configured (Development/Docker)
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    try
    {
        await app.Services.MigrateOptiFlowDatabaseAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not apply database migrations on startup. PostgreSQL container might not be running yet.");
    }
}

app.Run();
