using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SecureTokenAPI.DB;
using System;
using System.Threading.Tasks;

public class TokenMiddleware
    {
    private readonly RequestDelegate _next;

    public TokenMiddleware(RequestDelegate next)
        {
        _next = next;
        }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
        // Pominięcie autoryzacji dla określonych endpointów
        if (context.Request.Path.StartsWithSegments("/api/users", StringComparison.OrdinalIgnoreCase) ||
              context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
            {
            await _next(context);
            return;
            }

        if (!context.Request.Headers.TryGetValue("Authorization", out var extractedToken))
            {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token not provided");
            return;
            }

        if (!Guid.TryParse(extractedToken, out var token))
            {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid token format");
            return;
            }

        var userToken = await dbContext.UserTokens.FirstOrDefaultAsync(t => t.Token == token && t.ExpirationDate > DateTime.UtcNow);
        if (userToken == null)
            {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized access or token expired");
            return;
            }

        await _next(context);
        }
    }
