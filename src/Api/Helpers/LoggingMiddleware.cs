using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Claims;
using Datadog.Trace;
using Microsoft.AspNetCore.Http.Features;
using Passwordless.Api.Authorization;

namespace Passwordless.Api.Helpers;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        
        var activityFeature = context.Features.Get<IHttpActivityFeature>();
        if (!TryGetAppId(context.User, out var appId))
        {
            await _next(context);
            return;
        }

        using var scope = _logger.BeginScope(new AppIdLogScope(appId));
        
        // setup spanTags
        Tracer.Instance.ActiveScope?.Span.SetTag("appid", appId);
        activityFeature?.Activity.AddTag("appid", appId);
        activityFeature?.Activity.AddBaggage("appid", appId);
        
        
        await _next(context);
    }

    private static bool TryGetAppId(ClaimsPrincipal user, [NotNullWhen(true)] out string? appId)
    {
        if (user.Identity is not { IsAuthenticated: true })
        {
            appId = null;
            return false;
        }

        appId = user.FindFirstValue(CustomClaimTypes.AccountName);
        return appId != null;
    }

    private sealed class AppIdLogScope : IReadOnlyList<KeyValuePair<string, object>>
    {
        private readonly string _appId;
        private string? _cachedString;

        public AppIdLogScope(string appId)
        {
            _appId = appId;
        }

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return new KeyValuePair<string, object>("AppId", _appId);
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public int Count => 1;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            if (_cachedString == null)
            {
                _cachedString = string.Format(
                    CultureInfo.InvariantCulture,
                    "AppId:{0}",
                    _appId);
            }

            return _cachedString;
        }
    }
}