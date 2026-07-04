using Butterfly.Api.Auth;
using Butterfly.Shared.Enums;

namespace Butterfly.Api.Tests.TestSupport;

/// <summary>Test double for <see cref="ICurrentUser"/> with a settable role.</summary>
public sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; init; } = true;
    public string EntraObjectId { get; init; } = "test-oid";
    public string Email { get; init; } = "test@butterfly.dev";
    public string DisplayName { get; init; } = "Test User";
    public UserRole Role { get; init; }
}
