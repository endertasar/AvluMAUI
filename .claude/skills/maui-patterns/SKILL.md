---
name: maui-patterns
description: SiteYonetim'in MAUI uygulamalarında (AdminApp ve ResidentApp) UI, ViewModel, sayfa, API çağrısı, token saklama, navigasyon veya OneSignal işi yaparken MUTLAKA bu skill'i uygula. MVVM (CommunityToolkit.Mvvm), ortak SiteYonetim.Shared kullanımı, typed ApiClient, SecureStorage token yönetimi, Shell navigasyon ve sakin çoklu-mülk seçimi burada tanımlıdır. "sayfa", "ekran", "ViewModel", "MAUI", "ApiClient", "bildirim", "login ekranı" gibi her durumda geçerli.
---

# MAUI Desenleri

İki uygulama: `SiteYonetim.AdminApp` ve `SiteYonetim.ResidentApp`. Her ikisi de `SiteYonetim.Shared`'ı referans alır. Ortak kod (DTO, ApiClient, auth, OneSignal wrapper) `Shared` içinde; app'lerde TEKRARLANMAZ.

## Mimari

- **MVVM** — `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`). Code-behind'da iş mantığı YOK.
- **Navigasyon** — .NET MAUI **Shell**, route tabanlı.
- **DI** — `MauiProgram.CreateMauiApp` içinde servis, ViewModel ve sayfalar kaydedilir.
- **UI metinleri Türkçe**, kod İngilizce.

## ViewModel Deseni

```csharp
public partial class PropertyListViewModel(IApiClient api) : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? error;
    public ObservableCollection<PropertyResponse> Items { get; } = new();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true; Error = null;
        try
        {
            var page = await api.GetPropertiesAsync(page: 1, pageSize: 20);
            Items.Clear();
            foreach (var p in page.Items) Items.Add(p);
        }
        catch (ApiException ex) { Error = ex.UserMessage; }
        finally { IsBusy = false; }
    }
}
```

Her ekranda `IsBusy` (yükleniyor) ve `Error` (kullanıcıya Türkçe mesaj) durumları tutulur.

## Typed ApiClient (Shared)

- `HttpClient` typed olarak yapılandırılır: base address, JSON options, `Authorization: Bearer` header.
- **401 → refresh:** Bir `DelegatingHandler`, 401 alınca refresh token ile yeni access token alır, isteği bir kez tekrar dener; refresh de başarısızsa login'e yönlendirir.
- API hatası (`ProblemDetails`) `ApiException`'a çevrilir; içinde kullanıcıya gösterilecek Türkçe `UserMessage` bulunur.

```csharp
public interface IApiClient
{
    Task<AuthResult> AdminLoginAsync(string siteUsername, string username, string password);
    Task<AuthResult> ResidentLoginAsync(string phone, string password);
    Task<PagedResult<PropertyResponse>> GetPropertiesAsync(int page, int pageSize);
    Task<IReadOnlyList<ResidentPropertyResponse>> GetMyPropertiesAsync();
    // ...
}
```

## Token Saklama

- Access + refresh token **`SecureStorage`** içinde saklanır (düz `Preferences` DEĞİL).
- Uygulama açılışında token varsa doğrula/oto-login; yoksa login ekranı.
- Logout: token'ları `SecureStorage`'dan sil + API `logout` çağır.

## Kimlik Akışları

- **AdminApp login:** 3 alan (siteUsername, username, password) → `AdminLoginAsync`.
- **ResidentApp login:** telefon + şifre → `ResidentLoginAsync`.
- **Sakin çoklu-mülk seçimi:** Login sonrası `GetMyPropertiesAsync` birden fazla mülk dönerse önce bir **seçim ekranı** gösterilir; seçilen `propertyId` sonraki ekranlarda (borç, tahsilat) kullanılır. Tek mülk varsa doğrudan geçilir.

## OneSignal (ağırlıklı ResidentApp)

- OneSignal SDK `MauiProgram`'da başlatılır (App Id yapılandırmadan).
- **Login sonrası** OneSignal user/player Id alınır ve `PUT /resident/onesignal` ile kaydedilir; böylece yönetici hedefli bildirim gönderebilir.
- Bildirim listesi API'den çekilir; açılınca `POST /resident/notifications/{id}/read`.
- OneSignal entegrasyonu `Shared` içindeki bir `INotificationService` arkasına alınır; app'ler doğrudan SDK'ya bağlanmaz.

## Yapma

- İş mantığını code-behind'a koyma; ViewModel'e taşı.
- DTO/ApiClient'ı app içinde tekrar tanımlama; `Shared`'ı kullan.
- Token'ı düz `Preferences`/dosyada saklama.
- API çağrısını try/catch'siz yapma; `IsBusy`/`Error` durumlarını atlama.
- Base URL, App Id gibi ayarları koda gömme; yapılandırmadan al.
