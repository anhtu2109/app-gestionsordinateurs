using System.Diagnostics;

namespace GestionOrdinateurs.API.Middleware;

public class RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var chrono = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            // Après l'authentification, le nom provient du JWT validé. Ne jamais journaliser le token.
            logger.LogInformation("Utilisateur={User} {Method} {Path} -> {Status} en {Ms} ms",
                context.User.Identity?.Name ?? "Anonyme", context.Request.Method,
                context.Request.Path, context.Response.StatusCode, chrono.ElapsedMilliseconds);
        }
    }
}
