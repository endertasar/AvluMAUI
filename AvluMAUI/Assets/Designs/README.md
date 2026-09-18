# Avlu · MAUI Login Module

HTML'deki **Variation A · Klasik Kart** tasarımının birebir MAUI karşılığı. 5 ekran: Splash, Login, Kayıt Ol, Şifremi Unuttum, SMS Doğrulama.

## Dosya yapısı

```
maui/
├── App.xaml / App.xaml.cs                     ← ResourceDictionary birleştirme
├── AppShell.xaml / AppShell.xaml.cs           ← Route tanımları
├── MauiProgram.cs                             ← Font kayıtları
├── Controls/
│   └── AvluLogomark.cs                        ← SVG logomark (C#)
├── Resources/
│   ├── Fonts/                                 ← Inter-Regular.ttf, Inter-SemiBold.ttf (SEN EKLE)
│   └── Styles/
│       ├── Colors.xaml                        ← Marka paleti (light + dark)
│       ├── Styles.xaml                        ← Tipografi, ContentPage default
│       └── Templates.xaml                     ← Buton, input, OTP stilleri
└── Views/
    ├── SplashPage.xaml(.cs)
    ├── LoginPage.xaml(.cs)
    ├── SignupPage.xaml(.cs)
    ├── ForgotPasswordPage.xaml(.cs)
    └── OtpPage.xaml(.cs)
```

## Mevcut projeye taşıma

1. `maui/` klasöründeki dosyaları kendi `.NET MAUI` projenin kök dizinine kopyala.
2. Namespace'leri (`Avlu`, `Avlu.Views`, `Avlu.Controls`) projenin namespace'iyle değiştir.
3. **Inter fontunu indir** ([rsms.me/inter](https://rsms.me/inter)) ve `Resources/Fonts/` altına `Inter-Regular.ttf`, `Inter-SemiBold.ttf` olarak koy. `.csproj` dosyasına zaten tanımlıysa otomatik alınır; değilse şunu ekle:

   ```xml
   <MauiFont Include="Resources\Fonts\*" />
   ```

4. `eye.png` (şifre göz ikonu) için kendi ikonunu `Resources/Images/` altına ekle veya `FontAwesome`/`Fluent UI` gibi font-icon çözümüne çevir.
5. Derleyip çalıştır: splash → 1.5 sn sonra otomatik login'e geçer.

## Tasarım ↔ Kod eşleşmesi

| HTML öğesi | XAML karşılığı |
|---|---|
| Card input (52px, 14 radius) | `<Border Style="FieldBorder"><Entry Style="FieldEntry"/></Border>` |
| Primary button (lacivert) | `Style="{StaticResource PrimaryButton}"` |
| Role segmented (Sakin / Yönetici) | `LoginPage` içindeki `RoleResidentBtn` + `RoleManagerBtn` |
| TR/EN chip | `LangTrBtn` + `LangEnBtn` |
| OTP 6 kutu + otomatik focus | `OtpPage.xaml` + `OnOtpChanged` |
| Karanlık mod | `AppThemeBinding` — tüm renkler zaten bağlı |

## Karanlık mod testi

`App.xaml.cs`'te manuel test için:

```csharp
App.Current.UserAppTheme = AppTheme.Dark;
```

Ya da cihazın sistem temasını kullan — otomatik uyar.

## Sonraki adımlar

- Telefon numarası için `Behavior` ile `### ### ## ##` maskesi ekle.
- `AuthService` (HTTP → JWT) iskeletini yaz.
- `MVVM` tercih edersen: her sayfa için `ViewModel` + `CommunityToolkit.Mvvm` — şu an code-behind.
- SMS OTP geri sayımını `CountdownBehavior` olarak soyutla.
- Biyometrik giriş: `Plugin.Maui.Biometric` ile Face ID / parmak izi.
