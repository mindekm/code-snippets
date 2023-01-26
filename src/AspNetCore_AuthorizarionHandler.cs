services.AddSingleton<IAuthorizationHandler, AccessLevelHandler>();
services.AddAuthorization(o =>
{
    o.AddPolicy("CanModifyFoo", builder =>
    {
        builder
            .RequireAuthenticatedUser()
            .RequireAccessLevel(AccessLevel.Administrator)
    });
});

public enum AccessLevel
{
    None = 0,
    User = 1,
    Administrator = 2,
}

public sealed class AccessLevelRequirement : IAuthorizationRequirement
{
    public AccessLevelRequirement(AccessLevel requiredLevel)
        : this((int)requiredLevel)
    {        
    }

    public AccessLevelRequirement(int requiredLevel)
    {
        RequiredLevel = requiredLevel;
    }

    public int RequiredLevel { get; }
}

public sealed class AccessLevelHandler : AuthorizationHandler<AccessLevelRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessLevelRequirement requirement)
    {
        foreach (var claim in context.User.Claims)
        {
            if (claim.Type == "acc")
            {
                if (int.TryParse(claim.Value, out var level))
                {
                    if (level >= requirement.RequiredLevel)
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}

public static class PolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequireAccessLevel(this AuthorizationPolicyBuilder builder, AccessLevel level)
    {
        builder.Requirements.Add(new AccessLevelRequirement(level));
        return builder;
    }
}
