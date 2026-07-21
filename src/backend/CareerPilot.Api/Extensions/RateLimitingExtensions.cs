using System.Threading.RateLimiting;
using CareerPilot.Api.Configuration;
using Microsoft.AspNetCore.RateLimiting;

namespace CareerPilot.Api.Extensions;

/// <summary>
/// Rate limiting for the authentication surface.
/// </summary>
internal static class RateLimitingExtensions
{
    public static IServiceCollection AddAuthenticationRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            // 429 rather than the default 503: the request was refused for exceeding a
            // quota, not because the service is unavailable, and clients back off
            // differently for the two.
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiter.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString();
                }

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("CareerPilot.RateLimiting");

                logger.LogWarning(
                    "Rate limit exceeded for {Path} from {RemoteIp}",
                    context.HttpContext.Request.Path.Value,
                    context.HttpContext.Connection.RemoteIpAddress?.ToString());

                await context.HttpContext.Response.WriteAsync(
                    "Too many requests. Retry later.",
                    cancellationToken);
            };

            limiter.AddPolicy(
                RateLimitingOptions.AuthenticationPolicy,
                context => CreateLimiter(
                    context,
                    options.AuthenticationPermitLimit,
                    options.AuthenticationWindowSeconds));

            limiter.AddPolicy(
                RateLimitingOptions.RefreshPolicy,
                context => CreateLimiter(
                    context,
                    options.RefreshPermitLimit,
                    options.RefreshWindowSeconds));
        });

        return services;
    }

    /// <summary>
    /// A fixed window keyed by caller address.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Partitioning by IP rather than by submitted username is deliberate: the username
    /// is attacker-chosen, so a per-username limit is bypassed simply by varying it,
    /// and it lets an attacker lock a victim out by exhausting their quota.
    /// </para>
    /// <para>
    /// The limitation of an IP key is that callers behind one NAT or proxy share a
    /// bucket. That is why the limits here are generous enough for shared egress and
    /// why lockout, which is per account, remains the primary defence for a targeted
    /// attack. Behind a reverse proxy, configure forwarded-headers middleware or every
    /// request will partition to the proxy's own address and share a single bucket.
    /// </para>
    /// </remarks>
    private static RateLimitPartition<string> CreateLimiter(
        HttpContext context,
        int permitLimit,
        int windowSeconds) =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                // No queue. Holding a failed sign-in attempt open until a slot frees up
                // ties up server resources on exactly the traffic being throttled;
                // rejecting immediately is the point.
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            });
}
