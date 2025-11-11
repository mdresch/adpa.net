namespace ADPA.Web.Services;

public class AppState
{
    public event Action? OnChange;
    
    public bool IsLoading { get; private set; }
    public string? CurrentUser { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void SetLoading(bool isLoading)
    {
        IsLoading = isLoading;
        NotifyStateChanged();
    }

    public void SetCurrentUser(string? user)
    {
        CurrentUser = user;
        NotifyStateChanged();
    }

    public void SetError(string? error)
    {
        ErrorMessage = error;
        NotifyStateChanged();
    }

    public void ClearError()
    {
        ErrorMessage = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}