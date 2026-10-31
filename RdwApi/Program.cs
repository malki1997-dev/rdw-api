using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RdwApi.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("ApiKey manquante : l'API refuse de démarrer sans clé.");
var apiKeyBytes = Encoding.UTF8.GetBytes(apiKey);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<RdwDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Rdw")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Clé API obligatoire sur /api/*, sauf /api/v1/health (supervision)
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var protege = path.StartsWithSegments("/api") && !path.StartsWithSegments("/api/v1/health");
    if (protege)
    {
        var fournie = context.Request.Headers["X-Api-Key"].ToString();
        var ok = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(fournie), apiKeyBytes);
        if (!ok)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Clé API manquante ou invalide (en-tête X-Api-Key)." });
            return;
        }
    }
    await next();
});

app.UseAuthorization();
app.MapControllers();

app.Run();
