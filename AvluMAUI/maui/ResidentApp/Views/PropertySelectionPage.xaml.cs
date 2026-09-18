using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Avlu.Controls;
namespace Avlu.ResidentApp.Views;
public partial class PropertySelectionPage : ContentPage
{
    public PropertySelectionPage()
    {
        InitializeComponent();
        MatchesList.ItemsSource = new[]
        {
            new { Site = "Güneşli Konutları", Unit = "A Blok · D12", RoleLabel = "Mülk Sahibi", RoleTone = Badge.Tone.Owner, DebtDisplay = "Borç: 1.250,00 ₺", DebtColor = Color.FromArgb("#B3261E") },
            new { Site = "Güneşli Konutları", Unit = "A Blok · D14 (Kiracı)", RoleLabel = "Oturan", RoleTone = Badge.Tone.Neutral, DebtDisplay = "Borç yok", DebtColor = Color.FromArgb("#2E7D57") },
            new { Site = "Zümrüt Sitesi", Unit = "B Blok · D2", RoleLabel = "Mülk Sahibi", RoleTone = Badge.Tone.Owner, DebtDisplay = "Borç yok", DebtColor = Color.FromArgb("#2E7D57") },
        };
    }
    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;
        await Shell.Current.GoToAsync("//debts");
    }
}
