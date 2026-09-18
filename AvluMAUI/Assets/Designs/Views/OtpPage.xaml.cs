using Microsoft.Maui.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Avlu.Views;

public partial class OtpPage : ContentPage
{
    private Entry[] _boxes = null!;
    private int _remaining = 42;
    private CancellationTokenSource? _timerCts;

    public OtpPage()
    {
        InitializeComponent();
        _boxes = new[] { Otp0, Otp1, Otp2, Otp3, Otp4, Otp5 };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _timerCts = new CancellationTokenSource();
        _ = TickAsync(_timerCts.Token);
        Otp0.Focus();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timerCts?.Cancel();
    }

    private async Task TickAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _remaining > 0)
        {
            ResendSpan.Text = $"Tekrar gönder ({_remaining / 60:00}:{_remaining % 60:00})";
            try { await Task.Delay(1000, ct); } catch { return; }
            _remaining--;
        }
        if (!ct.IsCancellationRequested) ResendSpan.Text = "Tekrar gönder";
    }

    private void OnOtpChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry) return;
        // Sadece rakam
        if (!string.IsNullOrEmpty(e.NewTextValue) && !int.TryParse(e.NewTextValue, out _))
        {
            entry.Text = e.OldTextValue;
            return;
        }
        if (entry.Text?.Length >= 1)
        {
            var idx = Array.IndexOf(_boxes, entry);
            if (idx >= 0 && idx < _boxes.Length - 1) _boxes[idx + 1].Focus();
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnVerify(object sender, EventArgs e)
    {
        var code = string.Concat(_boxes.Select(b => b.Text ?? ""));
        // TODO: AuthService.VerifyOtpAsync(code)
        await Shell.Current.GoToAsync("//home");
    }
}
