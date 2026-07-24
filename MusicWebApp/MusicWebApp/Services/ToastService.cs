namespace MusicWebApp.Services;

public enum ToastLevel
{
    Info,
    Success,
    Error
}

public class ToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Message { get; init; } = string.Empty;
    public ToastLevel Level { get; init; } = ToastLevel.Info;
}

/// <summary>
/// Service hiển thị thông báo (toast) thống nhất cho toàn app.
/// </summary>
public class ToastService
{
    public event Action<ToastMessage>? OnToast;

    public void ShowInfo(string message) => Show(message, ToastLevel.Info);
    public void ShowSuccess(string message) => Show(message, ToastLevel.Success);
    public void ShowError(string message) => Show(message, ToastLevel.Error);

    private void Show(string message, ToastLevel level)
    {
        OnToast?.Invoke(new ToastMessage { Message = message, Level = level });
    }
}
