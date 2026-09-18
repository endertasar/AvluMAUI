using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace AvluMAUI.Helpers;

public static class AlertHelper
{
    public static async Task ShowErrorAsync(string message, string title = "Hata")
    {
        var page = GetCurrentPage();
        if (page is not null)
            await page.DisplayAlert(title, message, "Tamam");
    }

    public static async Task ShowSuccessAsync(string message, CancellationToken ct = default)
    {
        var toast = Toast.Make(message, ToastDuration.Short, 14);
        await toast.Show(ct);
    }

    public static async Task<bool> ShowConfirmAsync(string message, string title = "Onay",
        string accept = "Evet", string cancel = "Hayır")
    {
        var page = GetCurrentPage();
        if (page is null) return false;
        return await page.DisplayAlert(title, message, accept, cancel);
    }

    private static Page? GetCurrentPage()
    {
        return Application.Current?.Windows
            .OfType<Window>()
            .FirstOrDefault()?
            .Page;
    }
}
