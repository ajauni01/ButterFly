using Butterfly.Shared.Enums;

namespace Butterfly.Api.Auth;

/// <summary>
/// Typed access to the authenticated principal's Entra claims. Backed by the incoming token —
/// never a database lookup. The <c>oid</c> is the join key to the app-side <c>AppUser</c>.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>The Entra Object ID (<c>oid</c> claim). Stable per user per tenant.</summary>
    string EntraObjectId { get; }

    string Email { get; }
    string DisplayName { get; }

    /// <summary>The single app role from the <c>roles</c> claim, mapped to the domain enum.</summary>
    UserRole Role { get; }
}
