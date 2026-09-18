---
name: dapper-data-access
description: SiteYonetim API'sinde her türlü veritabanı erişimi için kullan. SQL Server'a Dapper ile bağlanma, repository yazma, parametreli sorgu, transaction, soft delete filtresi ve çok-adımlı yazma işlemleri gerektiğinde MUTLAKA bu skill'i uygula. "sorgu", "repository", "veritabanı", "Dapper", "tahsilat kaydet", "tahakkuk üret" gibi her durumda geçerlidir. EF Core / LINQ-to-SQL ASLA kullanma.
---

# Dapper Veri Erişimi

Bu projede veri erişimi **yalnızca Dapper** ile yapılır. EF Core, LINQ-to-SQL veya başka ORM eklenmez.

## Temel Kurallar

- **Parametreli sorgu zorunlu.** SQL'i string birleştirmeyle KURMA. Değerler daima `@param` ile geçer (SQL injection).
- **Katman:** SQL yalnızca `Repositories` içinde. Controller/endpoint veya Service içinde ham SQL yazma.
- **Soft delete:** Tüm okuma sorguları `IsDeleted = 0` filtreler (log tabloları hariç).
- **Async:** `QueryAsync` / `ExecuteAsync` kullan, `CancellationToken` geçir.
- **Tenant:** `SiteId` daima parametre olarak dışarıdan (JWT'den çözülmüş) gelir; sorgu içinde sabitlenmez.
- **Para:** `decimal`. **Dönem:** `int` (YYYYMM).

## Bağlantı Yönetimi

Bağlantıyı bir factory üzerinden aç, `using` ile kapat. Connection string `appsettings` → `IConfiguration`.

```csharp
public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateAsync(CancellationToken ct = default);
}

public sealed class SqlConnectionFactory(IConfiguration config) : IDbConnectionFactory
{
    private readonly string _cs = config.GetConnectionString("Default")!;
    public async Task<IDbConnection> CreateAsync(CancellationToken ct = default)
    {
        var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);
        return conn;
    }
}
```

## Repository Deseni

Her tablo/özellik için bir repository. Dönen tipler entity değil, iç modeller olabilir; DTO'ya map'leme Service katmanında.

```csharp
public sealed class PropertyRepository(IDbConnectionFactory factory)
{
    public async Task<IReadOnlyList<Property>> GetBySiteAsync(long siteId, CancellationToken ct)
    {
        const string sql = """
            SELECT Id, SiteId, Block, UnitNo, Type, OwnerName, OwnerPhone,
                   ResidentName, ResidentPhone, DuesResponsible, IsActive
            FROM dbo.Properties
            WHERE SiteId = @siteId AND IsDeleted = 0
            ORDER BY Block, UnitNo;
            """;
        using var conn = await factory.CreateAsync(ct);
        var rows = await conn.QueryAsync<Property>(
            new CommandDefinition(sql, new { siteId }, cancellationToken: ct));
        return rows.AsList();
    }
}
```

## Transaction (çok-adımlı yazma)

Tahsilat gibi birden fazla tabloyu etkileyen işlemler **tek transaction** içinde. Örnek: ödeme ekle → charge güncelle → fazlaysa kredi yaz.

```csharp
public async Task<PaymentResult> CollectAsync(CollectDuesCommand cmd, CancellationToken ct)
{
    using var conn = await factory.CreateAsync(ct);
    using var tx = conn.BeginTransaction();
    try
    {
        // 1) charge'ı kilitle + oku
        var charge = await conn.QuerySingleAsync<DuesCharge>(new CommandDefinition(
            """
            SELECT Id, Amount, PaidAmount, Status FROM dbo.DuesCharges WITH (UPDLOCK, ROWLOCK)
            WHERE Id = @chargeId AND IsDeleted = 0;
            """, new { cmd.ChargeId }, tx, cancellationToken: ct));

        var remaining = charge.Amount - charge.PaidAmount;
        var applied   = Math.Min(cmd.Amount, remaining);
        var overpay   = cmd.Amount - applied;

        // 2) ödeme kaydı
        var paymentId = await conn.ExecuteScalarAsync<long>(new CommandDefinition(
            """
            INSERT INTO dbo.Payments (SiteId, PropertyId, TargetType, TargetId, Amount, Method, Note, CollectedByUserId)
            OUTPUT INSERTED.Id
            VALUES (@SiteId, @PropertyId, 'Dues', @ChargeId, @Amount, @Method, @Note, @UserId);
            """, cmd, tx, cancellationToken: ct));

        // 3) charge güncelle
        var newPaid   = charge.PaidAmount + applied;
        var newStatus = newPaid >= charge.Amount ? "Paid" : (newPaid > 0 ? "Partial" : "Pending");
        await conn.ExecuteAsync(new CommandDefinition(
            "UPDATE dbo.DuesCharges SET PaidAmount = @newPaid, Status = @newStatus WHERE Id = @Id;",
            new { newPaid, newStatus, charge.Id }, tx, cancellationToken: ct));

        // 4) fazla ödeme → kredi
        if (overpay > 0)
            await AddCreditAsync(conn, tx, cmd.SiteId, cmd.PropertyId, overpay, "Overpayment", paymentId, ct);

        tx.Commit();
        return new PaymentResult(paymentId, newStatus, charge.Amount - newPaid, overpay);
    }
    catch
    {
        tx.Rollback();
        throw;
    }
}
```

## Yapma

- String interpolation ile SQL kurma (`$"... WHERE Id = {id}"`).
- Repository dışında SQL yazma.
- `SELECT *` kullanma; kolonları açıkça yaz.
- Transaction'ı `using` olmadan açık bırakma; hata halinde `Rollback`.
- Okuma sorgusunda `IsDeleted = 0` filtresini unutma.

## Migration

Şema değişikliği koda değil, `db/migrations/` altına sıralı SQL dosyası olarak yazılır. Detay: `docs/architecture.md`.
