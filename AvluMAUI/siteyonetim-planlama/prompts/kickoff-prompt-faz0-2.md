# Kickoff Prompt — Faz 0 → 2 (Claude Code'a yapıştır)

> Aşağıdaki bloğu Claude Code'a ilk mesaj olarak ver. Doküman ve skill'ler repoda hazır kabul edilir.

---

Bu repoda bir **site/apartman yönetim sistemi** kuruyoruz: `.NET 10` Web API (SQL Server + Dapper) ve iki `.NET 10 MAUI` uygulaması (AdminApp, ResidentApp), ortak bir `Shared` kütüphanesiyle.

**Önce şunları oku ve bunlara harfiyen uy:**
- `CLAUDE.md` (ana rehber ve kurallar)
- `docs/roadmap.md` (build sırası ve "bitti" kriterleri)
- `docs/architecture.md`, `docs/data-model.md`, `docs/api-contract.md`
- `.claude/skills/` altındaki ilgili skill'ler (özellikle `auth-multitenancy`, `dapper-data-access`, `api-conventions`, `git-workflow`)

## Bu oturumun kapsamı: Faz 0, 1, 2

Sadece bu üç fazı yap. **Faz 3 ve sonrasına GEÇME.** Roadmap'teki DoD kriterlerini karşıla.

**Faz 0 — İskelet & çekirdek altyapı**
- `SiteYonetim.sln` + 4 proje: `Api`, `Shared`, `AdminApp`, `ResidentApp`; app'ler `Shared`'ı referans alsın.
- `.gitignore`, `appsettings.example.json`, `appsettings.json` (gizliler user-secrets'ta).
- API çekirdeği: `IDbConnectionFactory`, global exception handler + `ProblemDetails`, `PagedResult<T>`, Swagger.
- `GET /api/v1/health` → 200.

**Faz 1 — DB & migration runner**
- `db/migrations/0001_init.sql` = `docs/data-model.md`'deki DDL (birebir).
- `__SchemaMigrations` + `MigrationRunner`: sıralı, idempotent, her script tek transaction; Dev'de açılışta uygula.

**Faz 2 — Auth & multi-tenancy** (en kritik faz; `auth-multitenancy` skill'ine tam uy)
- Endpoint'ler: `POST /auth/admin/register`, `admin/login`, `resident/register`, `resident/login`, `refresh`, `logout` — `docs/api-contract.md` ile birebir.
- BCrypt hash; JWT (access ~15dk + rotasyonlu refresh, `RefreshTokens`); claim'ler `sub/user_type/site_id/role`.
- Policy'ler: `AdminOnly`, `OwnerOnly`, `ResidentOnly`; `User.GetSiteId()` uzantısı.
- `SiteId` DAİMA token'dan; istemciden gelen `SiteId` yok sayılır.
- `Shared`: auth DTO'ları + `IApiClient` login metotları + SecureStorage yardımcıları + 401-refresh handler.

**Dikey dilim kontrolü (Faz 2 sonu):** AdminApp'te yalnızca login ekranı yap (3 alan: siteUsername, username, password), gerçek API'ye bağlan, başarılı girişte token'ı SecureStorage'a yaz ve basit bir "Giriş başarılı" ekranı göster. Amaç tüm boru hattını (MAUI → ApiClient → JWT → API) doğrulamak.

## Çalışma kuralları
- **Faz faz ilerle ve her fazın sonunda DUR**, ne yaptığını özetle, onayımı bekle. Onay almadan sonraki faza geçme.
- Kararsız kaldığın veya sözleşmeden sapman gereken bir nokta olursa **uygulamadan önce sor**.
- Skill kurallarını çiğneme: Dapper dışında ORM yok; SQL sadece Repository'de; parametreli sorgu; soft delete filtresi; `SiteId` izolasyonu.
- Gizli bilgi commit'leme. Connection string'i benden user-secrets için iste (bende SQL Server var).
- Conventional commits kullan; küçük, mantıklı commit'ler (`git-workflow` skill'i).
- Her faz için Swagger veya `.http` dosyasıyla hızlı bir test yolu bırak.

## Kapsam dışı (şimdi yapma)
- Faz 3+ modülleri (mülk, aidat, tahsilat, ek ödeme, gider, rapor, bildirim).
- Sakin OTP, online ödeme, talep/şikayet (v2).

Faz 0'a başlamadan önce: eksik gördüğün bilgi (ör. hedef SQL Server bağlantısı, .NET 10 SDK sürümü) varsa şimdi sor. Hazırsan Faz 0 planını kısaca özetle ve başla.
