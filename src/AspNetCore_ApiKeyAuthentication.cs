services
    .AddAuthorization(o =>
    {
        o.AddPolicy("CanAccessRestrictedResources", builder =>
        {
            builder
                .AddAuthenticationSchemes(ApiKeyDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireRole("Foo");
        });
    })
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer()
    .AddApiKeys();

public static class ApiKeyDefaults
{
    public static readonly string AuthenticationScheme = "ApiKey";
}

public sealed class ApiKeyOptions : AuthenticationSchemeOptions
{
}

public static class ApiKeyExtensions
{
    public static AuthenticationBuilder AddApiKeys(this AuthenticationBuilder builder)
        => AddApiKey(builder, _ => { });

    public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, Action<ApiKeyOptions> configureOptions)
    {
        return builder.AddScheme<ApiKeyOptions, ApiKeyHandler>(ApiKeyDefaults.AuthenticationScheme, configureOptions);
    }
}

public sealed partial class ApiKeyHandler : AuthenticationHandler<ApiKeyOptions>
{
    public ApiKeyHandler(IOptionsMonitor<ApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headerValue = Request.Headers.Authorization.ToString();
        if (!headerValue.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var key = headerValue.Substring("ApiKey ".Length).Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            return AuthenticateResult.NoResult();
        }

        var maybeEntity = await FetchEntityFor(key);
        if (!maybeEntity.TryUnwrap(out var entity))
        {
            KeyValidationFailed(Logger);
            return AuthenticateResult.Fail("Invalid API key");
        }

        KeyValidationSucceeded(Logger);
        var identity = new ClaimsIdentity(ApiKeyDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.Name, entity.Name));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, entity.Id.ToString()));
        foreach (var role in entity.Roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyDefaults.AuthenticationScheme);

        return AuthenticateResult.Success(ticket);
    }

    private async ValueTask<Maybe<ApiKeyEntity>> FetchEntityFor(string key)
    {
        // Data access
        await Task.Delay(10);

        return Maybe.None;
    }

    [LoggerMessage(1, LogLevel.Information, "API key validation failed")]
    public static partial void KeyValidationFailed(ILogger logger);

    [LoggerMessage(2, LogLevel.Debug, "API key validation succeeded")]
    public static partial void KeyValidationSucceeded(ILogger logger);
}

public sealed class ApiKeyEntity
{
    public required Guid Id { get; init; }

    public required string Key { get; init; }

    public required string Name { get; init; }

    public required List<string> Roles { get; init; }
}
