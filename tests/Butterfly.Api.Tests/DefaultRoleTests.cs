using System.Security.Claims;
using Butterfly.Api.Auth;
using FluentAssertions;
using Xunit;

namespace Butterfly.Api.Tests;

/// <summary>
/// Self-service sign-ups (e.g. Google) with no Entra App Role default to Mentor;
/// explicitly assigned CareManager/Admin roles are preserved.
/// </summary>
public class DefaultRoleTests
{
    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "test", nameType: "name", roleType: ClaimTypes.Role));

    [Fact]
    public async Task Role_less_user_is_granted_Mentor()
    {
        var principal = Principal(new Claim("oid", "google-user-1"));

        var result = await new DefaultRoleClaimsTransformation().TransformAsync(principal);

        result.IsInRole(AppRoles.Mentor).Should().BeTrue();
    }

    [Fact]
    public async Task Unauthenticated_principal_is_untouched()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // not authenticated

        var result = await new DefaultRoleClaimsTransformation().TransformAsync(principal);

        result.IsInRole(AppRoles.Mentor).Should().BeFalse();
    }

    [Theory]
    [InlineData(AppRoles.CareManager)]
    [InlineData(AppRoles.Admin)]
    public async Task Assigned_role_is_preserved_and_not_downgraded_to_Mentor(string role)
    {
        var principal = Principal(new Claim(ClaimTypes.Role, role));

        var result = await new DefaultRoleClaimsTransformation().TransformAsync(principal);

        result.IsInRole(role).Should().BeTrue();
        result.IsInRole(AppRoles.Mentor).Should().BeFalse();
    }

    [Fact]
    public async Task Transformation_is_idempotent()
    {
        var transformer = new DefaultRoleClaimsTransformation();
        var principal = Principal(new Claim("oid", "google-user-2"));

        await transformer.TransformAsync(principal);
        var result = await transformer.TransformAsync(principal);

        result.FindAll(ClaimTypes.Role).Should().ContainSingle(c => c.Value == AppRoles.Mentor);
    }
}
