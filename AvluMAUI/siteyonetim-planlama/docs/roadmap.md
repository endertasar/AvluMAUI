# Yol Haritası (Build Sırası)

Bu doküman modüllerin **hangi sırayla** ve **hangi bağımlılıkla** kurulacağını, her fazın "bitti" (Definition of Done) kriterini tanımlar. Prensip: önce API'yi uçtan uca çalışır ve test edilebilir hale getir, sonra MAUI app'leri bağla.

## Bağımlılık Akışı

```
Faz 0 (İskelet)
   └─ Faz 1 (DB + Migration)
        └─ Faz 2 (Auth) ── [dikey dilim: AdminApp login] ──┐
             ├─ Faz 3 (Mülk + Alt kullanıcı)                │
             │    ├─ Faz 4 (Aidat: tanım + tahakkuk job)    │
             │    │    └─ Faz 5 (Tahsilat + Kredi)          │
             │    ├─ Faz 6 (Ek Ödeme)  ──┐                  │
             │    └─ Faz 7 (Gider)       │                  │
             │                           └─ Faz 8 (Rapor/Dashboard)
             └─ Faz 9 (Bildirim/OneSignal)
                        └─ Faz 10 (AdminApp) ─ Faz 11 (ResidentApp)
```

Genel DoD (her faz için geçerli): derlenir; ilgili skill kurallarına uyar; endpoint'ler `docs/api-contract.md` ile birebir; gizli bilgi commit'lenmez; değişiklik conventional commit ile gelir.

---

## Faz 0 — Solution İskeleti & Çekirdek Altyapı

**Amaç:** Çalışan boş bir sistem ve ortak altyapı.
**Bağımlılık:** —
**İşler:**
- `SiteYonetim.sln` + 4 proje: `Api`, `Shared`, `AdminApp`, `ResidentApp` (bkz. `architecture.md` iskeleti).
- App'ler `Shared`'ı referans alır.
- `.gitignore`, `appsettings.example.json`, `appsettings.json` (gizliler user-secrets).
- API çekirdeği: `IDbConnectionFactory`, global exception handler + `ProblemDetails`, `PagedResult<T>`, OpenAPI/Swagger.
- Sağlık ucu: `GET /api/v1/health` → 200.

**Bitti:** `dotnet build` tüm projelerde yeşil; API ayağa kalkıyor; `/health` 200; Swagger açılıyor.

---

## Faz 1 — Veritabanı & Migration Runner

**Amaç:** Şema uygulanabilir ve versiyonlu.
**Bağımlılık:** Faz 0
**İşler:**
- `db/migrations/0001_init.sql` = `data-model.md` DDL'i.
- `__SchemaMigrations` + `MigrationRunner` (sıralı, idempotent, her script tek transaction).
- Dev'de açılışta otomatik uygula.

**Bitti:** Boş DB'ye runner tüm tabloları kuruyor; ikinci çalıştırma hiçbir script'i tekrar uygulamıyor.

---

## Faz 2 — Auth & Multi-Tenancy

**Amaç:** Kimlik ve tenant izolasyonu (en kritik faz).
**Bağımlılık:** Faz 1 · **Skill:** `auth-multitenancy`, `api-conventions`, `dapper-data-access`
**İşler:**
- `POST /auth/admin/register` (site + ilk Owner), `admin/login`, `resident/register`, `resident/login`, `refresh`, `logout`.
- BCrypt hash; JWT (access ~15dk + refresh rotasyonlu, `RefreshTokens`).
- Claim'ler + policy'ler: `AdminOnly`, `OwnerOnly`, `ResidentOnly`; `User.GetSiteId()`.
- `Shared`: auth DTO'ları + `IApiClient` login metotları + SecureStorage yardımcıları + 401-refresh handler iskeleti.

**Bitti:** Register→login→korumalı uç→401'de refresh→yeni token akışı Swagger/HTTP dosyasıyla doğrulanıyor; başka tenant'ın verisine erişilemiyor.

> **Dikey dilim kontrolü (opsiyonel ama önerilir):** Burada AdminApp'te sadece login ekranı yapıp gerçek API'ye bağlan; tüm boru hattı (MAUI→ApiClient→JWT→API) çalışıyor mu doğrula. Sonra API'ye devam.

---

## Faz 3 — Mülk & Alt Kullanıcı

**Amaç:** Yönetici temel yönetim.
**Bağımlılık:** Faz 2 · **Skill:** `api-conventions`, `dapper-data-access`
**İşler:**
- `Properties` CRUD (tip: Daire/Dukkan/Otopark; sahip + oturan telefonları; `DuesResponsible`).
- `/admin/users` CRUD (`OwnerOnly`).
- İlgili `Shared` DTO'ları + ApiClient metotları.
- Finansal olmayan ama audit'e değer değişikliklerde `AuditLogs` (opsiyonel).

**Bitti:** Mülk ve alt kullanıcı oluşturma/listeleme/güncelleme/soft-delete çalışıyor; hepsi token'daki SiteId'ye kapalı.

---

## Faz 4 — Aidat: Tanım + Tahakkuk Job

**Amaç:** Aylık borç otomatik oluşsun.
**Bağımlılık:** Faz 3 · **Skill:** `dapper-data-access`, `api-conventions`
**İşler:**
- `DuesDefinitions` (tipe göre tutar) CRUD; `DuesCharges` listeleme.
- `DuesAccrualHostedService`: aktif mülk × tipe uyan güncel tanım → `(PropertyId, Period)` idempotent üretim.
- **Eksik tanım uyarısı:** atla + log + `AuditLogs('DuesAccrual','MissingDefinition')`.
- `POST /dues/charges/generate` (manuel/geçmiş dönem).

**Bitti:** Belirli dönem için tahakkuk üretiliyor; tekrar çalıştırınca çift satır yok; tanımsız tip uyarı üretiyor.

---

## Faz 5 — Tahsilat + Kredi (Alacak)

**Amaç:** Ödeme al, kısmi/fazla ödemeyi doğru işle.
**Bağımlılık:** Faz 4 · **Skill:** `dapper-data-access` (transaction), `api-conventions`
**İşler:**
- `POST /payments` (Dues/Extra hedefli), tek transaction: ödeme + charge `PaidAmount/Status` + fazlaysa `PropertyBalances`/`BalanceTransactions`.
- `useCredit` ile mevcut alacaktan mahsup.
- `GET /payments`, `GET /properties/{id}/balance`.
- Her tahsilat `AuditLogs`.

**Bitti:** Tam/kısmi/fazla ödeme senaryoları doğru; kredi bakiyesi tutarlı; eşzamanlı çift ödeme charge'ı bozmuyor (kilit).

---

## Faz 6 — Ek Ödeme

**Amaç:** Aidat dışı tahsilatlar (taksitli).
**Bağımlılık:** Faz 5 (tahsilat altyapısını paylaşır) · **Skill:** `dapper-data-access`, `api-conventions`
**İşler:**
- `POST /extra-payments` (mülk başına sabit tutar; scope All/Selected; taksit bölme, kuruş farkı son taksite).
- `ExtraPaymentCharges` üretimi; listeleme.
- Tahsilat Faz 5'teki `/payments` (`targetType=Extra`) ile.

**Bitti:** Ek ödeme tanımı seçili mülklere taksitli tahakkuk üretiyor; tahsilatı aidatla aynı akıştan geçiyor.

---

## Faz 7 — Gider

**Amaç:** Gider kaydı.
**Bağımlılık:** Faz 3 · **Skill:** `api-conventions`, `dapper-data-access`
**İşler:** `Expenses` CRUD (kategori, tutar, tarih); her kayıt `AuditLogs`.
**Bitti:** Gider ekleme/listeleme/filtre (tarih/kategori) çalışıyor.

---

## Faz 8 — Rapor & Dashboard/Alerts

**Amaç:** Yöneticiye özet ve uyarılar.
**Bağımlılık:** Faz 5, 7 · **Skill:** `dapper-data-access`, `api-conventions`
**İşler:**
- `GET /reports/collection-rate`, `/income-expense`, `/debtors`, `/dashboard`.
- `/admin/alerts` (veya dashboard içinde): son `MissingDefinition` vb. uyarılar AuditLogs'tan.

**Bitti:** Raporlar tarih aralığıyla doğru sayılar dönüyor; eksik-tanım uyarısı yöneticiye görünüyor.

---

## Faz 9 — Bildirim (OneSignal)

**Amaç:** Yönetici → sakin push + kayıt.
**Bağımlılık:** Faz 3 · **Skill:** `api-conventions`
**İşler:**
- `POST /notifications` (All/Property/Resident): kayıt + hedef sakinlerin `OneSignalUserId` push.
- `GET /notifications`, `PUT /resident/onesignal`, `GET /resident/notifications`, `POST /resident/notifications/{id}/read`.
- `Shared`: `INotificationService` (OneSignal wrapper).

**Bitti:** Bildirim kaydı oluşuyor, hedeflere push gidiyor (test cihazıyla), okundu işaretleniyor.

---

## Faz 10 — AdminApp (MAUI)

**Amaç:** Yönetici uygulaması.
**Bağımlılık:** İlgili API fazları + Faz 2 · **Skill:** `maui-patterns`
**İşler (ekran sırası):** Login (3 alan) → Dashboard/Alerts → Mülkler → Aidat tanımı → Tahsilat → Ek ödeme → Gider → Rapor → Bildirim gönder → Alt kullanıcı.
- MVVM, Shell, `Shared.ApiClient`, SecureStorage, `IsBusy/Error`.

**Bitti:** Yönetici tüm çekirdek işleri app üzerinden yapabiliyor; hatalar Türkçe gösteriliyor.

---

## Faz 11 — ResidentApp (MAUI)

**Amaç:** Sakin uygulaması.
**Bağımlılık:** Faz 4/5/9 + Faz 2 · **Skill:** `maui-patterns`
**İşler:** Kayıt/Login (telefon+şifre) → çoklu-mülk seçim ekranı → Borçlar (aidat+ek, kredi) → Tahsilat geçmişi → Bildirimler (+okundu) → OneSignal kaydı.
**Bitti:** Telefonla eşleşen mülkler listeleniyor; seçilen mülkün borç/tahsilat/bildirimleri doğru; sadece kendi mülklerine erişim.

---

## Notlar

- **Shared DTO'ları** her API fazıyla birlikte büyür; app fazları hazır DTO/ApiClient'ı kullanır.
- Her faz sonunda küçük bir manuel test seti (Swagger veya `.http` dosyası) tut.
- v2'ye ertelenenler: sakin OTP, yöneticiye push uyarısı (`AdminUsers.OneSignalUserId`), online ödeme, talep/şikayet.
