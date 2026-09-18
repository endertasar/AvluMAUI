using AvluMAUI.ViewModels.Dues;

namespace AvluMAUI.Views.Dues;

public partial class DuesDefinitionListPage : ContentPage
{
    public DuesDefinitionListPage(DuesDefinitionListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DuesDefinitionListViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
