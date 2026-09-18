using AvluMAUI.Models;
using AvluMAUI.Models.Notification;

namespace AvluMAUI.Services.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<List<NotificationDto>>> GetHistoryAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ApiResponse<bool>>                  SendAsync(SendNotificationRequest request, CancellationToken ct = default);
}
