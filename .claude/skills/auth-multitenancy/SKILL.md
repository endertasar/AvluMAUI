---
name: auth-multitenancy
description: SiteYonetim'de kimlik doğrulama, yetkilendirme, JWT, şifre saklama veya tenant (site) izolasyonu içeren HER işte MUTLAKA bu skill'i uygula. 3 alanlı yönetici girişi, telefonla sakin girişi, JWT claim'leri, refresh token, sakinin mülk erişim kontrolü ve SiteId izolasyonu burada tanımlıdır. "login", "kayıt", "token", "yetki", "şifre", "sakin erişimi", "tenant" gibi her durumda geçerli. Güvenlik kritiktir; bu kurallar esnetilmez.
---

# Kimlik Doğrulama & Multi-Tenancy

Güvenlik kritik. Bu kurallar finansal veri koruduğu için esnetilmez.

## Giriş Modelleri

**Yönetici (3 alan):** `siteUsername + username + password`
1. `Sites` içinde `SiteUsername` ile tenant bulunur (yoksa 401).
2. `AdminUsers` içinde `(SiteId, Username)` ile kullanıcı bulunur.
3. `PasswordHash` doğrulanır. Başarılıysa token üretilir.

**Sakin:** `phone + password`
- `Residents` içinde `Phone` ile bulunur (telefon = username). `SiteId` YOK — sakin tenant'lar arası.
- v1'de OTP yok; `IsPhoneVerified` daima 1 (v2'de aktive edilecek, kolon silinmez).

## Şifre Saklama

- Şifreler **asla düz metin / MD5 / SHA** değil. **BCrypt** veya ASP.NET Core `PasswordHasher<T>` (PBKDF2) kullan.
- Hash'ler `PasswordHash` kolonunda. Doğrulama sabit-zamanlı karşılaştırma ile.

```csharp
// BCrypt.Net-Next örneği
var hash = BCrypt.Net.BCrypt.HashPassword(password);
var ok   = BCrypt.Net.BCrypt.Verify(password, storedHash);
```

## JWT Claim'leri

Access token kısa ömürlü (ör. 15 dk), refresh token uzun ömürlü + rotasyonlu.

- Ortak: `sub` (kullanıcı Id), `user_type` = `admin | resident`.
- Yönetici ek: `site_id`, `role` (Owner | SubUser).
- Sakinde `site_id` YOK.

```csharp
var claims = new List<Claim>
{
    new("sub", user.Id.ToString()),
    new("user_type", "admin"),
    new("site_id", user.SiteId.ToString()),
    new("role", user.Role)
};
```

## Tenant İzolasyonu (en kritik kural)

- Yönetici endpoint'lerinde `SiteId` **daima token'daki `site_id` claim'inden** okunur.
- İstemci gövdesinde/parametresinde `SiteId` gelse bile **yok sayılır**.
- Bir yardımcı üzerinden eriş:

```csharp
public static long GetSiteId(this ClaimsPrincipal user) =>
    long.Parse(user.FindFirstValue("site_id")
        ?? throw new UnauthorizedAccessException());
```

Controller içinde `User.GetSiteId()` ile erişilir. Repository/Service çağrılarında `SiteId` her zaman parametre olarak geçer; sorgular `WHERE SiteId = @siteId` filtreler.

## Sakin Erişim Kontrolü (telefon eşleşmesi)

Sakinin `SiteId`'si olmadığı için izolasyon **telefon eşleşmesiyle** yapılır:

- Sakin yalnızca telefonuyla eşleşen mülklere erişebilir:
  `Properties.OwnerPhone = @phone OR Properties.ResidentPhone = @phone`.
- Her `/resident/...` endpoint'i, istenen `propertyId`'nin sakinin telefonuyla eşleştiğini **doğrular**; eşleşmezse `403`.
- Telefon token'daki `sub` → `Residents.Phone` üzerinden alınır; istemciden telefon parametresi ALINMAZ.

```csharp
// Örnek guard
var phone = await residents.GetPhoneAsync(residentId, ct);
var owns  = await properties.IsPhoneLinkedAsync(propertyId, phone, ct);
if (!owns) return Results.Forbid();
```

## Yetkilendirme Politikaları

- `AdminOnly` — `user_type = admin`.
- `OwnerOnly` — `user_type = admin AND role = Owner` (alt kullanıcı oluşturma vb.).
- `ResidentOnly` — `user_type = resident`.

```csharp
options.AddPolicy("OwnerOnly", p =>
    p.RequireClaim("user_type", "admin").RequireClaim("role", "Owner"));
```

## Refresh Token

- `RefreshTokens` tablosunda saklanır (hash'lenmiş tercih edilir), `ExpiresAt` + `RevokedAt`.
- Yenilemede eski token revoke edilir, yenisi verilir (rotasyon).
- Logout eski token'ı revoke eder.

## Yapma

- `SiteId`'yi istemciden okuma.
- Şifreyi düz/zayıf hash'le saklama.
- Sakin endpoint'inde telefonu istemciden alma veya erişim guard'ını atlama.
- Access token'ı uzun ömürlü yapma.
- `IsPhoneVerified` kolonunu kaldırma (v2 OTP için ayrılmış).
