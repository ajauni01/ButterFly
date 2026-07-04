using CommunityToolkit.Mvvm.ComponentModel;
using Refit;

namespace Butterfly.Mobile.ViewModels;

/// <summary>
/// Shared state for data-backed screens: busy, error, and empty. Derived view models call
/// <see cref="RunAsync"/> to get consistent loading/error handling with a friendly message.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty] private string _title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty] private bool _isEmpty;

    public bool IsNotBusy => !IsBusy;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>Runs an operation with busy tracking and consistent error surfacing. Returns success.</summary>
    protected async Task<bool> RunAsync(Func<Task> operation)
    {
        if (IsBusy) return false;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await operation();
            return true;
        }
        catch (ApiException ex)
        {
            ErrorMessage = await FriendlyMessage(ex);
            return false;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Can't reach the server. Check your connection and try again.";
            return false;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task<string> FriendlyMessage(ApiException ex)
    {
        // The API returns our ErrorDto envelope; surface its message when present.
        try
        {
            var dto = await ex.GetContentAsAsync<Butterfly.Shared.Dtos.ErrorDto>();
            if (dto is not null && !string.IsNullOrWhiteSpace(dto.Message))
                return dto.Message;
        }
        catch
        {
            // fall through to a generic message
        }

        return ex.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Your session expired. Please sign in again.",
            System.Net.HttpStatusCode.Forbidden => "You don't have permission to do that.",
            System.Net.HttpStatusCode.NotFound => "That item couldn't be found.",
            _ => "Something went wrong. Please try again."
        };
    }
}
