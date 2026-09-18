using AvluMAUI.ViewModels.Notifications;

namespace AvluMAUI.Views.Notifications;

public partial class NotificationSendPage : ContentPage
{
    public NotificationSendPage(NotificationSendViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
