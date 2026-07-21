# CareerPilot AI — Dual Runtime Architecture (Local-First & SaaS)

CareerPilot AI supports a configurable **dual runtime hosting architecture** (`Local` vs `SaaS`) via strongly typed configuration (`HostingOptions`), maintaining 100% compliance with Clean Architecture principles.

---

## 1. High-Level Concept

| Feature / Subsystem | Local Mode (Default) | SaaS Mode (Enterprise Commercial) |
|---|---|---|
| **Target Deployment** | Single-user personal desktop / self-hosted | Multi-tenant cloud application |
| **Authentication Screen** | Bypassed. Application opens directly to Dashboard | Enforces Sign-in (`/auth/login`) & Register |
| **User Identity Context** | Auto-provisions and binds to `LocalUserProvider` (`local.user@careerpilot.internal`) | Extracted dynamically from JWT Bearer Claims (`sub`, `jti`) |
| **Middleware Pipeline** | Skips `app.UseAuthentication()` & `app.UseAuthorization()` | Enforces JWT Bearer validation & policy handlers |
| **Angular Route Guards** | `authGuard` returns `true` unconditionally | `authGuard` checks session & redirects unauthenticated users |
| **Angular HTTP Interceptor** | Bypasses Bearer token header injection | Injects `Authorization: Bearer <token>` and handles 401 refresh |
| **External OAuth (Google/Outlook)** | Preserved exclusively for Email & Calendar sync APIs | Used for both application sign-in & integrations |
| **Secure Credentials Store** | Encrypted OS Vault / File store (`ISecureCredentialStore`) | Azure KeyVault / HashiCorp Vault / KMS |

---

## 2. Options Pattern Configuration (`appsettings.json`)

```json
{
  "Hosting": {
    "Mode": "Local",
    "AutoCreateLocalUser": true,
    "DefaultLocalUserEmail": "local.user@careerpilot.internal",
    "DefaultLocalUserName": "Local User"
  }
}
```

To switch to commercial SaaS mode, change `"Mode": "SaaS"`.

---

## 3. Backend Dependency Injection & Identity Resolution

- **`HostingOptions`**: Strongly typed configuration section read at composition root in `DependencyInjection.cs`.
- **`LocalUserProvider`**: Seeding service that checks EF Core PostgreSQL on startup and provisions the default local user (`Guid.Parse("11111111-1111-1111-1111-111111111111")`).
- **`ICurrentUserService` Resolution**:
  - In **Local Mode**: `LocalCurrentUserService` yields the local user ID. Handlers and MediatR queries execute identically without duplicate code.
  - In **SaaS Mode**: `CurrentUserService` resolves identity from HTTP context JWT claims.

---

## 4. Secure Credential Storage Abstraction (`ISecureCredentialStore`)

- **Interface**:
  ```csharp
  public interface ISecureCredentialStore
  {
      Task SetCredentialAsync(string key, string secret, CancellationToken cancellationToken);
      Task<string?> GetCredentialAsync(string key, CancellationToken cancellationToken);
      Task<bool> DeleteCredentialAsync(string key, CancellationToken cancellationToken);
  }
  ```
- **Implementations**:
  - `EncryptedFileCredentialStore` / macOS Keychain integration for local desktop token storage.

---

## 5. Migration Path to Commercial SaaS

When transitioning CareerPilot AI to a commercial SaaS product:
1. Set `"Hosting:Mode": "SaaS"` in production configuration.
2. Deploy PostgreSQL and Redis managed instances.
3. No business logic, CQRS handlers, EF Core DbContext configurations, or Angular components require rewriting.
