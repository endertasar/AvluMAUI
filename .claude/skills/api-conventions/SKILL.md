---
name: api-conventions
description: SiteYonetim Web API'sinde herhangi bir endpoint, DTO, hata yönetimi veya katman yazarken MUTLAKA bu skill'i uygula. Endpoint ekleme, request/response şekli, doğrulama, ProblemDetails hata formatı, sayfalama, HTTP durum kodları ve katman sorumlulukları burada tanımlıdır. "endpoint", "controller", "servis", "DTO", "API'ye ... ekle" gibi her durumda geçerlidir.
---

# API Konvansiyonları

`.NET 10` Web API. Endpoint'ler **attribute-routed Controller** ile, özelliğe göre yazılır (`ApiController`). Base yol `/api/v1`.

## Katman Sorumlulukları

```
Controllers (HTTP)  →  Services (iş kuralı)  →  Repositories (Dapper/SQL)
```

- **Controller:** İstek al, doğrula, Service çağır, sonucu HTTP'ye çevir. İş kuralı veya SQL YOK. İnce tut.
- **Service:** İş kuralı, transaction orkestrasyonu, DTO ↔ iç model dönüşümü, audit yazımı.
- **Repository:** Yalnızca veri erişimi (bkz. `dapper-data-access` skill).

## Controller Deseni

Her özellik bir controller. `[ApiController]` + attribute routing. Bağımlılıklar primary constructor ile inject edilir.

```csharp
[ApiController]
[Route("api/v1/properties")]
[Authorize(Policy = "AdminOnly")]
[Produces("application/json")]
public sealed class PropertiesController(PropertyService svc) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PropertyResponse>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var siteId = User.GetSiteId();                    // JWT'den; istemciden ASLA
        var result = await svc.ListAsync(siteId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PropertyResponse>> Create(
        [FromBody] CreatePropertyRequest req, CancellationToken ct)
    {
        var siteId = User.GetSiteId();
        var created = await svc.CreateAsync(siteId, req, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PropertyResponse>> GetById(long id, CancellationToken ct)
    {
        var siteId = User.GetSiteId();
        var item = await svc.GetAsync(siteId, id, ct);
        return item is null ? NotFound() : Ok(item);
    }
}
```

- `SiteId` claim'i `ClaimsPrincipal` uzantısından okunur (bkz. `auth-multitenancy`):
  `public static long GetSiteId(this ClaimsPrincipal user) => ...`
- Yetki controller/action seviyesinde `[Authorize(Policy = "...")]` ile.
- Rota parametreleri tipli: `{id:long}`.

## DTO Kuralları

- Request/response DTO'ları `SiteYonetim.Shared` içinde (`record` tercih edilir).
- Entity/DB modelini istemciye DÖNME; her zaman DTO.
- İstemci `SiteId` gönderemez; gönderse bile yok sayılır (tenant izolasyonu).
- İsimlendirme: `CreatePropertyRequest`, `PropertyResponse`, `PagedResult<T>`.

## Sayfalama Zarfı

```csharp
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
```

Liste endpoint'leri `?page=1&pageSize=20` alır; `pageSize` üst sınırı (ör. 100) serviste zorlanır.

## Hata Yönetimi (ProblemDetails)

- Beklenen iş hataları için anlamlı durum kodu: `400` doğrulama, `401` kimlik, `403` yetki, `404` bulunamadı, `409` çakışma.
- Global exception handler `ProblemDetails` döner; gizli detay sızdırma.
- İş hatalarını özel exception'larla ifade et (ör. `NotFoundException`, `ConflictException`) ve tek yerde HTTP'ye map'le.

```csharp
app.UseExceptionHandler();               // ProblemDetails middleware
builder.Services.AddProblemDetails();
```

## Doğrulama

- Basit alan doğrulaması endpoint sınırında (zorunlu alan, format).
- Karmaşık iş kuralı doğrulaması Service içinde.
- Doğrulama hatası → `400` + `ValidationProblem` (alan bazlı mesaj). Kullanıcıya görünen mesajlar Türkçe.

## Genel

- Her handler `async` + `CancellationToken`.
- `Nullable` açık; null durumları açıkça ele al.
- Finansal işlemlerde (tahsilat, gider, tanım) Service `AuditLogs`'a yazar.
- OpenAPI açık; controller'lar `[Tags("...")]` veya varsayılan controller adıyla gruplanır.

## Yapma

- Endpoint içinde SQL veya iş kuralı.
- `SiteId`'yi request'ten okuma.
- Ham exception'ı istemciye yansıtma.
- Entity'yi doğrudan dönme.
