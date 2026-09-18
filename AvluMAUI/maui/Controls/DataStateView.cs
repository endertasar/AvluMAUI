using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System.Windows.Input;

namespace Avlu.Controls;

/// <summary>
/// Liste ekranlarının ortak durum kabuğu: Loading / Empty / Error / Content.
/// Kullanım: ContentPage içine koy, ViewModel'deki State'e bağla, dört slotu doldur.
/// </summary>
public class DataStateView : ContentView
{
    public enum ViewState { Loading, Empty, Error, Content }

    public static readonly BindableProperty CurrentStateProperty =
        BindableProperty.Create(nameof(CurrentState), typeof(ViewState), typeof(DataStateView), ViewState.Content,
            propertyChanged: OnStateChanged);
    public static readonly BindableProperty ContentViewProperty =
        BindableProperty.Create(nameof(ContentViewContent), typeof(View), typeof(DataStateView), null, propertyChanged: OnStateChanged);
    public static readonly BindableProperty EmptyTitleProperty =
        BindableProperty.Create(nameof(EmptyTitle), typeof(string), typeof(DataStateView), "Kayıt bulunamadı");
    public static readonly BindableProperty EmptySubProperty =
        BindableProperty.Create(nameof(EmptySub), typeof(string), typeof(DataStateView), "");
    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(DataStateView), "Bir şeyler ters gitti.");
    public static readonly BindableProperty RetryCommandProperty =
        BindableProperty.Create(nameof(RetryCommand), typeof(ICommand), typeof(DataStateView), null);

    public ViewState CurrentState { get => (ViewState)GetValue(CurrentStateProperty); set => SetValue(CurrentStateProperty, value); }
    public View ContentViewContent { get => (View)GetValue(ContentViewProperty); set => SetValue(ContentViewProperty, value); }
    public string EmptyTitle { get => (string)GetValue(EmptyTitleProperty); set => SetValue(EmptyTitleProperty, value); }
    public string EmptySub { get => (string)GetValue(EmptySubProperty); set => SetValue(EmptySubProperty, value); }
    public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty); set => SetValue(ErrorMessageProperty, value); }
    public ICommand RetryCommand { get => (ICommand)GetValue(RetryCommandProperty); set => SetValue(RetryCommandProperty, value); }

    private readonly VerticalStackLayout _loadingHost = new() { Spacing = 12 };
    private readonly VerticalStackLayout _emptyHost = new() { Spacing = 6, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 60, 0, 0) };
    private readonly VerticalStackLayout _errorHost = new() { Spacing = 6, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 60, 0, 0) };
    private readonly ContentView _contentHost = new();
    private readonly Grid _root = new();

    public DataStateView()
    {
        BuildLoading();
        BuildEmpty();
        BuildError();
        _root.Children.Add(_loadingHost);
        _root.Children.Add(_emptyHost);
        _root.Children.Add(_errorHost);
        _root.Children.Add(_contentHost);
        base.Content = _root;
        Refresh();
    }

    private void BuildLoading()
    {
        for (int i = 0; i < 4; i++)
        {
            _loadingHost.Children.Add(new Border
            {
                HeightRequest = 68,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                BackgroundColor = Color.FromArgb("#EEF1F5"),
            });
        }
    }

    private void BuildEmpty()
    {
        var iconBox = new Border { WidthRequest = 56, HeightRequest = 56, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 16 }, BackgroundColor = Color.FromArgb("#F0F3F8"), HorizontalOptions = LayoutOptions.Center };
        var title = new Label { FontFamily = "InterSemiBold", FontSize = 16, HorizontalTextAlignment = TextAlignment.Center };
        title.SetBinding(Label.TextProperty, new Binding(nameof(EmptyTitle), source: this));
        var sub = new Label { FontFamily = "Inter", FontSize = 13, HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromArgb("#5C6A7E"), MaxLines = 3 };
        sub.SetBinding(Label.TextProperty, new Binding(nameof(EmptySub), source: this));
        _emptyHost.Children.Add(iconBox);
        _emptyHost.Children.Add(title);
        _emptyHost.Children.Add(sub);
    }

    private void BuildError()
    {
        var iconBox = new Border { WidthRequest = 56, HeightRequest = 56, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 16 }, BackgroundColor = Color.FromArgb("#FBEAE9"), HorizontalOptions = LayoutOptions.Center };
        var title = new Label { Text = "Yüklenemedi", FontFamily = "InterSemiBold", FontSize = 16, HorizontalTextAlignment = TextAlignment.Center };
        var msg = new Label { FontFamily = "Inter", FontSize = 13, HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromArgb("#5C6A7E") };
        msg.SetBinding(Label.TextProperty, new Binding(nameof(ErrorMessage), source: this));
        var retry = new Button { Text = "Tekrar Dene", FontFamily = "InterSemiBold", HeightRequest = 44, CornerRadius = 12, BackgroundColor = Colors.Transparent, BorderColor = Color.FromArgb("#E4E8EF"), BorderWidth = 1.5, TextColor = Color.FromArgb("#0E2A47") };
        retry.SetBinding(Button.CommandProperty, new Binding(nameof(RetryCommand), source: this));
        _errorHost.Children.Add(iconBox);
        _errorHost.Children.Add(title);
        _errorHost.Children.Add(msg);
        _errorHost.Children.Add(retry);
    }

    private static void OnStateChanged(BindableObject b, object o, object n) => ((DataStateView)b).Refresh();

    private void Refresh()
    {
        _loadingHost.IsVisible = CurrentState == ViewState.Loading;
        _emptyHost.IsVisible = CurrentState == ViewState.Empty;
        _errorHost.IsVisible = CurrentState == ViewState.Error;
        _contentHost.IsVisible = CurrentState == ViewState.Content;
        _contentHost.Content = ContentViewContent;
    }
}
