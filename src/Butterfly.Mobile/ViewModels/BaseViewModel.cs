using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Identity.Client;
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
        catch (OperationCanceledException)
        {
            ErrorMessage = "Sign-in was cancelled.";
            return false;
        }
        catch (MsalClientException ex) when (ex.ErrorCode == MsalError.AuthenticationCanceledError)
        {
            ErrorMessage = "Sign-in was cancelled.";
            return false;
        }
        catch (MsalServiceException ex)
        {
            ErrorMessage = BuildMsalServiceError(ex);
            return false;
        }
        catch (MsalException ex)
        {
            ErrorMessage = $"Authentication failed ({ex.ErrorCode}). Please try again.";
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

    // Surface Entra's concrete AADSTS/AADB2C code when present so tenant/user-flow issues are diagnosable.
    private static string BuildMsalServiceError(MsalServiceException ex)
    {
        var entraCode = ExtractEntraCode(ex.Message);
        return entraCode is null
            ? $"Authentication failed ({ex.ErrorCode}). {ex.Message}"
            : $"Authentication failed ({ex.ErrorCode}, {entraCode}). {ex.Message}";
    }

    private static string? ExtractEntraCode(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        foreach (var prefix in new[] { "AADSTS", "AADB2C" })
        {
            var idx = message.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                continue;

            var end = idx;
            while (end < message.Length && (char.IsLetterOrDigit(message[end]) || message[end] == '_'))
                end++;

            return message[idx..end];
        }

        return null;
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
