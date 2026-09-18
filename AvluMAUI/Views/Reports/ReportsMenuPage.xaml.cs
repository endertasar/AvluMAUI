using AvluMAUI.Helpers;
using AvluMAUI.ViewModels.Reports;

namespace AvluMAUI.Views.Reports;

public partial class ReportsMenuPage : ContentPage
{
    public ReportsMenuPage(ReportsMenuViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private void OnMonthlyCollectionTapped(object? sender, TappedEventArgs e) =>
        _ = NavigationHelper.GoToMonthlyCollectionAsync();

    private void OnDebtAgingTapped(object? sender, TappedEventArgs e) =>
        _ = NavigationHelper.GoToDebtAgingAsync();

    private void OnPropertySummaryTapped(object? sender, TappedEventArgs e) =>
        _ = NavigationHelper.GoToPropertySummaryAsync();

    private void OnOverdueTapped(object? sender, TappedEventArgs e) =>
        _ = NavigationHelper.GoToOverdueAsync();

    private void OnSendNotificationTapped(object? sender, TappedEventArgs e) =>
        _ = NavigationHelper.GoToSendNotificationAsync();

    private void OnNotificationHistoryTapped(object? sender, TappedEventArgs e) =>
        _ = NavigationHelper.GoToNotificationHistoryAsync();
}
