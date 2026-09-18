using AvluMAUI.Models;
using AvluMAUI.Models.Notification;
using AvluMAUI.Services.Base;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.Services.Implementations;

public class NotificationService : BaseApiService, INotificationService
{
    public NotificationService(HttpClient http) : base(http) { }

    public Task<ApiResponse<List<NotificationDto>>> GetHistoryAsync(int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        GetAsync<List<NotificationDto>>($"api/v1/notifications?page={page}&pageSize={pageSize}", ct);

    public Task<ApiResponse<bool>> SendAsync(SendNotificationRequest request, CancellationToken ct = default) =>
        PostAsync<bool>("api/v1/notifications/send", request, ct);
}
