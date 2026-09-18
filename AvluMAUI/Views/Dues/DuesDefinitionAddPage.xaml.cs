using AvluMAUI.ViewModels.Dues;

namespace AvluMAUI.Views.Dues;

public partial class DuesDefinitionAddPage : ContentPage
{
    public DuesDefinitionAddPage(DuesDefinitionAddViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
