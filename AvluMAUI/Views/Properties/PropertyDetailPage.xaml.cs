using AvluMAUI.ViewModels.Properties;

namespace AvluMAUI.Views.Properties;

public partial class PropertyDetailPage : ContentPage
{
    public PropertyDetailPage(PropertyDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
