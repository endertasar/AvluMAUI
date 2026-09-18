using AvluMAUI.ViewModels.Properties;

namespace AvluMAUI.Views.Properties;

public partial class PropertyListPage : ContentPage
{
    public PropertyListPage(PropertyListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PropertyListViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
