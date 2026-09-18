namespace AvluMAUI.Helpers;

public static class NavigationHelper
{
        public static Task GoToLoginAsync() =>
        Shell.Current.GoToAsync("//login");

    public static Task GoToMainAsync() =>
        Shell.Current.GoToAsync("//main");

    public static Task GoToPropertyDetailAsync(long propertyId) =>
        Shell.Current.GoToAsync($"properties/detail?propertyId={propertyId}");

    public static Task GoToPropertyAddEditAsync(long? propertyId = null) =>
        propertyId.HasValue
            ? Shell.Current.GoToAsync($"properties/addedit?propertyId={propertyId}")
            : Shell.Current.GoToAsync("properties/addedit");

    public static Task GoToAssignResidentAsync(long propertyId) =>
        Shell.Current.GoToAsync($"properties/assign?propertyId={propertyId}");

    public static Task GoToAddDuesDefinitionAsync() =>
        Shell.Current.GoToAsync("dues/definition/add");

    public static Task GoToBulkChargeAsync() =>
        Shell.Current.GoToAsync("dues/bulk");

    public static Task GoToSingleChargeAsync(long? propertyId = null) =>
        propertyId.HasValue
            ? Shell.Current.GoToAsync($"dues/single?propertyId={propertyId}")
            : Shell.Current.GoToAsync("dues/single");

    public static Task GoToPaymentListAsync(long? propertyId = null) =>
        propertyId.HasValue
            ? Shell.Current.GoToAsync($"payments/list?propertyId={propertyId}")
            : Shell.Current.GoToAsync("payments/list");

    public static Task GoToSendNotificationAsync() =>
        Shell.Current.GoToAsync("notifications/send");

    public static Task GoToNotificationHistoryAsync() =>
        Shell.Current.GoToAsync("notifications/history");

    public static Task GoToMonthlyCollectionAsync() =>
        Shell.Current.GoToAsync("reports/monthly");

    public static Task GoToDebtAgingAsync() =>
        Shell.Current.GoToAsync("reports/aging");

    public static Task GoToPropertySummaryAsync() =>
        Shell.Current.GoToAsync("reports/summary");

    public static Task GoToOverdueAsync() =>
        Shell.Current.GoToAsync("reports/overdue");

    public static Task GoBackAsync() =>
        Shell.Current.GoToAsync("..");
}
