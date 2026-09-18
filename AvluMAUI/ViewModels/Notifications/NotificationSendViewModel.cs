using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Notification;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Notifications;

public partial class NotificationSendViewModel : ObservableObject
{
    private readonly INotificationService _notifications;

    [ObservableProperty] private bool    _isBusy      = false;
    [ObservableProperty] private string  _title       = string.Empty;
    [ObservableProperty] private string  _body        = string.Empty;
    [ObservableProperty] private string  _targetType  = "All";
    [ObservableProperty] private string? _targetId;
    [ObservableProperty] private string? _errorMessage;

    public List<string> TargetTypes { get; } = ["All", "Block", "Property", "Resident"];

    public bool NeedsTargetId => TargetType != "All";

    public NotificationSendViewModel(INotificationService notifications)
    {
        _notifications = notifications;
    }

    partial void OnTargetTypeChanged(string value) =>
        OnPropertyChanged(nameof(NeedsTargetId));

    [RelayCommand]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Body))
        {
            ErrorMessage = "Başlık ve içerik zorunludur.";
            return;
        }
        IsBusy       = true;
        ErrorMessage = null;
        try
        {
            var req = new SendNotificationRequest
            {
                Title      = Title.Trim(),
                Body       = Body.Trim(),
                TargetType = TargetType,
                TargetId   = NeedsTargetId ? TargetId : null
            };
            var result = await _notifications.SendAsync(req);
            if (!result.Success) { ErrorMessage = result.Message; return; }
            await AlertHelper.ShowSuccessAsync("Bildirim gönderildi.");
            Title    = string.Empty;
            Body     = string.Empty;
            TargetId = null;
        }
        finally { IsBusy = false; }
    }
}
