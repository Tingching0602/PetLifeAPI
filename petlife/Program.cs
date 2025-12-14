using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.OpenApi.Models;
using petlife.Authentication;
using System.Data;
using System.Data.SqlClient;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Bind to dynamic port if not explicitly configured (avoids conflicts)
var configuredUrls = builder.Configuration["Hosting:Urls"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (string.IsNullOrWhiteSpace(configuredUrls))
{
    //0 means dynamic port selection
    builder.WebHost.UseUrls("http://0.0.0.0:0");
}

builder.Services.AddControllers();
// CORS: allow all origins, methods, headers
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "petlife API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your Firebase ID token. Example: \"Bearer eyJ...\"",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// Dapper: register IDbConnection factory
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    return new SqlConnection(connStr);
});

// Firebase initialization (single instance)
try
{
    if (FirebaseApp.DefaultInstance == null)
    {
        var projectId = builder.Configuration["Firebase:ProjectId"] ?? "petlife-3cedc"; // fallback
        var configuredPath = builder.Configuration["Firebase:ServiceAccountPath"]; // optional configured path
        string filePathToUse = null;

        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
        {
            filePathToUse = configuredPath;
        }
        else
        {
            // fallback to local deployment file name serviceAccountKey.json in content root
            var localPath = Path.Combine(AppContext.BaseDirectory, "serviceAccountKey.json");
            if (File.Exists(localPath)) filePathToUse = localPath;
        }

        AppOptions options;
        if (filePathToUse != null)
        {
            options = new AppOptions
            {
                Credential = GoogleCredential.FromFile(filePathToUse),
                ProjectId = projectId
            };
        }
        else
        {
            // Attempt Application Default Credentials (GCP environment)
            var adcCredential = GoogleCredential.GetApplicationDefault();
            options = new AppOptions
            {
                Credential = adcCredential,
                ProjectId = projectId
            };
        }

        FirebaseApp.Create(options);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to initialize FirebaseApp: {ex.Message}");
    // Optionally rethrow if this should block startup
    // throw;
}

// Add custom Firebase authentication scheme
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Firebase";
    options.DefaultChallengeScheme = "Firebase";
}).AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>("Firebase", options => { });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable HTTPS redirection only if an HTTPS endpoint is configured
var urlsForRedirectCheck = configuredUrls ?? string.Join(";", app.Urls);
if (!string.IsNullOrWhiteSpace(urlsForRedirectCheck) && urlsForRedirectCheck.Contains("https://", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}

// Serve static files (for uploaded images in wwwroot)
app.UseStaticFiles();

// Enable CORS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Helper endpoint to reveal server addresses (for front-end to discover dynamic port)
app.MapGet("/server-addresses", (IServer server) =>
{
    var feature = server.Features.Get<IServerAddressesFeature>();
    return Results.Ok(feature?.Addresses ?? Array.Empty<string>());
});

app.MapControllers();

app.Run();
