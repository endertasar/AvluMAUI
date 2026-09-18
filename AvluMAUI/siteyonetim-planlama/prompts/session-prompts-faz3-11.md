# Oturum Prompt Playbook — Faz 3 → 11 (Claude Code)

Faz 0→2 bittikten sonra kalan işleri oturumlara böldük. Her oturumda ilgili bloğu Claude Code'a yapıştır. Bloklar kendi kendine yeterli; dokümanlar (`CLAUDE.md`, `docs/`, `.claude/skills/`) repoda sabit kalır, yalnızca kapsam değişir.

**Tüm oturumlarda geçerli ortak kurallar** (her blok bunlara atıfta bulunur):
- Önce `CLAUDE.md` + `docs/roadmap.md` + ilgili `docs/*` + ilgili `.claude/skills/*` oku, harfiyen uy.
- **Faz faz ilerle, her fazın sonunda DUR**, özetle, onayımı bekle. Onaysız sonraki faza geçme.
- Sözleşmeden (`docs/api-contract.md`) sapman gerekiyorsa **uygulamadan önce sor**.
- Dapper dışında ORM yok; SQL sadece Repository'de; parametreli sorgu; soft delete; `SiteId` izolasyonu token'dan.
- Finansal işlemlerde `AuditLogs` yaz. Gizli bilgi commit'leme. Conventional commits (`git-workflow`).
- Roadmap'teki ilgili fazın "Bitti" (DoD) kriterini karşıla. Kapsam dışına ÇIKMA.

---

## OTURUM 2 — Faz 3–5 (Finansal Çekirdek)

> Yapıştır:

Bu oturumun kapsamı **Faz 3, 4, 5** (roadmap). Faz 6+'ya geçme. İlgili skill'ler: `dapper-data-access`, `api-conventions`, `auth-multitenancy`. Ortak kurallar playbook'taki gibi (faz faz dur ve onay bekle).

**Faz 3 — Mülk & Alt Kullanıcı**
- `Properties` CRUD: tip (Daire/Dukkan/Otopark), sahip + oturan adı/telefonu, `DuesResponsible`, aktiflik, soft delete.
- `/admin/users` CRUD (`OwnerOnly` politikası).
- `Shared` DTO'ları + `IApiClient` metotları.
- DoD: mülk ve alt kullanıcı işlemleri çalışıyor, hepsi token'daki SiteId'ye kapalı.

**Faz 4 — Aidat: Tanım + Tahakkuk Job**
- `DuesDefinitions` (tipe göre tutar, `EffectiveFrom` YYYYMM) CRUD; `DuesCharges` listeleme.
- `DuesAccrualHostedService`: aktif mülk × tipe uyan güncel tanım → `(PropertyId, Period)` idempotent üretim.
- Eksik tanım: atla + log + `AuditLogs('DuesAccrual','MissingDefinition')`.
- `POST /dues/charges/generate` (manuel/geçmiş dönem).
- DoD: tahakkuk üretiliyor, tekrar çalıştırınca çift satır yok, tanımsız tip uyarı üretiyor.

**Faz 5 — Tahsilat + Kredi**
- `POST /payments` (Dues/Extra hedefli) tek transaction: ödeme + charge `PaidAmount/Status` + fazlaysa `PropertyBalances`/`BalanceTransactions`.
- `useCredit` ile mahsup; `GET /payments`; `GET /properties/{id}/balance`.
- Eşzamanlılık için charge kilidi (UPDLOCK). Her tahsilat `AuditLogs`.
- DoD: tam/kısmi/fazla ödeme doğru; kredi tutarlı; çift ödeme charge'ı bozmuyor.

Kapsam dışı: Ek ödeme tanımı (Faz 6), gider, rapor, bildirim, MAUI ekranları.

---

## OTURUM 3 — Faz 6–8 (Ek Ödeme, Gider, Rapor)

> Yapıştır:

Bu oturumun kapsamı **Faz 6, 7, 8** (roadmap). İlgili skill'ler: `dapper-data-access`, `api-conventions`. Ortak kurallar playbook'taki gibi.

**Faz 6 — Ek Ödeme**
- `POST /extra-payments`: mülk başına sabit tutar (`amountPerProperty`), scope All/Selected, taksite bölme (kuruş farkı son taksite), `ExtraPaymentCharges` üretimi.
- Listeleme uçları. Tahsilat Faz 5'teki `/payments` (`targetType=Extra`) ile.
- DoD: seçili mülklere taksitli tahakkuk üretiliyor, tahsilat aynı akıştan geçiyor.

**Faz 7 — Gider**
- `Expenses` CRUD (kategori, tutar, tarih, açıklama); her kayıt `AuditLogs`.
- DoD: ekleme/listeleme/filtre (tarih, kategori) çalışıyor.

**Faz 8 — Rapor & Dashboard/Alerts**
- `GET /reports/collection-rate`, `/income-expense`, `/debtors`, `/dashboard`.
- `/admin/alerts` (veya dashboard içinde): son `MissingDefinition` vb. uyarılar `AuditLogs`'tan.
- DoD: raporlar tarih aralığıyla doğru sayılar dönüyor; eksik-tanım uyarısı yöneticiye görünüyor.

Kapsam dışı: Bildirim (Faz 9), MAUI ekranları.

---

## OTURUM 4 — Faz 9 (Bildirim / OneSignal)

> Yapıştır:

Bu oturumun kapsamı yalnızca **Faz 9** (roadmap). İlgili skill: `api-conventions`. Ortak kurallar playbook'taki gibi.

- `POST /notifications` (hedef All/Property/Resident): `Notifications` kaydı + hedef sakinlerin `OneSignalUserId`'lerine push. Push başarısızlığı kaydı geçersiz kılmaz.
- `GET /notifications` (yönetici), `PUT /resident/onesignal`, `GET /resident/notifications`, `POST /resident/notifications/{id}/read`.
- `Shared`: `INotificationService` (OneSignal wrapper); OneSignal App Id/Key yapılandırmadan, user-secrets'ta.
- DoD: bildirim kaydı oluşuyor, hedeflere push gidiyor (test cihazıyla), okundu işaretleniyor.

Kapsam dışı: MAUI ekranları (bildirim UI'ları Faz 10/11'de).

---

## OTURUM 5 — Faz 10 (AdminApp / MAUI)

> Yapıştır:

Bu oturumun kapsamı **Faz 10** (roadmap). İlgili skill: `maui-patterns` (+ `auth-multitenancy` bağlamı). Ortak kurallar playbook'taki gibi; **ekran ekran ilerle ve her ekran grubunda dur/onay bekle**.

- MVVM (`CommunityToolkit.Mvvm`), Shell navigasyon, `Shared.ApiClient`, SecureStorage, her ekranda `IsBusy`/`Error` (Türkçe mesaj).
- Ekran sırası: Login (3 alan) → Dashboard/Alerts → Mülkler (liste + ekle/düzenle) → Aidat tanımı → Tahsilat → Ek ödeme → Gider → Rapor → Bildirim gönder → Alt kullanıcı.
- Not: Faz 2'deki login ekranı zaten var; onun üstüne inşa et, yeniden yazma.
- DoD: yönetici tüm çekirdek işleri app'ten yapabiliyor; hatalar Türkçe; sadece kendi sitesinin verisi.

Kapsam dışı: ResidentApp (Faz 11), yeni API uçları (hepsi hazır olmalı; eksik varsa önce sor).

---

## OTURUM 6 — Faz 11 (ResidentApp / MAUI)

> Yapıştır:

Bu oturumun kapsamı **Faz 11** (roadmap). İlgili skill: `maui-patterns`. Ortak kurallar playbook'taki gibi; ekran ekran dur/onay bekle.

- Ekranlar: Kayıt/Login (telefon + şifre) → çoklu-mülk seçim ekranı (telefon birden fazla mülkle eşleşirse) → Borçlar (aidat + ek ödeme + kredi bakiyesi) → Tahsilat geçmişi → Bildirimler (+ okundu) → OneSignal kaydı (login sonrası `PUT /resident/onesignal`).
- Tek mülk varsa seçim ekranını atla.
- DoD: telefonla eşleşen mülkler listeleniyor; seçilen mülkün borç/tahsilat/bildirimleri doğru; yalnızca kendi mülklerine erişim.

Kapsam dışı: yönetici işlevleri; v2 özellikleri (OTP, online ödeme, talep/şikayet).

---

## Oturum Sonrası Ritmi

Her oturum bitince:
1. Değişiklikleri gözden geçir, DoD karşılandı mı doğrula (Swagger/`.http` veya app'te elle).
2. Küçük bir "durum notu" tut (hangi faz bitti, açık kalan var mı).
3. Sonraki oturuma temiz bir repo ve net kapsamla başla.

Bir faz beklenenden büyür veya sözleşme yetersiz kalırsa: önce buraya dönüp ilgili dokümanı güncelle, sonra Claude Code'a geç. Doküman = tek doğruluk kaynağı.
