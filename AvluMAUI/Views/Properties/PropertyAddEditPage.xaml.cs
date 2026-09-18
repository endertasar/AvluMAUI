using AvluMAUI.ViewModels.Properties;

namespace AvluMAUI.Views.Properties;

public partial class PropertyAddEditPage : ContentPage
{
    public PropertyAddEditPage(PropertyAddEditViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
