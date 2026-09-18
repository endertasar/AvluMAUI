using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Models.Notification;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Notifications;

public partial class NotificationHistoryViewModel : ObservableObject
{
    private readonly INotificationService _notifications;

    [ObservableProperty] private bool    _isBusy = false;
    [ObservableProperty] private bool    _hasMore = false;
    [ObservableProperty] private string? _errorMessage;

    private int _page = 1;
    private const int PageSize = 20;

    public ObservableCollection<NotificationDto> Notifications { get; } = [];

    public NotificationHistoryViewModel(INotificationService notifications)
    {
        _notifications = notifications;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        _page        = 1;
        IsBusy       = true;
        ErrorMessage = null;
        Notifications.Clear();
        try
        {
            var result = await _notifications.GetHistoryAsync(_page, PageSize);
            if (!result.Success || result.Data is null)
            {
                ErrorMessage = result.Message ?? "Bildirimler yüklenemedi.";
                return;
            }
            foreach (var n in result.Data) Notifications.Add(n);
            HasMore = result.Data.Count == PageSize;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (!HasMore || IsBusy) return;
        _page++;
        IsBusy = true;
        try
        {
            var result = await _notifications.GetHistoryAsync(_page, PageSize);
            if (result.Success && result.Data is not null)
            {
                foreach (var n in result.Data) Notifications.Add(n);
                HasMore = result.Data.Count == PageSize;
            }
        }
        finally { IsBusy = false; }
    }
}
