using AvluMAUI.ViewModels.Properties;

namespace AvluMAUI.Views.Properties;

public partial class AssignResidentPage : ContentPage
{
    public AssignResidentPage(AssignResidentViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
