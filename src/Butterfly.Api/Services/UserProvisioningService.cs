using Butterfly.Api.Auth;
using Butterfly.Data;
using Butterfly.Data.Entities;
using Butterfly.Shared.Dtos;
using Butterfly.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Butterfly.Api.Services;

/// <summary>
/// Reconciles the app-side <see cref="AppUser"/> with the authenticated Entra principal. On the
/// first authenticated call it inserts the user (keyed by <c>oid</c>) and creates the matching
/// <see cref="Mentor"/> or <see cref="CareManager"/> extension from the role claim; on later calls
/// it refreshes email/display-name if they changed in the identity provider.
/// </summary>
public interface IUserProvisioningService
{
    Task<UserProfileDto> GetOrProvisionCurrentUserAsync(CancellationToken ct = default);

    /// <summary>Resolve the current caller's <see cref="AppUser"/>, provisioning if needed.</summary>
    Task<AppUser> EnsureCurrentAppUserAsync(CancellationToken ct = default);
}

public sealed class UserProvisioningService : IUserProvisioningService
{
    private readonly ButterflyDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserProvisioningService> _logger;

    public UserProvisioningService(ButterflyDbContext db, ICurrentUser currentUser, ILogger<UserProvisioningService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AppUser> EnsureCurrentAppUserAsync(CancellationToken ct = default)
    {
        var oid = _currentUser.EntraObjectId;
        var role = _currentUser.Role; // throws 403 if no recognized app role

        var user = await _db.AppUsers
            .Include(u => u.Mentor).ThenInclude(m => m!.Survey)
            .Include(u => u.CareManager)
            .FirstOrDefaultAsync(u => u.EntraObjectId == oid, ct);

        if (user is null)
        {
            user = new AppUser
            {
                EntraObjectId = oid,
                Email = _currentUser.Email,
                DisplayName = _currentUser.DisplayName,
                Role = role
            };
            _db.AppUsers.Add(user);
            AttachRoleExtension(user, role);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Provisioned new AppUser {UserId} with role {Role}", user.Id, role);
            return user;
        }

        // Keep profile fields fresh from the identity provider.
        var changed = false;
        if (!string.IsNullOrWhiteSpace(_currentUser.Email) && user.Email != _currentUser.Email)
        {
            user.Email = _currentUser.Email;
            changed = true;
        }
        if (!string.IsNullOrWhiteSpace(_currentUser.DisplayName) && user.DisplayName != _currentUser.DisplayName)
        {
            user.DisplayName = _currentUser.DisplayName;
            changed = true;
        }

        // If the role changed in Entra, mirror it and backfill the missing extension record.
        if (user.Role != role)
        {
            _logger.LogInformation("AppUser {UserId} role changed {Old}->{New}", user.Id, user.Role, role);
            user.Role = role;
            AttachRoleExtension(user, role);
            changed = true;
        }
        else if (RoleExtensionMissing(user, role))
        {
            AttachRoleExtension(user, role);
            changed = true;
        }

        if (changed)
            await _db.SaveChangesAsync(ct);

        return user;
    }

    public async Task<UserProfileDto> GetOrProvisionCurrentUserAsync(CancellationToken ct = default)
    {
        var user = await EnsureCurrentAppUserAsync(ct);
        return ToDto(user);
    }

    private void AttachRoleExtension(AppUser user, UserRole role)
    {
        switch (role)
        {
            case UserRole.Mentor when user.Mentor is null:
                user.Mentor = new Mentor { AppUser = user };
                break;
            case UserRole.CareManager when user.CareManager is null:
                user.CareManager = new CareManager { AppUser = user, Region = string.Empty };
                break;
            // Admin has no extension entity.
        }
    }

    private static bool RoleExtensionMissing(AppUser user, UserRole role) => role switch
    {
        UserRole.Mentor => user.Mentor is null,
        UserRole.CareManager => user.CareManager is null,
        _ => false
    };

    private static UserProfileDto ToDto(AppUser user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName,
        Role = user.Role,
        MentorId = user.Mentor?.Id,
        CareManagerId = user.CareManager?.Id,
        HasCompletedSurvey = user.Mentor?.Survey is not null,
        IsVerifiedCareManager = user.Role == UserRole.CareManager ? user.CareManager?.IsVerified : null,
        CreatedAt = user.CreatedAt
    };
}
