using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Avlu.Controls;
namespace Avlu.AdminApp.Views;
public partial class SubUsersPage : ContentPage
{
    public SubUsersPage()
    {
        InitializeComponent();
        UsersList.ItemsSource = new[]
        {
            new { Name = "Fatma Güneş", Username = "@fatma.g", RoleLabel = "Owner", RoleTone = Badge.Tone.Owner, ActiveLabel = "Aktif", ActiveColor = Color.FromArgb("#2E7D57"), OpacityValue = 1.0 },
            new { Name = "Kemal Aydın", Username = "@kemal.a", RoleLabel = "Alt Kullanıcı", RoleTone = Badge.Tone.Neutral, ActiveLabel = "Aktif", ActiveColor = Color.FromArgb("#2E7D57"), OpacityValue = 1.0 },
            new { Name = "Deniz Yıl", Username = "@deniz.y", RoleLabel = "Alt Kullanıcı", RoleTone = Badge.Tone.Neutral, ActiveLabel = "Pasif", ActiveColor = Color.FromArgb("#5C6A7E"), OpacityValue = 0.55 },
        };
    }
    private async void OnAddTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("subuser-form");
}
