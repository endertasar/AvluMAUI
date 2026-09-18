# AvluApi — API Referans Dökümanı

> **Base URL:** `https://<sunucu>/api`  
> **Tüm başarılı yanıtlar** `ApiResponse<T>` wrapper ile döner:  
> `{ "success": true, "data": {...}, "message": "...", "errors": null }`  
> **Hata yanıtları:** `{ "success": false, "data": null, "message": "...", "errors": ["..."] }`

---

## Token Tipleri

| Tip | Elde Edildiği Endpoint | Süre | Claims |
|---|---|---|---|
| **Pre-Auth Token** | `POST /api/auth/login` | 1 saat | `adminId`, `username`, `tenant` (çoğul) |
| **Tenant Token** | `POST /api/auth/tenant-select` | 8 saat | `adminId`, `tenantId`, `role` |
| **Resident Token** | `POST /api/auth/otp/verify` | 24 saat | `phoneNumber`, `tenantId`, `propertyId` (çoğul) |

---

## İçindekiler

1. [Auth — `/api/auth`](#auth)
2. [Tenants — `/api/tenants`](#tenants)
3. [Properties — `/api/properties`](#properties)
4. [Residents — `/api/residents`](#residents)
5. [Dues — `/api/dues`](#dues)
6. [Charges — `/api/charges`](#charges)
7. [Payments — `/api/payments`](#payments)
8. [Notifications — `/api/notifications`](#notifications)
9. [Reports — `/api/reports`](#reports)
10. [PhoneMap — `/api/phonemap`](#phonemap)
11. [Eksik / Önerilen Endpointler](#eksik--önerilen-endpointler)
12. [MAUI Temel Altyapı](#maui-temel-altyapı)
13. [Hata Kodları](#hata-kodları)

---

## Auth

### POST /api/auth/login

**Açıklama:** Admin kullanıcı girişi. Başarılı girişte Pre-Auth Token döner; token içinde adminId, username ve adminin bağlı olduğu tenant ID'leri claim olarak bulunur. Ardından `/api/auth/tenant-select` ile aktif tenant seçilir.  
**Yetki:** Public  
**Tenant Scope:** Hayır

**Request Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "Giriş başarılı.",
  "errors": null
}
```
> `data`: Pre-Auth JWT string. Decode edildiğinde `adminId`, `username`, `tenant` (birden fazla olabilir) claim'leri içerir.

**Response (Hata):**
```json
// 401 — Kullanıcı adı veya şifre hatalı
{ "success": false, "data": null, "message": "Kullanıcı adı veya şifre hatalı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
// Models
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Service
public async Task<string?> LoginAsync(string username, string password)
{
    var body = new LoginRequest { Username = username, Password = password };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/auth/login", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<string>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result?.Success == true)
        {
            // Pre-Auth token'ı kısa süreli sakla
            await SecureStorage.SetAsync("pre_auth_token", result.Data!);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.Data!);
            return result.Data;
        }
        return null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/auth/tenant-select

**Açıklama:** Login sonrası tenant seçimi. Pre-Auth Token ile çağrılır; seçilen tenant için `adminId`, `tenantId` ve `role` içeren Tenant Token döner. Bu token sonraki tüm işlemlerde kullanılır.  
**Yetki:** Pre-Auth Token (Bearer)  
**Tenant Scope:** Hayır

**Request Headers:**
```
Authorization: Bearer {preAuthToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "Tenant seçildi.",
  "errors": null
}
```
> `data`: Tenant JWT. Claims: `adminId`, `tenantId`, `role` (`SuperAdmin` | `Manager`)

**Response (Hata):**
```json
// 401 — Adminin bu tenanta erişimi yok
{ "success": false, "data": null, "message": "Bu tenant'a erişim yetkiniz yok.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class TenantSelectRequest
{
    public Guid TenantId { get; set; }
}

public async Task<bool> SelectTenantAsync(Guid tenantId)
{
    var body = new TenantSelectRequest { TenantId = tenantId };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/auth/tenant-select", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<string>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result?.Success == true)
        {
            await SecureStorage.SetAsync("tenant_token", result.Data!);
            SecureStorage.Remove("pre_auth_token");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.Data!);
            return true;
        }
        return false;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/auth/otp/send

**Açıklama:** Mülk sahibi uygulaması için OTP gönderir. Yönetici Tenant Token ile çağrılır; `tenantId` bu token'dan okunur. Şu an OTP SMS ile gönderilmez, uygulama loglarına yazılır.  
**Yetki:** Tenant Token (Bearer)  
**Tenant Scope:** Evet (tenantId JWT'den alınır)

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "phoneNumber": "+905551234567"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": "OTP gönderildi.",
  "message": "OTP loglanmıştır.",
  "errors": null
}
```

**Response (Hata):**
```json
// 401 — Geçersiz veya eksik token
{ "success": false, "data": null, "message": "Unauthorized", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class OtpSendRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public async Task<bool> SendOtpAsync(string phoneNumber)
{
    var body = new OtpSendRequest { PhoneNumber = phoneNumber };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/auth/otp/send", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<string>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/auth/otp/verify

**Açıklama:** Mülk sahibi OTP doğrulama. Başarılı olursa `phoneNumber`, `tenantId` ve erişilebilir `propertyId` listesi içeren Resident Token döner.  
**Yetki:** Public  
**Tenant Scope:** Hayır (tenantId body'den alınır)

**Request Headers:**
```
Content-Type: application/json
```

**Request Body:**
```json
{
  "phoneNumber": "+905551234567",
  "otp": "482931",
  "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "OTP doğrulandı.",
  "errors": null
}
```
> `data`: Resident JWT. Claims: `phoneNumber`, `tenantId`, `propertyId` (birden fazla olabilir)

**Response (Hata):**
```json
// 401 — OTP geçersiz veya süresi dolmuş
{ "success": false, "data": null, "message": "OTP geçersiz veya süresi dolmuş.", "errors": null }

// 401 — OTP hatalı
{ "success": false, "data": null, "message": "OTP hatalı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class OtpVerifyRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}

public async Task<bool> VerifyOtpAsync(string phoneNumber, string otp, Guid tenantId)
{
    var body = new OtpVerifyRequest
    {
        PhoneNumber = phoneNumber,
        Otp = otp,
        TenantId = tenantId
    };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/auth/otp/verify", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<string>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result?.Success == true)
        {
            await SecureStorage.SetAsync("resident_token", result.Data!);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.Data!);
            return true;
        }
        return false;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## Tenants

### GET /api/tenants

**Açıklama:** Sistemdeki tüm tenant (apartman/site) listesini döner. Genellikle SuperAdmin rolü kullanır.  
**Yetki:** Tenant Token (Bearer)  
**Tenant Scope:** Hayır

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "tenantName": "Gül Apartmanı",
      "tenantCode": "GUL-APT-01",
      "address": "Atatürk Mah. 12. Sok No:5 İstanbul",
      "isActive": true,
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ],
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 401 — Token eksik veya geçersiz
{ "success": false, "data": null, "message": "Unauthorized", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class TenantDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string TenantCode { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public async Task<List<TenantDto>> GetTenantsAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("/api/tenants");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<TenantDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/tenants

**Açıklama:** Yeni tenant (apartman) oluşturur. Yalnızca SuperAdmin kullanmalıdır.  
**Yetki:** Tenant Token (Bearer) — SuperAdmin  
**Tenant Scope:** Hayır

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "tenantName": "Lale Sitesi",
  "tenantCode": "LALE-SITE-01",
  "address": "Bahçelievler Mah. 5. Cad. No:12 Ankara"
}
```
> `address` opsiyoneldir.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "tenantId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "tenantName": "Lale Sitesi",
    "tenantCode": "LALE-SITE-01",
    "address": "Bahçelievler Mah. 5. Cad. No:12 Ankara",
    "isActive": true,
    "createdAt": "2024-06-01T08:00:00Z"
  },
  "message": "Tenant oluşturuldu.",
  "errors": null
}
```

**Response (Hata):**
```json
// 400 — TenantCode zaten kullanımda
{ "success": false, "data": null, "message": "Bu tenant kodu zaten mevcut.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class CreateTenantRequest
{
    public string TenantName { get; set; } = string.Empty;
    public string TenantCode { get; set; } = string.Empty;
    public string? Address { get; set; }
}

public async Task<TenantDto?> CreateTenantAsync(string name, string code, string? address)
{
    var body = new CreateTenantRequest { TenantName = name, TenantCode = code, Address = address };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/tenants", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<TenantDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### PUT /api/tenants/{id}

**Açıklama:** Mevcut tenant bilgilerini günceller. `tenantCode` değiştirilemez.  
**Yetki:** Tenant Token (Bearer) — SuperAdmin  
**Tenant Scope:** Hayır

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "tenantName": "Lale Sitesi (Güncellendi)",
  "address": "Yeni Adres Cad. No:1 Ankara",
  "isActive": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "tenantId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "tenantName": "Lale Sitesi (Güncellendi)",
    "tenantCode": "LALE-SITE-01",
    "address": "Yeni Adres Cad. No:1 Ankara",
    "isActive": true,
    "createdAt": "2024-06-01T08:00:00Z"
  },
  "message": "Tenant güncellendi.",
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Tenant bulunamadı
{ "success": false, "data": null, "message": "Tenant bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class UpdateTenantRequest
{
    public string TenantName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

public async Task<bool> UpdateTenantAsync(Guid tenantId, string name, string? address, bool isActive)
{
    var body = new UpdateTenantRequest { TenantName = name, Address = address, IsActive = isActive };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PutAsync($"/api/tenants/{tenantId}", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<TenantDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### GET /api/tenants/{id}/admins

**Açıklama:** Belirtilen tenanta atanmış admin kullanıcılarını ve rollerini listeler.  
**Yetki:** Tenant Token (Bearer) — SuperAdmin / Manager  
**Tenant Scope:** Hayır

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "adminId": 1,
      "username": "yonetici",
      "email": "yonetici@gulapt.com",
      "role": "Manager",
      "assignedAt": "2024-01-15T10:30:00Z"
    }
  ],
  "message": null,
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class AdminTenantDto
{
    public long AdminId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

public async Task<List<AdminTenantDto>> GetTenantAdminsAsync(Guid tenantId)
{
    try
    {
        var response = await _httpClient.GetAsync($"/api/tenants/{tenantId}/admins");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<AdminTenantDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/tenants/{id}/admins

**Açıklama:** Mevcut bir admin kullanıcısını belirtilen tenanta atar. `role` değerleri: `SuperAdmin` | `Manager`.  
**Yetki:** Tenant Token (Bearer) — SuperAdmin  
**Tenant Scope:** Hayır

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "adminId": 2,
  "role": "Manager"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": "Admin atandı.",
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Admin veya tenant bulunamadı
{ "success": false, "data": null, "message": "Admin bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class AssignAdminRequest
{
    public long AdminId { get; set; }
    public string Role { get; set; } = "Manager";
}

public async Task<bool> AssignAdminToTenantAsync(Guid tenantId, long adminId, string role)
{
    var body = new AssignAdminRequest { AdminId = adminId, Role = role };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync($"/api/tenants/{tenantId}/admins", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<string>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## Properties

### GET /api/properties

**Açıklama:** Aktif tenant'a ait tüm mülkleri (daire/dükkan) listeler. TenantId JWT'den otomatik alınır.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "propertyId": 1,
      "blockName": "A Blok",
      "doorNumber": "5",
      "floor": 2,
      "propertyType": "Apartment",
      "isActive": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "activeResident": null
    }
  ],
  "message": null,
  "errors": null
}
```
> `propertyType` değerleri: `Apartment` | `Commercial` | `Other`  
> `activeResident`: `null` veya `ResidentDto` nesnesi

**Response (Hata):**
```json
// 403 — tenantId claim yok (yanlış token tipi)
{ "success": false, "data": null, "message": "Forbidden", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class ResidentDto
{
    public long ResidentId { get; set; }
    public long PropertyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ResidentType { get; set; } = string.Empty;
    public DateOnly? MoveInDate { get; set; }
    public DateOnly? MoveOutDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PropertyDto
{
    public long PropertyId { get; set; }
    public string? BlockName { get; set; }
    public string DoorNumber { get; set; } = string.Empty;
    public int? Floor { get; set; }
    public string PropertyType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public ResidentDto? ActiveResident { get; set; }
}

public async Task<List<PropertyDto>> GetPropertiesAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("/api/properties");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<PropertyDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### GET /api/properties/{id}

**Açıklama:** Tek mülk detayı. Varsa aktif sakin (`activeResident`) bilgisiyle birlikte döner.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "propertyId": 1,
    "blockName": "A Blok",
    "doorNumber": "5",
    "floor": 2,
    "propertyType": "Apartment",
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "activeResident": {
      "residentId": 3,
      "propertyId": 1,
      "fullName": "Ahmet Yılmaz",
      "phoneNumber": "+905551234567",
      "residentType": "Owner",
      "moveInDate": "2022-03-01",
      "moveOutDate": null,
      "isActive": true,
      "createdAt": "2024-01-16T09:00:00Z"
    }
  },
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Mülk bulunamadı
{ "success": false, "data": null, "message": "Mülk bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<PropertyDto?> GetPropertyByIdAsync(long propertyId)
{
    try
    {
        var response = await _httpClient.GetAsync($"/api/properties/{propertyId}");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PropertyDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/properties

**Açıklama:** Yeni mülk tanımlar. TenantId JWT'den otomatik atanır.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "blockName": "B Blok",
  "doorNumber": "12",
  "floor": 3,
  "propertyType": "Apartment"
}
```
> `blockName` ve `floor` opsiyoneldir. `propertyType` varsayılanı `Apartment`.

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "propertyId": 7,
    "blockName": "B Blok",
    "doorNumber": "12",
    "floor": 3,
    "propertyType": "Apartment",
    "isActive": true,
    "createdAt": "2024-06-01T08:00:00Z",
    "activeResident": null
  },
  "message": "Mülk oluşturuldu.",
  "errors": null
}
```

**Response (Hata):**
```json
// 400 — DoorNumber zorunludur
{ "success": false, "data": null, "message": "DoorNumber alanı zorunludur.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class CreatePropertyRequest
{
    public string? BlockName { get; set; }
    public string DoorNumber { get; set; } = string.Empty;
    public int? Floor { get; set; }
    public string PropertyType { get; set; } = "Apartment";
}

public async Task<PropertyDto?> CreatePropertyAsync(
    string doorNumber, string propertyType, string? blockName = null, int? floor = null)
{
    var body = new CreatePropertyRequest
    {
        BlockName = blockName,
        DoorNumber = doorNumber,
        Floor = floor,
        PropertyType = propertyType
    };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/properties", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PropertyDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### PUT /api/properties/{id}

**Açıklama:** Mülk bilgilerini günceller. TenantId kontrolü servis katmanında yapılır.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "blockName": "B Blok",
  "doorNumber": "12",
  "floor": 4,
  "propertyType": "Commercial"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "propertyId": 7,
    "blockName": "B Blok",
    "doorNumber": "12",
    "floor": 4,
    "propertyType": "Commercial",
    "isActive": true,
    "createdAt": "2024-06-01T08:00:00Z",
    "activeResident": null
  },
  "message": "Mülk güncellendi.",
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Mülk bulunamadı
{ "success": false, "data": null, "message": "Mülk bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class UpdatePropertyRequest
{
    public string? BlockName { get; set; }
    public string DoorNumber { get; set; } = string.Empty;
    public int? Floor { get; set; }
    public string PropertyType { get; set; } = "Apartment";
}

public async Task<bool> UpdatePropertyAsync(long propertyId, UpdatePropertyRequest request)
{
    var content = new StringContent(
        JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PutAsync($"/api/properties/{propertyId}", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PropertyDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### DELETE /api/properties/{id}

**Açıklama:** Mülkü soft-delete yapar (`IsActive = false`). Veri silinmez, tarihçe korunur.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": "Mülk silindi.",
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Mülk bulunamadı
{ "success": false, "data": null, "message": "Mülk bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<bool> DeletePropertyAsync(long propertyId)
{
    try
    {
        var response = await _httpClient.DeleteAsync($"/api/properties/{propertyId}");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<string>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/properties/{id}/assign-resident

**Açıklama:** Mülke yeni sakin atar. Varsa mevcut aktif sakini pasife çeker, `PhonePropertyMap`'i günceller ve yeni sakin kaydı oluşturur.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "fullName": "Fatma Kaya",
  "phoneNumber": "+905559876543",
  "residentType": "Tenant",
  "moveInDate": "2024-06-01"
}
```
> `residentType` değerleri: `Owner` | `Tenant`. `moveInDate` opsiyoneldir.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "residentId": 7,
    "propertyId": 1,
    "fullName": "Fatma Kaya",
    "phoneNumber": "+905559876543",
    "residentType": "Tenant",
    "moveInDate": "2024-06-01",
    "moveOutDate": null,
    "isActive": true,
    "createdAt": "2024-06-01T08:00:00Z"
  },
  "message": "Sakin atandı.",
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Mülk bulunamadı
{ "success": false, "data": null, "message": "Mülk bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class AssignResidentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ResidentType { get; set; } = "Owner";
    public DateOnly? MoveInDate { get; set; }
}

public async Task<ResidentDto?> AssignResidentAsync(
    long propertyId, string fullName, string phone, string residentType, DateOnly? moveInDate)
{
    var body = new AssignResidentRequest
    {
        FullName = fullName,
        PhoneNumber = phone,
        ResidentType = residentType,
        MoveInDate = moveInDate
    };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync(
            $"/api/properties/{propertyId}/assign-resident", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ResidentDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## Residents

### GET /api/residents

**Açıklama:** Aktif tenant'a ait sakin listesi. Opsiyonel filtreler ile daraltılabilir.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Query Parameters:**

| Parametre | Tip | Zorunlu | Açıklama |
|---|---|---|---|
| `propertyId` | `long` | Hayır | Belirli mülke ait sakinler |
| `residentType` | `string` | Hayır | `Owner` veya `Tenant` |
| `isActive` | `bool` | Hayır | `true` / `false` |

**Örnek İstek:**
```
GET /api/residents?propertyId=5&isActive=true
GET /api/residents?residentType=Owner
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "residentId": 3,
      "propertyId": 5,
      "fullName": "Ahmet Yılmaz",
      "phoneNumber": "+905551234567",
      "residentType": "Owner",
      "moveInDate": "2022-03-01",
      "moveOutDate": null,
      "isActive": true,
      "createdAt": "2024-01-16T09:00:00Z"
    }
  ],
  "message": null,
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<List<ResidentDto>> GetResidentsAsync(
    long? propertyId = null, string? residentType = null, bool? isActive = null)
{
    var query = new List<string>();
    if (propertyId.HasValue) query.Add($"propertyId={propertyId}");
    if (residentType != null) query.Add($"residentType={Uri.EscapeDataString(residentType)}");
    if (isActive.HasValue) query.Add($"isActive={isActive.Value.ToString().ToLower()}");

    var url = "/api/residents" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
    try
    {
        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<ResidentDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### GET /api/residents/{id}

**Açıklama:** Tek sakin detayı.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "residentId": 3,
    "propertyId": 5,
    "fullName": "Ahmet Yılmaz",
    "phoneNumber": "+905551234567",
    "residentType": "Owner",
    "moveInDate": "2022-03-01",
    "moveOutDate": null,
    "isActive": true,
    "createdAt": "2024-01-16T09:00:00Z"
  },
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Sakin bulunamadı
{ "success": false, "data": null, "message": "Sakin bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<ResidentDto?> GetResidentByIdAsync(long residentId)
{
    try
    {
        var response = await _httpClient.GetAsync($"/api/residents/{residentId}");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ResidentDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/residents

**Açıklama:** Yeni sakin ekler. `POST /api/properties/{id}/assign-resident`'ten farkı: mevcut aktif sakini otomatik pasife çekmez, sadece yeni kayıt oluşturur.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "propertyId": 3,
  "fullName": "Mehmet Demir",
  "phoneNumber": "+905553334455",
  "residentType": "Owner",
  "moveInDate": "2024-01-01"
}
```
> `moveInDate` opsiyoneldir.

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "residentId": 8,
    "propertyId": 3,
    "fullName": "Mehmet Demir",
    "phoneNumber": "+905553334455",
    "residentType": "Owner",
    "moveInDate": "2024-01-01",
    "moveOutDate": null,
    "isActive": true,
    "createdAt": "2024-06-01T08:00:00Z"
  },
  "message": "Sakin eklendi.",
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class CreateResidentRequest
{
    public long PropertyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ResidentType { get; set; } = "Owner";
    public DateOnly? MoveInDate { get; set; }
}

public async Task<ResidentDto?> CreateResidentAsync(CreateResidentRequest request)
{
    var content = new StringContent(
        JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/residents", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ResidentDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### PUT /api/residents/{id}

**Açıklama:** Sakin bilgilerini günceller. Taşınma çıkış tarihi bu endpoint ile kaydedilir.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "fullName": "Mehmet Demir",
  "phoneNumber": "+905553334455",
  "residentType": "Owner",
  "moveInDate": "2024-01-01",
  "moveOutDate": null
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "residentId": 8,
    "propertyId": 3,
    "fullName": "Mehmet Demir",
    "phoneNumber": "+905553334455",
    "residentType": "Owner",
    "moveInDate": "2024-01-01",
    "moveOutDate": null,
    "isActive": true,
    "createdAt": "2024-06-01T08:00:00Z"
  },
  "message": "Sakin güncellendi.",
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class UpdateResidentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ResidentType { get; set; } = "Owner";
    public DateOnly? MoveInDate { get; set; }
    public DateOnly? MoveOutDate { get; set; }
}

public async Task<bool> UpdateResidentAsync(long residentId, UpdateResidentRequest request)
{
    var content = new StringContent(
        JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PutAsync($"/api/residents/{residentId}", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ResidentDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### DELETE /api/residents/{id}

**Açıklama:** Sakini soft-delete yapar (`IsActive = false`, `MoveOutDate = bugün`). Veri silinmez.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": "Sakin silindi.",
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Sakin bulunamadı
{ "success": false, "data": null, "message": "Sakin bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<bool> DeleteResidentAsync(long residentId)
{
    try
    {
        var response = await _httpClient.DeleteAsync($"/api/residents/{residentId}");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<string>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## Dues

### GET /api/dues/definitions

**Açıklama:** Aktif tenant'a ait aidat tanımlarını listeler.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "duesDefId": 1,
      "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Aylık Aidat",
      "amount": 500.00,
      "dueDay": 5,
      "periodType": "Monthly",
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "message": null,
  "errors": null
}
```
> `dueDay`: Ayın kaçında vadesi dolacak (1–31)  
> `periodType` değerleri: `Monthly` | `OneTime`

**MAUI C# Örnek Kullanım:**
```csharp
public class DuesDefinition
{
    public long DuesDefId { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int DueDay { get; set; }
    public string PeriodType { get; set; } = "Monthly";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public async Task<List<DuesDefinition>> GetDuesDefinitionsAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("/api/dues/definitions");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<DuesDefinition>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/dues/definitions

**Açıklama:** Yeni aidat tanımı oluşturur. TenantId JWT'den atanır.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Asansör Bakım",
  "amount": 150.00,
  "dueDay": 10,
  "periodType": "Monthly"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "duesDefId": 2,
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Asansör Bakım",
    "amount": 150.00,
    "dueDay": 10,
    "periodType": "Monthly",
    "isActive": true,
    "createdAt": "2024-06-01T08:00:00Z"
  },
  "message": "Aidat tanımı oluşturuldu.",
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class CreateDuesDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int DueDay { get; set; } = 1;
    public string PeriodType { get; set; } = "Monthly";
}

public async Task<DuesDefinition?> CreateDuesDefinitionAsync(
    string name, decimal amount, int dueDay, string periodType)
{
    var body = new CreateDuesDefinitionRequest
    {
        Name = name, Amount = amount, DueDay = dueDay, PeriodType = periodType
    };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/dues/definitions", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<DuesDefinition>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### PUT /api/dues/definitions/{id}

**Açıklama:** Aidat tanımını günceller. `IsActive = false` ile tanım devre dışı bırakılabilir.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Asansör Bakım (Güncellendi)",
  "amount": 175.00,
  "dueDay": 10,
  "isActive": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "duesDefId": 2,
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Asansör Bakım (Güncellendi)",
    "amount": 175.00,
    "dueDay": 10,
    "periodType": "Monthly",
    "isActive": true,
    "createdAt": "2024-06-01T08:00:00Z"
  },
  "message": "Aidat tanımı güncellendi.",
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Tanım bulunamadı
{ "success": false, "data": null, "message": "Aidat tanımı bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class UpdateDuesDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int DueDay { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}

public async Task<bool> UpdateDuesDefinitionAsync(long duesDefId, UpdateDuesDefinitionRequest request)
{
    var content = new StringContent(
        JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PutAsync($"/api/dues/definitions/{duesDefId}", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<DuesDefinition>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/dues/charge/bulk

**Açıklama:** Seçili aidat tanımı için aktif tüm mülklere aynı anda borç kaydı oluşturur. Aynı mülk + yıl + ay + tanım kombinasyonu zaten varsa o mülk atlanır (duplicate önleme).  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "year": 2024,
  "month": 6,
  "duesDefId": 1
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": "Toplu borçlandırma tamamlandı.",
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Aidat tanımı bulunamadı veya pasif
{ "success": false, "data": null, "message": "Aidat tanımı bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class BulkChargeRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
    public long DuesDefId { get; set; }
}

public async Task<bool> BulkChargeAsync(int year, int month, long duesDefId)
{
    var body = new BulkChargeRequest { Year = year, Month = month, DuesDefId = duesDefId };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/dues/charge/bulk", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<string>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/dues/charge/single

**Açıklama:** Tek bir mülke manuel borç kaydı ekler. Aidat tanımına bağlı olmaksızın özel tutar girilebilir (örn: tamir masrafı).  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "propertyId": 3,
  "duesDefId": null,
  "description": "Su Sayacı Arıza Onarımı",
  "amount": 350.00,
  "dueDate": "2024-06-30",
  "periodYear": null,
  "periodMonth": null
}
```
> `duesDefId`, `periodYear`, `periodMonth` opsiyoneldir. Standart aidat için `duesDefId` verilmeli.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "chargeId": 42,
    "propertyId": 3,
    "duesDefId": null,
    "description": "Su Sayacı Arıza Onarımı",
    "amount": 350.00,
    "paidAmount": 0.00,
    "remainingAmount": 350.00,
    "dueDate": "2024-06-30",
    "periodYear": null,
    "periodMonth": null,
    "status": "Pending",
    "createdAt": "2024-06-01T08:00:00Z"
  },
  "message": "Borç kaydedildi.",
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class SingleChargeRequest
{
    public long PropertyId { get; set; }
    public long? DuesDefId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public int? PeriodYear { get; set; }
    public int? PeriodMonth { get; set; }
}

public class ChargeDto
{
    public long ChargeId { get; set; }
    public long PropertyId { get; set; }
    public long? DuesDefId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateOnly DueDate { get; set; }
    public int? PeriodYear { get; set; }
    public int? PeriodMonth { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public async Task<ChargeDto?> AddSingleChargeAsync(SingleChargeRequest request)
{
    var content = new StringContent(
        JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/dues/charge/single", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ChargeDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## Charges

### GET /api/charges

**Açıklama:** Aktif tenant'a ait borç kayıtlarını listeler. Birden fazla filtre birlikte kullanılabilir.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Query Parameters:**

| Parametre | Tip | Zorunlu | Açıklama |
|---|---|---|---|
| `propertyId` | `long` | Hayır | Belirli mülkün borçları |
| `status` | `string` | Hayır | `Pending` / `Partial` / `Paid` |
| `year` | `int` | Hayır | Periyot yılı |
| `month` | `int` | Hayır | Periyot ayı |

**Örnek İstek:**
```
GET /api/charges?propertyId=5&status=Pending&year=2024&month=6
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "chargeId": 10,
      "propertyId": 5,
      "duesDefId": 1,
      "description": "Aylık Aidat - 2024/06",
      "amount": 500.00,
      "paidAmount": 200.00,
      "remainingAmount": 300.00,
      "dueDate": "2024-06-05",
      "periodYear": 2024,
      "periodMonth": 6,
      "status": "Partial",
      "createdAt": "2024-06-01T08:00:00Z"
    }
  ],
  "message": null,
  "errors": null
}
```
> `status` değerleri: `Pending` (hiç ödenmedi) | `Partial` (kısmi ödendi) | `Paid` (tamamı ödendi)

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<List<ChargeDto>> GetChargesAsync(
    long? propertyId = null, string? status = null, int? year = null, int? month = null)
{
    var query = new List<string>();
    if (propertyId.HasValue) query.Add($"propertyId={propertyId}");
    if (status != null) query.Add($"status={Uri.EscapeDataString(status)}");
    if (year.HasValue) query.Add($"year={year}");
    if (month.HasValue) query.Add($"month={month}");

    var url = "/api/charges" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
    try
    {
        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<ChargeDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### GET /api/charges/{id}

**Açıklama:** Tek borç kaydı detayı.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "chargeId": 10,
    "propertyId": 5,
    "duesDefId": 1,
    "description": "Aylık Aidat - 2024/06",
    "amount": 500.00,
    "paidAmount": 200.00,
    "remainingAmount": 300.00,
    "dueDate": "2024-06-05",
    "periodYear": 2024,
    "periodMonth": 6,
    "status": "Partial",
    "createdAt": "2024-06-01T08:00:00Z"
  },
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Borç bulunamadı
{ "success": false, "data": null, "message": "Borç bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<ChargeDto?> GetChargeByIdAsync(long chargeId)
{
    try
    {
        var response = await _httpClient.GetAsync($"/api/charges/{chargeId}");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ChargeDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### GET /api/charges/summary

**Açıklama:** Dashboard için toplam borç, tahsilat, vadesi geçmiş tutar ve adet özeti döner.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalDebt": 15000.00,
    "totalCollected": 9500.00,
    "totalOverdue": 2300.00,
    "overdueCount": 7
  },
  "message": null,
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class ChargeSummaryDto
{
    public decimal TotalDebt { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalOverdue { get; set; }
    public int OverdueCount { get; set; }
}

public async Task<ChargeSummaryDto?> GetChargeSummaryAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("/api/charges/summary");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ChargeSummaryDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## Payments

### GET /api/payments

**Açıklama:** Aktif tenant'a ait tahsilat (ödeme) kayıtlarını listeler.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Query Parameters:**

| Parametre | Tip | Zorunlu | Açıklama |
|---|---|---|---|
| `propertyId` | `long` | Hayır | Belirli mülkün tahsilatları |
| `dateFrom` | `DateTime` | Hayır | Başlangıç tarihi (`2024-06-01`) |
| `dateTo` | `DateTime` | Hayır | Bitiş tarihi (`2024-06-30`) |

**Örnek İstek:**
```
GET /api/payments?propertyId=3&dateFrom=2024-06-01&dateTo=2024-06-30
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "paymentId": 5,
      "chargeId": 10,
      "propertyId": 3,
      "paidAmount": 500.00,
      "paymentDate": "2024-06-03T14:22:00Z",
      "paymentMethod": "Cash",
      "receiptNumber": "MAK-2024-001",
      "collectedBy": 1,
      "notes": "Elden alındı"
    }
  ],
  "message": null,
  "errors": null
}
```
> `paymentMethod` değerleri: `Cash` | `Transfer` | `Card`

**MAUI C# Örnek Kullanım:**
```csharp
public class PaymentDto
{
    public long PaymentId { get; set; }
    public long ChargeId { get; set; }
    public long PropertyId { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
    public long? CollectedBy { get; set; }
    public string? Notes { get; set; }
}

public async Task<List<PaymentDto>> GetPaymentsAsync(
    long? propertyId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
{
    var query = new List<string>();
    if (propertyId.HasValue) query.Add($"propertyId={propertyId}");
    if (dateFrom.HasValue) query.Add($"dateFrom={dateFrom.Value:yyyy-MM-dd}");
    if (dateTo.HasValue) query.Add($"dateTo={dateTo.Value:yyyy-MM-dd}");

    var url = "/api/payments" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
    try
    {
        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<PaymentDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### GET /api/payments/{id}

**Açıklama:** Tek tahsilat kaydı detayı.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "paymentId": 5,
    "chargeId": 10,
    "propertyId": 3,
    "paidAmount": 500.00,
    "paymentDate": "2024-06-03T14:22:00Z",
    "paymentMethod": "Cash",
    "receiptNumber": "MAK-2024-001",
    "collectedBy": 1,
    "notes": "Elden alındı"
  },
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Tahsilat bulunamadı
{ "success": false, "data": null, "message": "Tahsilat bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<PaymentDto?> GetPaymentByIdAsync(long paymentId)
{
    try
    {
        var response = await _httpClient.GetAsync($"/api/payments/{paymentId}");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PaymentDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/payments

**Açıklama:** Borç için tahsilat kaydeder. Kısmi ödeme (partial payment) desteklenir. Ödeme sonrası ilgili `Charge.PaidAmount` güncellenir ve durum otomatik hesaplanır: tam ödendiyse `Paid`, eksik kaldıysa `Partial`.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "chargeId": 10,
  "paidAmount": 300.00,
  "paymentMethod": "Transfer",
  "receiptNumber": "TRF-2024-0603",
  "notes": "EFT ile yapıldı"
}
```
> `receiptNumber` ve `notes` opsiyoneldir. `paymentMethod` varsayılanı `Cash`.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "paymentId": 6,
    "chargeId": 10,
    "propertyId": 3,
    "paidAmount": 300.00,
    "paymentDate": "2024-06-03T14:30:00Z",
    "paymentMethod": "Transfer",
    "receiptNumber": "TRF-2024-0603",
    "collectedBy": 1,
    "notes": "EFT ile yapıldı"
  },
  "message": "Tahsilat kaydedildi.",
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Borç bulunamadı
{ "success": false, "data": null, "message": "Borç bulunamadı.", "errors": null }

// 400 — Ödeme tutarı kalan borçtan fazla
{ "success": false, "data": null, "message": "Ödeme tutarı kalan borçtan fazla olamaz.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public class CreatePaymentRequest
{
    public long ChargeId { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? ReceiptNumber { get; set; }
    public string? Notes { get; set; }
}

public async Task<PaymentDto?> RecordPaymentAsync(
    long chargeId, decimal amount, string method,
    string? receipt = null, string? notes = null)
{
    var body = new CreatePaymentRequest
    {
        ChargeId = chargeId,
        PaidAmount = amount,
        PaymentMethod = method,
        ReceiptNumber = receipt,
        Notes = notes
    };
    var content = new StringContent(
        JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/payments", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PaymentDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### DELETE /api/payments/{id}

**Açıklama:** Tahsilatı iptal eder (siler). İlgili `Charge.PaidAmount` geri düşürülür, durum otomatik `Pending` veya `Partial`'a döner.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": "Tahsilat iptal edildi.",
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 404 — Tahsilat bulunamadı
{ "success": false, "data": null, "message": "Tahsilat bulunamadı.", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<bool> CancelPaymentAsync(long paymentId)
{
    try
    {
        var response = await _httpClient.DeleteAsync($"/api/payments/{paymentId}");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<string>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## Notifications

### GET /api/notifications

**Açıklama:** Aktif tenant'a ait gönderilmiş bildirim geçmişini listeler.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "notifId": 3,
      "title": "Haziran Aidatı Hatırlatması",
      "body": "Haziran ayı aidatınızın son ödeme günü 5 Haziran'dır.",
      "targetType": "All",
      "targetId": null,
      "sentAt": "2024-06-01T09:00:00Z",
      "sentBy": 1
    }
  ],
  "message": null,
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class NotificationDto
{
    public long NotifId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public DateTime SentAt { get; set; }
    public long? SentBy { get; set; }
}

public async Task<List<NotificationDto>> GetNotificationsAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("/api/notifications");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<NotificationDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### POST /api/notifications

**Açıklama:** Tüm sakine, belirli bloka, mülke veya bireysel sakine bildirim gönderir.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "Su Kesintisi",
  "body": "Yarın 10:00-14:00 arası su kesintisi yaşanacaktır.",
  "targetType": "Block",
  "targetId": "A Blok"
}
```

`targetType` / `targetId` kombinasyonları:

| targetType | targetId | Açıklama |
|---|---|---|
| `All` | `null` | Tüm sakinler |
| `Block` | `"A Blok"` | Belirtilen blok sakinleri |
| `Property` | `"5"` | Belirtilen mülk sakini |
| `Resident` | `"+905551234567"` | Belirtilen telefon numarasına sahip sakin |

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "notifId": 4,
    "title": "Su Kesintisi",
    "body": "Yarın 10:00-14:00 arası su kesintisi yaşanacaktır.",
    "targetType": "Block",
    "targetId": "A Blok",
    "sentAt": "2024-06-04T11:00:00Z",
    "sentBy": 1
  },
  "message": "Bildirim gönderildi.",
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class SendNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string TargetType { get; set; } = "All";
    public string? TargetId { get; set; }
}

public async Task<NotificationDto?> SendNotificationAsync(
    string title, string body, string targetType = "All", string? targetId = null)
{
    var bodyObj = new SendNotificationRequest
    {
        Title = title, Body = body, TargetType = targetType, TargetId = targetId
    };
    var content = new StringContent(
        JsonSerializer.Serialize(bodyObj), Encoding.UTF8, "application/json");
    try
    {
        var response = await _httpClient.PostAsync("/api/notifications", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<NotificationDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Success == true ? result.Data : null;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## Reports

### GET /api/reports/monthly-collection

**Açıklama:** Belirtilen ay için oluşturulan toplam borç, tahsil edilen, bekleyen tutar ve mülk sayısı özetini döner.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Query Parameters:**

| Parametre | Tip | Zorunlu | Açıklama |
|---|---|---|---|
| `year` | `int` | Evet | Rapor yılı |
| `month` | `int` | Evet | Rapor ayı (1–12) |

**Örnek İstek:**
```
GET /api/reports/monthly-collection?year=2024&month=6
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "year": 2024,
    "month": 6,
    "totalCharged": 25000.00,
    "totalCollected": 18500.00,
    "totalPending": 6500.00,
    "totalProperties": 50
  },
  "message": null,
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class MonthlyCollectionDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalCharged { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalPending { get; set; }
    public int TotalProperties { get; set; }
}

public async Task<MonthlyCollectionDto?> GetMonthlyCollectionAsync(int year, int month)
{
    try
    {
        var response = await _httpClient.GetAsync(
            $"/api/reports/monthly-collection?year={year}&month={month}");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<MonthlyCollectionDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### GET /api/reports/debt-aging

**Açıklama:** Vadesi geçmiş borçları yaş gruplarına göre toplar: 0–30, 31–60, 61–90, 90+ gün.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "days0To30": 3200.00,
    "days31To60": 1500.00,
    "days61To90": 800.00,
    "daysOver90": 450.00
  },
  "message": null,
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class DebtAgingDto
{
    public decimal Days0To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal DaysOver90 { get; set; }
}

public async Task<DebtAgingDto?> GetDebtAgingAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("/api/reports/debt-aging");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<DebtAgingDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data;
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### GET /api/reports/property-summary

**Açıklama:** Her mülk için toplam borç, toplam ödeme ve net bakiye listesi. Dashboard'daki mülk kartları için kullanın.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "propertyId": 1,
      "blockName": "A Blok",
      "doorNumber": "5",
      "totalDebt": 1500.00,
      "totalPaid": 1000.00,
      "balance": 500.00
    }
  ],
  "message": null,
  "errors": null
}
```
> `balance` = `totalDebt - totalPaid` (server-side hesaplanır)

**MAUI C# Örnek Kullanım:**
```csharp
public class PropertySummaryDto
{
    public long PropertyId { get; set; }
    public string? BlockName { get; set; }
    public string DoorNumber { get; set; } = string.Empty;
    public decimal TotalDebt { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Balance { get; set; }
}

public async Task<List<PropertySummaryDto>> GetPropertySummaryAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("/api/reports/property-summary");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<PropertySummaryDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### GET /api/reports/overdue

**Açıklama:** Vadesi geçmiş tüm borçları mülk bilgileriyle birlikte listeler. `daysOverdue` alanı bugün ile vade tarihi arasındaki gün farkıdır.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "chargeId": 8,
      "propertyId": 3,
      "blockName": "A Blok",
      "doorNumber": "3",
      "amount": 500.00,
      "paidAmount": 0.00,
      "dueDate": "2024-05-05",
      "daysOverdue": 30,
      "status": "Pending"
    }
  ],
  "message": null,
  "errors": null
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public class OverdueChargeDto
{
    public long ChargeId { get; set; }
    public long PropertyId { get; set; }
    public string? BlockName { get; set; }
    public string DoorNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateOnly DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public string Status { get; set; } = string.Empty;
}

public async Task<List<OverdueChargeDto>> GetOverdueChargesAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("/api/reports/overdue");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<OverdueChargeDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## PhoneMap

### GET /api/phonemap/my-properties

**Açıklama:** Oturum açmış mülk sahibinin (OTP ile doğrulanmış) erişebildiği mülkleri döner. `phoneNumber` claim'den okunur; o telefona kayıtlı tüm mülkler listelenir.  
**Yetki:** Resident Token (OTP JWT — `phoneNumber` claim zorunlu)  
**Tenant Scope:** Evet (tenantId JWT'den alınır)

**Request Headers:**
```
Authorization: Bearer {residentToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "propertyId": 5,
      "blockName": "B Blok",
      "doorNumber": "8",
      "floor": 3,
      "propertyType": "Apartment",
      "isActive": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "activeResident": null
    }
  ],
  "message": null,
  "errors": null
}
```

**Response (Hata):**
```json
// 403 — phoneNumber claim yok (Admin token ile çağrıldı)
{ "success": false, "data": null, "message": "Forbidden", "errors": null }
```

**MAUI C# Örnek Kullanım:**
```csharp
// OTP verify sonrası alınan resident token set edilmiş olmalı
public async Task<List<PropertyDto>> GetMyPropertiesAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("/api/phonemap/my-properties");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<PropertyDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## Eksik / Önerilen Endpointler

> Aşağıdaki endpointler mevcut kodda **bulunmamaktadır**. Resident uygulaması ve tam CRUD akışı için eklenmesi önerilir.

---

### GET /api/dues/definitions/{id} ⚠️ EKSİK

**Açıklama:** Tek aidat tanımı detayını döner. Şu an yalnızca liste endpoint'i mevcut; tekil sorgulama için bu endpoint gereklidir.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "duesDefId": 1,
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Aylık Aidat",
    "amount": 500.00,
    "dueDay": 5,
    "periodType": "Monthly",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

**Eklenecek Controller Kodu:**
```csharp
/// <summary>Aidat tanımı detayı — Manager/SuperAdmin</summary>
[HttpGet("definitions/{id:long}")]
public async Task<ActionResult<ApiResponse<DuesDefinition>>> GetDefinitionById(long id)
{
    var def = await _duesService.GetDefinitionByIdAsync(id);
    return Ok(ApiResponse<DuesDefinition>.Ok(def));
}
```

---

### DELETE /api/dues/definitions/{id} ⚠️ EKSİK

**Açıklama:** Aidat tanımını soft-delete yapar (`IsActive = false`). Şu an devre dışı bırakmak için `PUT` ile `isActive: false` göndermek gerekiyor; ayrı bir DELETE endpoint'i beklenen standarttır.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "data": "Aidat tanımı devre dışı bırakıldı.",
  "message": null,
  "errors": null
}
```

**Eklenecek Controller Kodu:**
```csharp
/// <summary>Aidat tanımı devre dışı bırak — Manager/SuperAdmin</summary>
[HttpDelete("definitions/{id:long}")]
public async Task<ActionResult<ApiResponse<string>>> DeleteDefinition(long id)
{
    await _duesService.DeactivateDefinitionAsync(id);
    return Ok(ApiResponse<string>.Ok("Aidat tanımı devre dışı bırakıldı."));
}
```

---

### GET /api/notifications/{id} ⚠️ EKSİK

**Açıklama:** Tek bildirim detayını döner. Bildirim geçmişinde bir bildirimin tam içeriğini görmek için gereklidir.  
**Yetki:** Tenant Token (Bearer) — Manager / SuperAdmin  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {tenantToken}
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "notifId": 3,
    "title": "Su Kesintisi",
    "body": "Yarın 10:00-14:00 arası su kesintisi yaşanacaktır.",
    "targetType": "Block",
    "targetId": "A Blok",
    "sentAt": "2024-06-04T11:00:00Z",
    "sentBy": 1
  }
}
```

**Eklenecek Controller Kodu:**
```csharp
/// <summary>Bildirim detayı — Manager/SuperAdmin</summary>
[HttpGet("{id:long}")]
public async Task<ActionResult<ApiResponse<NotificationDto>>> GetById(long id)
{
    var notif = await _notificationService.GetByIdAsync(id);
    return Ok(ApiResponse<NotificationDto>.Ok(notif));
}
```

---

### GET /api/phonemap/my-charges ⚠️ EKSİK

**Açıklama:** Mülk sahibinin kendi mülklerine ait borçlarını görüntüler. Resident uygulamasının borç ekranı için kritik öneme sahiptir; mevcut `GET /api/charges` endpoint'i Admin token gerektirdiğinden resident tarafından kullanılamaz.  
**Yetki:** Resident Token (OTP JWT — `phoneNumber` claim zorunlu)  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {residentToken}
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "chargeId": 10,
      "propertyId": 5,
      "duesDefId": 1,
      "description": "Aylık Aidat - 2024/06",
      "amount": 500.00,
      "paidAmount": 200.00,
      "remainingAmount": 300.00,
      "dueDate": "2024-06-05",
      "periodYear": 2024,
      "periodMonth": 6,
      "status": "Partial",
      "createdAt": "2024-06-01T08:00:00Z"
    }
  ]
}
```

**Eklenecek Controller Kodu:**
```csharp
/// <summary>Sakin kendi borçlarını görür — OTP JWT gerektirir</summary>
[HttpGet("my-charges")]
public async Task<ActionResult<ApiResponse<IEnumerable<ChargeDto>>>> MyCharges(
    [FromQuery] string? status = null)
{
    var phone = _tenantContext.PhoneNumber;
    if (string.IsNullOrEmpty(phone)) return Forbid();

    var propertyIds = await _phoneMapRepo.GetPropertyIdsByPhoneAsync(phone, _tenantContext.TenantId);
    var charges = new List<ChargeDto>();
    foreach (var pid in propertyIds)
    {
        var propCharges = await _chargeService.GetAllAsync(pid, status, null, null);
        charges.AddRange(propCharges);
    }
    return Ok(ApiResponse<IEnumerable<ChargeDto>>.Ok(charges));
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<List<ChargeDto>> GetMyChargesAsync(string? status = null)
{
    var url = "/api/phonemap/my-charges" + (status != null ? $"?status={status}" : "");
    try
    {
        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<ChargeDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

### GET /api/phonemap/my-payments ⚠️ EKSİK

**Açıklama:** Mülk sahibinin kendi mülklerine ait ödeme geçmişini döner. Resident uygulamasının ödeme geçmişi ekranı için gereklidir.  
**Yetki:** Resident Token (OTP JWT — `phoneNumber` claim zorunlu)  
**Tenant Scope:** Evet

**Request Headers:**
```
Authorization: Bearer {residentToken}
```

**Beklenen Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "paymentId": 5,
      "chargeId": 10,
      "propertyId": 5,
      "paidAmount": 500.00,
      "paymentDate": "2024-06-03T14:22:00Z",
      "paymentMethod": "Cash",
      "receiptNumber": "MAK-2024-001",
      "collectedBy": 1,
      "notes": null
    }
  ]
}
```

**Eklenecek Controller Kodu:**
```csharp
/// <summary>Sakin kendi ödemelerini görür — OTP JWT gerektirir</summary>
[HttpGet("my-payments")]
public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> MyPayments()
{
    var phone = _tenantContext.PhoneNumber;
    if (string.IsNullOrEmpty(phone)) return Forbid();

    var propertyIds = await _phoneMapRepo.GetPropertyIdsByPhoneAsync(phone, _tenantContext.TenantId);
    var payments = new List<PaymentDto>();
    foreach (var pid in propertyIds)
    {
        var propPayments = await _paymentService.GetAllAsync(pid, null, null);
        payments.AddRange(propPayments);
    }
    return Ok(ApiResponse<IEnumerable<PaymentDto>>.Ok(payments));
}
```

**MAUI C# Örnek Kullanım:**
```csharp
public async Task<List<PaymentDto>> GetMyPaymentsAsync()
{
    try
    {
        var response = await _httpClient.GetAsync("/api/phonemap/my-payments");
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<PaymentDto>>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result?.Data ?? [];
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Sunucuya ulaşılamıyor: " + ex.Message);
    }
}
```

---

## MAUI Temel Altyapı

### ApiResponse Model

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}
```

### MauiProgram.cs Kayıt

```csharp
builder.Services.AddSingleton(sp =>
{
    var client = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7001/"),
        Timeout = TimeSpan.FromSeconds(30)
    };
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    return client;
});

builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<PropertyService>();
// ... diğer servisler
```

### Token Yönetimi

```csharp
public class TokenManager
{
    private readonly HttpClient _httpClient;

    public TokenManager(HttpClient httpClient) => _httpClient = httpClient;

    public async Task LoadSavedTokenAsync()
    {
        var token = await SecureStorage.GetAsync("tenant_token")
                    ?? await SecureStorage.GetAsync("resident_token");
        if (!string.IsNullOrEmpty(token))
            SetToken(token);
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task ClearAsync()
    {
        SecureStorage.Remove("pre_auth_token");
        SecureStorage.Remove("tenant_token");
        SecureStorage.Remove("resident_token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
```

### Global Hata Yakalayıcı

```csharp
public static class ApiExtensions
{
    public static async Task<ApiResponse<T>?> SafeCallAsync<T>(
        this Func<Task<ApiResponse<T>?>> call,
        string errorTitle = "Hata")
    {
        try
        {
            return await call();
        }
        catch (HttpRequestException)
        {
            await Shell.Current.DisplayAlert(errorTitle,
                "Sunucuya ulaşılamıyor. İnternet bağlantınızı kontrol edin.", "Tamam");
            return null;
        }
        catch (TaskCanceledException)
        {
            await Shell.Current.DisplayAlert(errorTitle,
                "İstek zaman aşımına uğradı. Lütfen tekrar deneyin.", "Tamam");
            return null;
        }
    }
}

// Kullanım:
var result = await ((Func<Task<ApiResponse<List<PropertyDto>>?>>)
    (() => _api.GetAsync<List<PropertyDto>>("properties")))
    .SafeCallAsync();

if (result?.Success == true)
    Properties = result.Data ?? [];
else
    await DisplayAlert("Hata", result?.Message ?? "Bilinmeyen hata", "Tamam");
```

---

## Hata Kodları

| HTTP Kodu | Anlamı | Yaygın Sebepler |
|---|---|---|
| `200` | Başarılı | — |
| `201` | Oluşturuldu | POST ile yeni kayıt oluşturuldu |
| `400` | Geçersiz İstek | Zorunlu alan eksik, iş kuralı ihlali |
| `401` | Yetkisiz | JWT eksik, geçersiz veya süresi dolmuş |
| `403` | Yasak | Yanlış token tipi (ör: Admin token ile resident endpoint) |
| `404` | Bulunamadı | İstenen kayıt mevcut değil veya başka tenant'a ait |
| `500` | Sunucu Hatası | Beklenmeyen hata — sunucu loglarını kontrol edin |

> **401 geldiğinde:** Token silinerek login sayfasına yönlendirilmeli.  
> **403 geldiğinde:** Kullanıcıya yetersiz yetki mesajı gösterilmeli, logout **yapılmamalı**.  
> **OTP notu:** Şu an OTP SMS ile gönderilmez, sunucu loglarına yazılır. Geliştirme sırasında `dotnet run` çıktısından OTP değeri okunabilir.
