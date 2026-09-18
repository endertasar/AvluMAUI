# CLAUDE.md

Bu dosya, bu repository'de çalışan Claude Code için ana rehberdir. Her oturumda önce bunu, sonra `docs/` altındaki ilgili dokümanı ve `.claude/skills/` altındaki ilgili skill'i oku.

## Proje Özeti

Site / apartman yönetim sistemi. Çok kiracılı (multi-tenant) yapıda, her site ayrı bir tenant. Üç bileşenden oluşur:

1. **API** — `.NET 10` Web API. Diğer iki uygulama buna bağlanır. Veritabanı **SQL Server**, veri erişimi **Dapper** ile yapılır (EF Core KULLANILMAZ).
2. **Yönetici Uygulaması** — `.NET 10 MAUI`. Site/apartman yöneticisinin işlemleri.
3. **Sakin Uygulaması** — `.NET 10 MAUI`. Site sakininin işlemleri.

Bildirimler **OneSignal** üzerinden gönderilir. Para birimi **TRY**, tüm tutarlar `decimal(18,2)`.

## Kimlik / Giriş Modeli

- **Yönetici girişi:** `SiteUsername + Username + Password` (3 alan). Tenant, `SiteUsername` ile çözülür; kullanıcı `(SiteId, Username)` çifti ile bulunur.
- **Sakin girişi:** `Phone + Password`. Telefon numarası username görevi görür. Sakinin `SiteId`'si YOKTUR — telefon numarası tüm tenant'lardaki mülklerle eşleştirilir.
- **Sakin - mülk eşleşmesi:** Sakinin telefonu, bir mülkün `OwnerPhone` VEYA `ResidentPhone` alanıyla eşleşen tüm mülkleri görür (farklı sitelerde olabilir). Birden fazla mülk çıkarsa seçim ekranı gösterilir.
- **OTP notu:** v1'de SMS/OTP YOKTUR. `Residents.IsPhoneVerified` kolonu şimdilik daima `1`; v2'de OTP eklenince şema bozulmadan aktive edilecek. Bu kolonu asla silme.
- Auth: **JWT access token + refresh token**. Yönetici ve sakin için ayrı claim setleri (`user_type = admin | resident`).

## Solution Yapısı

```
SiteYonetim.sln
├─ src/
│  ├─ SiteYonetim.Api/          (.NET 10 Web API, Dapper, background job)
│  ├─ SiteYonetim.Shared/       (DTO'lar, ApiClient, OneSignal wrapper, auth modelleri)
│  ├─ SiteYonetim.AdminApp/     (.NET 10 MAUI)
│  └─ SiteYonetim.ResidentApp/  (.NET 10 MAUI)
├─ db/migrations/               (0001_init.sql, 0002_... — sıralı, elle yazılan SQL)
├─ docs/                        (architecture.md, data-model.md, api-contract.md, roadmap.md)
├─ .claude/skills/              (dapper, api-conventions, maui, auth, git)
└─ CLAUDE.md
```

- `Shared` her iki MAUI app tarafından referans alınır. DTO ve ApiClient tekrarı YAPILMAZ; ortak kod buraya konur.
- İki MAUI uygulaması ayrıdır (AdminApp / ResidentApp), tek app + rol yaklaşımı KULLANILMAZ.

## Teknoloji Kuralları

- Hedef framework: **net10.0** (MAUI için `net10.0-android;net10.0-ios`).
- **Dapper** dışında ORM kullanma. Sorgular parametreli olmalı (SQL injection'a karşı); string birleştirme ile SQL kurma.
- **Migration:** Elle yazılan sıralı SQL script'leri `db/migrations/` altında. Her değişiklik yeni bir numaralı dosya (`0002_...sql`); eski script'leri DÜZENLEME. Uygulama başlangıcında script'leri çalıştırma stratejisi `docs/architecture.md`'de.
- **Soft delete:** Kayıtlar fiziksel silinmez; `IsDeleted = 1` yapılır. Tüm okuma sorguları `IsDeleted = 0` filtreler.
- **Audit:** Finansal işlemler (tahsilat, gider, aidat/ek ödeme tanımı) `AuditLogs`'a yazılmalı.
- **Para:** `decimal` kullan; `float`/`double` ile para tutma. DB tarafında `DECIMAL(18,2)`.
- **Tarih:** DB'de `DATETIME2`, UTC sakla (`SYSUTCDATETIME()`), sunumda yerelleştir.
- **Dönem (Period):** Aylık tahakkuk dönemi `INT` olarak `YYYYMM` formatında (örn. `202607`).

## Kod Konvansiyonları

- Kod tanımlayıcıları (sınıf, metot, kolon) İngilizce; kullanıcıya görünen metinler Türkçe.
- API katmanları: `Controllers` → `Services` (iş kuralı) → `Repositories` (Dapper). Endpoint'ler attribute-routed `[ApiController]` ile yazılır. Controller içinde SQL YAZMA.
- DTO'lar `Shared` içinde; entity/DB modelini doğrudan istemciye dönme.
- Hata yönetimi: tutarlı `ProblemDetails` formatı. Beklenen iş hataları için anlamlı HTTP kodları (400/401/403/404/409).
- Async her yerde: `async/await`, `CancellationToken` geçir.
- Nullable reference types açık (`<Nullable>enable</Nullable>`).

## Aidat & Tahsilat (Kritik İş Kuralları)

- `DuesDefinitions` sadece TANIMDIR: **mülk tipine göre** (Daire/Dukkan/Otopark) ayrı tutar + geçerlilik dönemi.
- Her ay, aktif her mülk için `DuesCharges`'a bir tahakkuk satırı **background job** (HostedService / zamanlanmış görev) otomatik üretilir. Mülk, `Type`'ına uyan en güncel tanımı (EffectiveFrom <= dönem) kullanır; uyan tanım yoksa üretilmez ve log'a düşülür.
- Job idempotent olmalı: aynı `(PropertyId, Period)` için ikinci kez satır ÜRETMEZ (UNIQUE kısıt var).
- **Ek ödeme:** Mülk başına sabit tutar (`AmountPerProperty`), `InstallmentCount` taksite eşit bölünür; kuruş farkı son taksite eklenir.
- **Kısmi ödeme:** `Payments` eklenince ilgili charge'ın `PaidAmount`/`Status` (Pending/Partial/Paid) güncellenir.
- **Alacak/avans:** Borçtan fazla ödeme `PropertyBalances.CreditBalance`'a eklenir ve `BalanceTransactions`'a yazılır. Ödeme + charge güncelleme + bakiye hareketi TEK transaction içinde olmalı.

## Build / Çalıştırma

> Komutlar proje iskeleti kurulunca netleşecek; kurulunca bu bölümü güncelle.

- API: `dotnet run --project src/SiteYonetim.Api`
- MAUI (Android): `dotnet build src/SiteYonetim.AdminApp -f net10.0-android`
- DB migration: script'ler `db/migrations/` altında sırayla çalıştırılır.

## Yapma / Dikkat

- EF Core, LINQ-to-SQL veya başka ORM ekleme.
- Sakin verisini `SiteId` ile filtreleme — sakin tenant'lar arası çalışır, filtre telefon eşleşmesiyle olur.
- Tutamayacağın hataları yutma; logla ve anlamlı hata dön.
- v1 kapsamı dışı özellikler (online ödeme, talep/şikayet, OTP) için şemayı bozma ama implement etme; sadece uzatılabilir bırak.
