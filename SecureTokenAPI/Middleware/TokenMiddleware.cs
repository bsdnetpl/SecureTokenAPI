using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SecureTokenAPI.DB;
using System;
using System.Threading.Tasks;
using NLog;

public class TokenMiddleware
    {
    private readonly RequestDelegate _next;
    private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

    public TokenMiddleware(RequestDelegate next)
        {
        _next = next;
        }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
        // Logowanie ścieżki żądania
        _logger.Info($"Request path: {context.Request.Path}");

        // Pominięcie autoryzacji dla określonych endpointów
        if (context.Request.Path.StartsWithSegments("/api/users", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
            {
            _logger.Info("Skipping token validation for this endpoint");
            await _next(context);
            return;
            }

        // Sprawdzenie, czy nagłówek "Authorization" istnieje
        if (!context.Request.Headers.TryGetValue("Authorization", out var extractedToken))
            {
            _logger.Warn("Token not provided");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token not provided");
            return;
            }

        // Sprawdzenie poprawności formatu tokena
        if (!Guid.TryParse(extractedToken, out var token))
            {
            _logger.Warn("Invalid token format");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid token format");
            return;
            }

        // Sprawdzenie, czy token istnieje w bazie danych i czy jest ważny
        var userToken = await dbContext.UserTokens.FirstOrDefaultAsync(t => t.Token == token && t.ExpirationDate > DateTime.UtcNow);
        if (userToken == null)
            {
            _logger.Warn("Unauthorized access or token expired");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized access or token expired");
            return;
            }

        _logger.Info("Token validated successfully");
        await _next(context);
        }
    }
