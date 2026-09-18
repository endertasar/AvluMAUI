# Avlu · MAUI (Login + AdminApp + ResidentApp)

HTML'deki **Variation A · Klasik Kart** tasarımının birebir MAUI karşılığı.
Üç bölüm: ortak Login/Splash akışı, **AdminApp** (Yönetici) ve **ResidentApp** (Sakin).
Gerçek projede bunlar muhtemelen iki ayrı .NET MAUI çözümü (solution) olacak — bu paket
ikisinin de aynı paylaşılan `Controls/`, `Resources/`, `Converters/` üzerinden türediğini
gösterir; kendi projene taşırken AdminApp/ResidentApp klasörlerini ayrı projelere,
kökteki paylaşılanları ortak bir class library'ye koyman önerilir.

## Dosya yapısı

```
maui/
├── App.xaml / App.xaml.cs                     ← Ortak ResourceDictionary birleştirme
├── AppShell.xaml(.cs)                          ← Login/Splash akışı (ortak giriş noktası)
├── MauiProgram.cs                              ← Font kayıtları
├── Controls/                                   ← PAYLAŞILAN (her iki app da kullanır)
│   ├── AvluLogomark.cs, AvluField.cs, RoleSegmented.cs
│   ├── StatusPill.cs      ← Bekliyor/Kısmi/Ödendi/Gecikmiş rozeti
│   ├── Badge.cs           ← Daire/Dükkan/Otopark, Owner/Alt Kullanıcı vb.
│   ├── SummaryCard.cs     ← Panel özet kartı
│   ├── AlertRow.cs        ← Panel uyarı satırı
│   ├── ProgressBarView.cs ← Tahsilat/ek ödeme ilerleme çubuğu
│   └── DataStateView.cs   ← Loading/Empty/Error/Content kabuğu (bkz. aşağı)
├── Converters/
│   └── BoolToSuccessColorConverter.cs
├── Resources/Styles/       ← Colors.xaml, Styles.xaml, Templates.xaml (OtpBox dahil)
├── Views/                  ← Splash, Login, Signup, Forgot, Otp (ortak giriş akışı)
├── AdminApp/
│   ├── AdminShell.xaml(.cs)   ← 5 tab: Panel, Mülkler, Tahsilat, Raporlar, Diğer
│   └── Views/                 ← Dashboard, PropertyList/Detail/Form, DuesDefinition,
│                                 Collect (Property/Debt/Form/Success), ExtraPayments
│                                 (List/Form/Detail), Expenses (List/Form), Reports,
│                                 NotificationSend, SubUsers (List/Form), More
└── ResidentApp/
    ├── ResidentShell.xaml(.cs) ← 4 tab: Borçlarım, Ödemelerim, Bildirimler, Profil
    └── Views/                  ← ResidentSignup, PropertySelection, Debts, Payments,
                                    Notifications (List/Detail), Profile
```

## Loading / Empty / Error durumları

Brief'te "her ekranda" istenen üç durum, ayrı XAML sayfaları olarak DEĞİL —
`DataStateView` adlı paylaşılan kabukla çözüldü (gerçek MAUI uygulamalarında da
böyle yapılır: state route değil, aynı sayfanın farklı görünümüdür):

```xml
<controls:DataStateView x:Name="StateHost" CurrentState="Content"
                         EmptyTitle="..." EmptySub="..." ErrorMessage="...">
    <controls:DataStateView.ContentViewContent>
        <!-- gerçek liste/içerik burada -->
    </controls:DataStateView.ContentViewContent>
</controls:DataStateView>
```

Code-behind'dan `StateHost.CurrentState = DataStateView.ViewState.Loading` gibi
ata değiştirirsin — ViewModel'e bağlarsan otomatik güncellenir.
DashboardPage, PropertyListPage, ExtraPaymentsListPage, ExpensesListPage,
DebtsPage, PaymentsPage, NotificationsPage bu kabuğu kullanır.

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
- `AuthService` (HTTP → JWT) iskeletini yaz; AdminApp/ResidentApp aynı servisi paylaşabilir (rol claim'i ile ayrışır).
- `MVVM` tercih edersen: her sayfa için `ViewModel` + `CommunityToolkit.Mvvm` — şu an code-behind + örnek statik veri.
- SMS OTP geri sayımını `CountdownBehavior` olarak soyutla.
- Biyometrik giriş: `Plugin.Maui.Biometric` ile Face ID / parmak izi.
- Raporlar sayfasındaki çubuk grafik yer tutucu — gerçek projede `Microcharts.Maui` veya `SkiaSharp` öner.
- İki ayrı .NET MAUI projesi kuracaksan: `Controls/`, `Resources/`, `Converters/`, ortak `Views/` (Login akışı) içeriğini bir `Avlu.Shared` class library'ye taşı, AdminApp/ResidentApp'i ondan referansla.
