using AvluMAUI.ViewModels.Notifications;

namespace AvluMAUI.Views.Notifications;

public partial class NotificationHistoryPage : ContentPage
{
    public NotificationHistoryPage(NotificationHistoryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is NotificationHistoryViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
