# API Sözleşmesi

`.NET 10` Web API. Bu doküman endpoint'leri ve DTO'ları tanımlar. Detay iş kuralları için `CLAUDE.md`.

## Genel Kurallar

- **Base yol:** `/api/v1`
- **Auth header:** `Authorization: Bearer <accessToken>`
- **Tenant izolasyonu:** Yönetici endpoint'lerinde `SiteId` **daima JWT'den** alınır, istemciden ASLA. İstemci `SiteId` gönderse bile yok sayılır.
- **Hata formatı:** RFC 7807 `ProblemDetails`. Doğrulama hataları `400`, yetkisiz `401`, yasak `403`, bulunamadı `404`, çakışma `409`.
- **Sayfalama:** Liste endpoint'leri `?page=1&pageSize=20`. Yanıt zarfı: `{ items, page, pageSize, totalCount }`.
- **Tarih/tutar:** Tutar `decimal`, tarih ISO 8601 UTC. Dönem `YYYYMM` integer.
- **Yetki rolleri:** `admin` (Owner/SubUser) ve `resident`. Her endpoint hangi tipe açık, aşağıda belirtilmiştir.

---

## 1. Auth

### POST /auth/admin/register
Yeni site + ilk yönetici (Owner) oluşturur. Auth gerekmez.
```
Request:  { siteUsername, siteName, address?, username, password, fullName? }
Response: { site: { id, siteUsername, name }, accessToken, refreshToken }
Hata:     409 siteUsername zaten kayıtlı
```

### POST /auth/admin/login
```
Request:  { siteUsername, username, password }
Response: { accessToken, refreshToken, user: { id, username, fullName, role, siteId } }
Hata:     401 hatalı kimlik
```

### POST /auth/resident/register
```
Request:  { phone, password, fullName? }
Response: { accessToken, refreshToken, resident: { id, phone, fullName } }
Hata:     409 telefon zaten kayıtlı
```

### POST /auth/resident/login
```
Request:  { phone, password }
Response: { accessToken, refreshToken, resident: { id, phone, fullName } }
```

### POST /auth/refresh
```
Request:  { refreshToken }
Response: { accessToken, refreshToken }
```

### POST /auth/logout   (admin | resident)
```
Request:  { refreshToken }
Response: 204
```

---

## 2. Yönetici — Alt Kullanıcılar   (admin: Owner)

| Method | Yol | Açıklama |
|---|---|---|
| POST | /admin/users | Alt kullanıcı oluştur |
| GET | /admin/users | Kullanıcıları listele |
| PUT | /admin/users/{id} | Güncelle (ad, rol, aktiflik) |
| DELETE | /admin/users/{id} | Soft delete |

```
POST /admin/users
Request:  { username, password, fullName?, role }   // role: Owner | SubUser
Response: { id, username, fullName, role, isActive }
```

---

## 3. Mülkler   (admin)

| Method | Yol | Açıklama |
|---|---|---|
| POST | /properties | Mülk oluştur |
| GET | /properties | Listele (?block=&type=&search=) |
| GET | /properties/{id} | Detay |
| PUT | /properties/{id} | Güncelle |
| DELETE | /properties/{id} | Soft delete |

```
POST /properties
Request:  { block?, unitNo, type, ownerName?, ownerPhone?,
            residentName?, residentPhone?, duesResponsible }
Response: { id, ...aynı alanlar, isActive }
```

---

## 4. Aidat   (admin)

| Method | Yol | Açıklama |
|---|---|---|
| POST | /dues/definitions | Aylık aidat tanımı |
| GET | /dues/definitions | Tanımları listele |
| GET | /dues/charges | Tahakkukları listele (?period=&propertyId=&status=) |
| POST | /dues/charges/generate | Belirli dönem için manuel tahakkuk üret (idempotent) |

```
POST /dues/definitions
Request:  { propertyType, amount, effectiveFrom, description? }
          // propertyType: Daire | Dukkan | Otopark ; effectiveFrom: YYYYMM
Response: { id, propertyType, amount, effectiveFrom, description }
// Her tip için ayrı tanım girilir. Tahakkuk, mülkün tipine uyan
// en güncel (EffectiveFrom <= dönem) tanımı kullanır.

// Not: Otomatik aylık tahakkuk background job ile üretilir.
// generate endpoint'i geçmiş dönem düzeltmesi / manuel tetikleme içindir.
```

---

## 5. Ek Ödemeler   (admin)

| Method | Yol | Açıklama |
|---|---|---|
| POST | /extra-payments | Ek ödeme tanımla (mülklere taksitli tahakkuk üretir) |
| GET | /extra-payments | Listele |
| GET | /extra-payments/{id} | Detay + taksit özeti |
| GET | /extra-payments/{id}/charges | Mülk bazlı tahakkuklar (?propertyId=&status=) |

```
POST /extra-payments
Request:  { name, amountPerProperty, installmentCount, propertyScope, propertyIds? }
          // amountPerProperty: her mülkün ödeyeceği sabit tutar
          // propertyScope: All | Selected ; Selected ise propertyIds zorunlu
Response: { id, name, amountPerProperty, totalAmount, installmentCount, generatedChargeCount }
          // totalAmount = amountPerProperty * kapsanan mülk sayısı (türetilmiş)
```

---

## 6. Tahsilat   (admin)

Hem aidat hem ek ödeme tahsilatı bu endpoint üzerinden. Kısmi ödeme desteklenir; ilgili charge'ın `PaidAmount`/`Status`'ü tek transaction'da güncellenir.

```
POST /payments
Request:  { propertyId, targetType, targetId, amount, method, note?, useCredit? }
          // targetType: Dues | Extra
          // targetId: DuesCharges.Id | ExtraPaymentCharges.Id
          // useCredit: true ise mevcut alacak bakiyesi önce mahsup edilir
Response: { id, propertyId, targetType, targetId, amount, method, paidAt,
            chargeStatus, chargeRemaining,
            appliedToCredit,      // borçtan fazlası kredi olarak eklendiyse
            creditBalance }       // mülkün güncel alacak bakiyesi

GET /payments   (admin)
  ?propertyId=&from=&to=&targetType=   → sayfalı liste

GET /properties/{id}/balance   (admin)
Response: { propertyId, creditBalance,
            transactions: [ { direction, amount, source, note, createdAt } ] }
```

---

## 7. Giderler   (admin)

| Method | Yol | Açıklama |
|---|---|---|
| POST | /expenses | Gider ekle |
| GET | /expenses | Listele (?from=&to=&category=) |
| PUT | /expenses/{id} | Güncelle |
| DELETE | /expenses/{id} | Soft delete |

```
POST /expenses
Request:  { category?, amount, expenseDate, description? }
Response: { id, category, amount, expenseDate, description }
```

---

## 8. Raporlar   (admin)

| Method | Yol | Açıklama |
|---|---|---|
| GET | /reports/collection-rate | Dönem bazlı tahsilat oranı (?from=&to=) |
| GET | /reports/income-expense | Gelir/gider özeti (?from=&to=) |
| GET | /reports/debtors | Borçlu mülk listesi (?minAmount=) |
| GET | /reports/dashboard | Özet kart verileri (bu ay tahsilat, borç, gider) |

```
GET /reports/income-expense?from=202601&to=202612
Response: { totalIncome, totalExpense, net,
            byPeriod: [ { period, income, expense } ] }
```

---

## 9. Bildirimler   (admin gönderir)

Gönderim hem `Notifications` kaydını oluşturur hem OneSignal push tetikler.

```
POST /notifications
Request:  { title, body, targetType, targetId? }
          // targetType: All | Property | Resident
          // Property/Resident ise targetId zorunlu
Response: { id, title, body, targetType, sentAt, recipientCount }

GET /notifications   (admin)   → gönderilenler listesi
```

---

## 10. Sakin Uygulaması   (resident)

| Method | Yol | Açıklama |
|---|---|---|
| GET | /resident/properties | Telefon eşleşen mülkler (çoklu ise seçim için) |
| GET | /resident/properties/{id}/payments | Mülkün tahsilat geçmişi |
| GET | /resident/properties/{id}/debts | Bekleyen borçlar (aidat + ek ödeme) |
| GET | /resident/notifications | Sakine gelen bildirimler |
| POST | /resident/notifications/{id}/read | Okundu işaretle |
| PUT | /resident/onesignal | OneSignalUserId kaydet |

```
GET /resident/properties
Response: { items: [ { propertyId, siteName, block, unitNo, type, matchedAs } ] }
          // matchedAs: Owner | Resident

GET /resident/properties/{id}/debts
Response: { totalDebt, creditBalance,
            dues:  [ { chargeId, period, amount, paidAmount, remaining, status } ],
            extra: [ { chargeId, name, installmentNo, amount, paidAmount, remaining } ] }

PUT /resident/onesignal
Request:  { oneSignalUserId }
Response: 204
```

> Güvenlik: Sakin, yalnızca kendi telefonuyla eşleşen `propertyId`'lere erişebilir. Her sakin endpoint'i, istenen mülkün sakinin telefonuyla eşleştiğini doğrular; eşleşmiyorsa `403`.

---

## Alınan Kararlar

1. **Tipe göre aidat:** Aidat mülk tipine göre ayrı tanımlanır (`DuesDefinitions.PropertyType`).
2. **Ek ödeme:** Mülk başına sabit tutar (`amountPerProperty`), taksite bölünür.
3. **Alacak takibi:** Fazla ödeme `PropertyBalances.CreditBalance`'a yazılır, `useCredit` ile mahsup edilir.
4. **Site kaydı:** Herkese açık (`/auth/admin/register` auth'suz). İleride rate-limit/abuse önlemi eklenebilir (v2).
