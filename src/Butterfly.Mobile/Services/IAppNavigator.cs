using Butterfly.Shared.Enums;

namespace Butterfly.Mobile.Services;

/// <summary>Routes the app to the correct top-level UI based on the signed-in user's role.</summary>
public interface IAppNavigator
{
    void ShowRoleHome(UserRole role);
    void ShowLogin();
}
