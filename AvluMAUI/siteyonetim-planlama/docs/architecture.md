# Mimari

Bu doküman sistemin uçtan uca çalışma mantığını tanımlar: katmanlar, kimlik akışı, migration stratejisi, background job'lar ve bildirim akışı. Detay kurallar için `.claude/skills/` ve `CLAUDE.md`.

## Genel Bakış

Üç bileşen tek API üzerinden konuşur:

```
┌──────────────┐        ┌──────────────┐
│  AdminApp    │        │ ResidentApp  │   (.NET 10 MAUI)
│  (MAUI)      │        │  (MAUI)      │
└──────┬───────┘        └──────┬───────┘
       │  HTTPS + JWT          │
       └───────────┬───────────┘
                   ▼
          ┌─────────────────┐
          │   API (.NET 10) │  Controllers → Services → Repositories (Dapper)
          │  + BackgroundSvc│  aidat tahakkuku, migration runner
          └───────┬─────────┘
                  ▼
          ┌─────────────────┐        ┌──────────────┐
          │   SQL Server    │        │  OneSignal   │  (push)
          └─────────────────┘        └──────────────┘
```

`SiteYonetim.Shared` her iki MAUI app tarafından paylaşılır (DTO, ApiClient, auth, OneSignal wrapper).

## Solution / Proje İskeleti

```
SiteYonetim.sln
├─ src/
│  ├─ SiteYonetim.Api/
│  │  ├─ Controllers/        (ince; HTTP ↔ Service)
│  │  ├─ Services/           (iş kuralı, transaction, audit)
│  │  ├─ Repositories/       (Dapper; tek SQL yeri)
│  │  ├─ Models/             (iç modeller/entity)
│  │  ├─ Auth/               (JWT üretimi, policy, PasswordHasher, GetSiteId)
│  │  ├─ BackgroundJobs/     (DuesAccrualHostedService)
│  │  ├─ Infrastructure/     (DbConnectionFactory, MigrationRunner, ExceptionHandler)
│  │  ├─ appsettings.json
│  │  └─ Program.cs
│  ├─ SiteYonetim.Shared/
│  │  ├─ Dtos/               (Request/Response record'ları)
│  │  ├─ ApiClient/          (IApiClient + implementasyon, 401-refresh handler)
│  │  ├─ Auth/               (token modelleri, SecureStorage yardımcıları)
│  │  └─ Notifications/      (INotificationService — OneSignal wrapper)
│  ├─ SiteYonetim.AdminApp/  (MVVM; Views + ViewModels)
│  └─ SiteYonetim.ResidentApp/
├─ db/migrations/            (0001_init.sql, 0002_...)
├─ docs/
└─ .claude/skills/
```

## Katmanlı Yapı (API)

- **Controllers** — kimlik/yetki, model doğrulama, Service çağrısı, HTTP yanıtı. SQL/iş kuralı yok.
- **Services** — iş kuralları, çok-adımlı işlemleri tek transaction'da orkestre eder, `AuditLogs` yazar, DTO ↔ iç model dönüşümü.
- **Repositories** — yalnızca Dapper ile veri erişimi (bkz. `dapper-data-access`).

## Kimlik Akışı (JWT)

**Token'lar:** kısa ömürlü **access token** (~15 dk) + uzun ömürlü **refresh token** (rotasyonlu, DB'de `RefreshTokens`).

Giriş:
1. **Yönetici:** `siteUsername + username + password` → `Sites` ve `AdminUsers(SiteId, Username)` çözülür, `PasswordHash` (BCrypt) doğrulanır.
2. **Sakin:** `phone + password` → `Residents(Phone)` çözülür.
3. Başarılıysa access + refresh üretilir. Claim'ler: `sub`, `user_type`; yöneticide ek `site_id`, `role`.

İstek akışı:
```
İstemci → Authorization: Bearer <access>
   API → token doğrula → SiteId claim'i → Service (SiteId parametre)
```

Yenileme (401 üzerine):
```
API 401 → İstemci /auth/refresh (refreshToken)
   → eski refresh revoke, yeni access+refresh
   → orijinal istek 1 kez tekrar
   → refresh de geçersizse → login ekranı
```

Tenant izolasyonu: `SiteId` **daima** token'dan; istemciden gelen `SiteId` yok sayılır (bkz. `auth-multitenancy`). Sakinde `SiteId` yoktur; erişim telefon eşleşmesiyle sınırlanır.

## Migration Stratejisi (elle SQL script)

Şema, `db/migrations/` altında **sıralı ve değişmez** SQL dosyalarıyla yönetilir. Uygulanan script'ler bir izleme tablosunda tutulur.

```sql
CREATE TABLE dbo.__SchemaMigrations (
    ScriptName NVARCHAR(200) PRIMARY KEY,
    AppliedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

**MigrationRunner** (Infrastructure): açılışta `db/migrations/*.sql` dosyalarını isme göre sıralar, `__SchemaMigrations`'da olmayanları **sırayla ve her birini tek transaction içinde** çalıştırır, ardından kaydını yazar.

- Development: açılışta otomatik çalışır.
- Production: tercihen açık komutla (`--migrate` modu / CI adımı) — istenmeyen otomatik şema değişikliğini önlemek için.
- **Uygulanmış bir script ASLA düzenlenmez.** Düzeltme yeni numaralı dosyayla gelir.
- `0001_init.sql` = `docs/data-model.md`'deki DDL.

## Background Job: Aidat Tahakkuku

`DuesAccrualHostedService : BackgroundService` — `PeriodicTimer` ile periyodik uyanır (ör. günde bir), ayın tahakkuk gününde çalışır.

Mantık (her aktif site için):
1. Dönem = geçerli `YYYYMM`. Bu dönem için üretim yapılmış mı kontrol et (idempotent).
2. Aktif her mülk (`IsActive = 1, IsDeleted = 0`) için, mülkün `Type`'ına uyan **en güncel** `DuesDefinitions` (`EffectiveFrom <= dönem`) bulunur.
3. `DuesCharges`'a `(PropertyId, Period)` satırı eklenir. `UNIQUE(PropertyId, Period)` kısıtı çift üretimi engeller (ekstra güvence).

**Eksik tanım uyarısı:** Bir mülk tipinin o dönem için aidat tanımı yoksa o mülk atlanır ve:
- Log'a `Warning` düşülür.
- `AuditLogs`'a `Entity='DuesAccrual', Action='MissingDefinition', Detail={siteId, propertyType, period}` yazılır.
- Yöneticiye iletilir: bu uyarılar **dashboard**'da "çözülmemiş uyarılar" olarak gösterilir (`GET /reports/dashboard` veya `GET /admin/alerts` son uyarıları AuditLogs'tan okur).

> Not (opsiyonel v1.1): Yöneticiye anlık push için `AdminUsers`'a `OneSignalUserId` eklenip uyarı OneSignal ile de gönderilebilir. v1'de dashboard/alert yeterli.

**Manuel tetikleme:** Geçmiş dönem düzeltmesi için `POST /dues/charges/generate` aynı idempotent mantığı çağırır.

Ek ödeme tahakkukları job ile değil, tanımlanınca (`POST /extra-payments`) senkron üretilir.

## Bildirim Akışı (OneSignal)

Gönderme (yönetici → `POST /notifications`):
1. Service `Notifications` kaydını oluşturur (hedef: All / Property / Resident).
2. Hedef sakinlerin `OneSignalUserId`'leri toplanır.
3. OneSignal API ile push gönderilir. Push başarısızlığı kaydı geçersiz kılmaz (kayıt kalıcı; okundu takibi ayrı).

Alma (ResidentApp):
- Login sonrası OneSignal user Id alınır → `PUT /resident/onesignal` ile kaydedilir.
- Bildirim listesi API'den çekilir; açılınca `POST /resident/notifications/{id}/read`.

## Konfigürasyon & Gizli Bilgi

- Ayarlar `appsettings.json`; gizli değerler (connection string, JWT key, OneSignal App Id/Key, SMS anahtarları) **user-secrets / ortam değişkeni** ile (bkz. `git-workflow`).
- Repoda yalnızca `appsettings.example.json` (placeholder).

## Hata Yönetimi

- Global exception handler → RFC 7807 `ProblemDetails`. Gizli detay sızmaz.
- İş hataları özel exception'larla (`NotFoundException`, `ConflictException`, `DomainException`) ifade edilir, tek yerde HTTP koduna map'lenir.
- İstemci `ApiException` alır; kullanıcıya Türkçe mesaj gösterir.

## Para, Tarih, Dönem

- Para `decimal` / `DECIMAL(18,2)`, birim TRY.
- Tarih `DATETIME2`, UTC saklanır, istemcide yerelleştirilir.
- Dönem `INT` `YYYYMM`.

## Dağıtım (özet)

- API: Kestrel + reverse proxy (IIS/Nginx) arkasında, HTTPS zorunlu.
- SQL Server ayrı; connection string gizli yönetimiyle.
- Migration'lar dağıtım öncesi/adımında uygulanır.
- MAUI app'ler ilgili store süreçleriyle (Android/iOS) yayınlanır; API base URL yapılandırmadan gelir (dev/prod ayrı).
