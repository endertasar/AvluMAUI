---
name: git-workflow
description: SiteYonetim reposunda commit atarken, branch açarken, değişiklik hazırlarken veya sürüm/gizli bilgi yönetimi yaparken MUTLAKA bu skill'i uygula. Branch isimlendirme, conventional commit formatı, gizli bilgilerin (connection string, API key) commit'lenmemesi ve migration dosyalarının sıralı yönetimi burada tanımlıdır. "commit", "branch", "git", "push", "gizli anahtar", "secret" gibi her durumda geçerli.
---

# Git İş Akışı

## Branch İsimlendirme

- `feature/<kısa-açıklama>` — yeni özellik (ör. `feature/dues-collection`)
- `fix/<kısa-açıklama>` — hata düzeltme
- `chore/<kısa-açıklama>` — bakım, bağımlılık, yapılandırma
- `main` korumalı; doğrudan push yok, PR ile merge.

## Commit Mesajları (Conventional Commits)

Format: `<type>(<scope>): <özet>` — özet İngilizce, emir kipi, ~72 karakter.

- Tipler: `feat`, `fix`, `chore`, `refactor`, `docs`, `test`, `perf`, `build`.
- Scope opsiyonel ama faydalı: `api`, `admin`, `resident`, `shared`, `db`.

Örnekler:
```
feat(api): add dues collection endpoint with partial payment
fix(resident): correct phone match for multi-site properties
chore(db): add 0003 migration for property balances
```

- Küçük, odaklı commit'ler; tek commit'te alakasız değişiklikleri karıştırma.
- Bir commit derlenebilir/çalışır durumu bozmamalı.

## Gizli Bilgiler (kritik)

- Connection string, JWT signing key, OneSignal App Id/API key, SMS anahtarları **ASLA commit'lenmez**.
- Yerelde **user-secrets** veya ortam değişkeni; sunucuda gizli yönetimi (ör. environment/secret store).
- `appsettings.Development.json` gibi gizli içeren dosyalar `.gitignore`'da.
- Repoda yalnızca **örnek** dosya tutulur: `appsettings.example.json` (gerçek değerler boş/placeholder).

`.gitignore` en azından şunları içerir:
```
bin/
obj/
*.user
appsettings.Development.json
appsettings.*.local.json
.vs/
```

## Migration Dosyaları

- `db/migrations/` altında **sıralı ve değişmez**: `0001_init.sql`, `0002_...sql`.
- Yayınlanmış (merge edilmiş) bir migration'ı DÜZENLEME; düzeltme için yeni numaralı dosya ekle.
- Her migration ayrı, açıklayıcı commit ile gelir.

## PR Öncesi

- Derleme ve testler yeşil.
- Gereksiz dosya/gizli bilgi eklenmemiş.
- Değişiklik ilgili skill kurallarına uygun (Dapper, api-conventions, auth).

## Yapma

- `main`'e doğrudan push.
- Gizli değer commit'leme.
- Eski migration'ı değiştirme.
- Dev/derleme çıktısı (`bin/`, `obj/`) commit'leme.
- Tek dev commit'te devasa, karışık değişiklik gönderme.
