# PharmaTrack Pro

A production-quality **Pharmaceutical Inventory & Pharmacy Management System** built with ASP.NET Core 9 MVC, designed to reflect real-world software development practices for a professional GitHub portfolio.

> **Status:** ✅ Feature-complete. All 20 modules from the original project spec are implemented. See [Future Improvements](#future-improvements) for optional polish beyond the original scope.

---

## Project Overview

PharmaTrack Pro helps pharmacies manage medicines, inventory, purchases, sales, suppliers, customers, and staff — all from a single, role-based web application that runs entirely on a local Windows machine (no cloud, no Docker, no paid services).

## Features

- [x] Project foundation (Identity, EF Core, Serilog, MVC pipeline)
- [x] Authentication (Login / Logout / Access Denied) & base layout (sidebar + topbar)
- [x] Category Management (CRUD, soft delete/restore, AJAX + DataTables)
- [x] Manufacturer Management (CRUD, soft delete/restore, AJAX + DataTables)
- [x] Medicine Management — core catalog + photo upload + auto-generated barcode & QR code
- [x] Supplier Management (CRUD, soft delete/restore, AJAX + DataTables)
- [x] Purchase Management (Part 1: Pending → Received → Cancelled workflow; Batches created on Receive)
- [x] Purchase Return (Part 2: reduces Batch.QuantityRemaining, immutable audit record)
- [x] Inventory & Batch Management (stock summary, FEFO batch drill-down, manual adjustments)
- [x] Expiry Management (Expired / Near Expiry / All tabs, write-off action)
- [x] Dashboard with statistics (stock, expiry, inventory value, purchase spend & category charts)
- [x] Customer Management (CRUD, soft delete/restore, AJAX + DataTables)
- [x] Sales (POS) — Part 1: cart-based checkout, FEFO multi-batch stock deduction, prescription verification
- [x] Sales — Part 2: printable receipts, Sales History list, Sale Details view
- [x] Sales Return (mirrors Purchase Return: restocks originating batch, tracks cumulative returns per line)
- [x] Reports (PDF via QuestPDF, Excel via ClosedXML — Sales, Purchases, Inventory, Expiry)
- [x] Notifications (bell dropdown: low stock, expiry, pending purchase alerts; lazy sync, no background job)
- [x] Audit Logs (permanent record: login/logout, stock adjustments, purchase/sale/return lifecycle events)
- [x] User Management + Role Management (Administrator creates/edits/deactivates staff, assigns one of the four fixed roles, resets passwords)
- [x] System Settings (pharmacy contact info + stock thresholds, database-backed, drives Inventory/Expiry/Dashboard/Notifications)

**All 20 modules from the original spec are now complete.**

## Technology Stack

**Backend:** ASP.NET Core 9 MVC, C#, Entity Framework Core (Code First), LINQ, ASP.NET Core Identity
**Database:** SQL Server Express
**Frontend:** Razor Views, Bootstrap 5, HTML5/CSS3/JS, jQuery, AJAX, DataTables, Chart.js, SweetAlert2, Font Awesome
**Reporting:** QuestPDF, ClosedXML
**Logging:** Serilog (console + rolling file sinks)

## Folder Structure

```text
Controllers/
Models/
  Identity/
ViewModels/
Views/
Data/
Repositories/
Services/
Interfaces/
Helpers/
Middleware/
Filters/
Extensions/
Migrations/
wwwroot/
  css/
  js/
  images/
  uploads/
  vendor/
Program.cs
appsettings.json
```

## Database Summary

- **Provider:** SQL Server Express, EF Core Code First.
- **Current tables:** Identity tables (`Users`, `Roles`, `UserRoles`, `UserClaims`, `UserLogins`, `RoleClaims`, `UserTokens`) plus `Categories`, `Manufacturers`, `Medicines`, `Suppliers`, `Purchases`, `PurchaseItems`, `Batches`, `PurchaseReturns`, `PurchaseReturnItems`, `Customers`, `Sales`, `SaleItems`, `SaleReturns`, `SaleReturnItems`, `Notifications`, `AuditLogs`, and `PharmacySettings`.
- Business tables are added incrementally, one migration per module, so the migration history stays readable.
- `ApplicationUser` extends `IdentityUser` with `FullName`, `ProfileImagePath`, `IsActive`, `CreatedAt`, `LastLoginAt`, and soft-delete fields (`IsDeleted`, `DeletedAt`).
- `Category` and `Manufacturer` both support soft delete (`IsDeleted`, `DeletedAt`); name uniqueness for each is enforced at the application layer among active records only, so a name can be reused after the original record is deleted.
- `Medicine` has required FKs to `Category` and `Manufacturer` (`Restrict` delete behavior), a `UnitOfMeasure` enum, decimal pricing fields, an uploaded product photo path, and an auto-generated, permanent numeric barcode with matching barcode/QR PNG images. Uniqueness is checked on Name + Strength together. Batch-level stock and expiry data live in the upcoming Batch/Inventory tables, not here.
- `Supplier` is the vendor you purchase stock from (distinct from `Manufacturer`, which makes the drug) — soft delete, name uniqueness among active records, plus license number and payment terms for procurement use.
- `Purchase` (no soft delete — it's a financial record) moves through **Pending → Received → Cancelled**. Creating a purchase does *not* add stock; only marking it **Received** generates one `Batch` per `PurchaseItem`, each with its own expiry date, quantity, and cost. Only Pending orders can be cancelled — a Received order needs a Purchase Return.
- `PurchaseReturn` (also no soft delete) can only be created against a **Received** purchase, and only for quantities up to a batch's current `QuantityRemaining`. Creating one immediately reduces the batch's remaining stock — there's no separate approval step.
- `Customer` supports soft delete; unlike Category/Manufacturer/Supplier, name is **not** required to be unique (real customers share names) — phone number is checked for uniqueness instead, only when provided.
- `Sale` (no soft delete — financial record, like Purchase) only exists once checkout fully succeeds. Checkout validates stock and prescription requirements in a first pass, then deducts stock from batches using **FEFO** (oldest-expiring, non-expired batches first) in a second pass — a single cart line can produce multiple `SaleItem` rows if it has to draw from more than one batch to fulfill the quantity. Receipts (`/Sales/Receipt/{id}`) and Sales History are read-only views over this same data — no new tables.
- `SaleReturn` (also no soft delete) restocks the exact `Batch` each returned line was originally sold from, and tracks cumulative returns per `SaleItem` so the same units can't be returned twice across multiple visits.

### Reports

No new tables — `ReportService` reuses `ISalesService`, `IPurchaseRepository`, `IInventoryService`, and `IExpiryService` and renders the results as PDF (`QuestPDF`) or Excel (`ClosedXML`). Sales and Purchase reports take a date range; Inventory and Expiry are point-in-time snapshots. Restricted to Administrator/StoreManager.

### Notifications

`Notification` rows are synced lazily — there's no background job/scheduler in this stack. Every time the bell dropdown is opened, `NotificationService.SyncAsync()` re-checks current low-stock, expired, near-expiry, and pending-purchase conditions (reusing the same services as Inventory/Expiry/Dashboard), creates a row for any new condition (keyed by a stable `ReferenceKey` like `LowStock-42` so nothing duplicates), and deletes rows whose condition has since resolved. Notifications are broadcast to all staff, not scoped per user.

### Audit Logs

`AuditLog` is a permanent, append-only record (`AuditLogService.LogAsync`) — Administrator-only. Wired into: login success/failure, logout, stock adjustments (covers both Inventory manual adjustments and Expiry write-offs, since both share `AdjustBatchQuantityAsync`), and the full Purchase/Sale/Return lifecycle (created, received, cancelled, returned). It intentionally does **not** cover every CRUD action across every module (e.g. renaming a Category) — that would be high-volume, low-value noise. `UserName` is stored as a denormalized snapshot at the time of the action, so the trail stays readable even if a user account is later renamed.

### User Management + Role Management

Administrator-only. Uses `UserManager<ApplicationUser>` directly (Identity's own abstraction) for create/update/role-assignment/password-reset rather than a repository wrapper. "Role Management" here means assigning one of the four existing roles to a user — not creating new roles or granular permissions, since those four roles are structural to every `[Authorize(Roles=...)]` check across the app. An Administrator cannot deactivate or delete their own account (guards against accidental lockout); there's no "prevent removing the last Administrator" check, a deliberate scope line for this project. Every action here — create, edit, activate/deactivate, delete/restore, password reset — writes to Audit Logs.

### System Settings

The final module — and it genuinely drives behavior rather than being cosmetic. `PharmacySettings` is a single-row, database-backed table for business settings (pharmacy name/address/phone/email, low-stock threshold, near-expiry threshold, currency symbol). This deliberately does **not** live in `appsettings.json`, which stays reserved for environment/infrastructure config (connection strings, Serilog, Identity password policy) that shouldn't be rewritten by a running app. On first access, `SettingsService` seeds the row from the `appsettings.json` defaults that Inventory/Dashboard have used since Steps 8 and 10 — from then on, the database row is the single source of truth. `InventoryService`, `DashboardService`, `NotificationService`, and `ExpiryController` were all updated in this step to read thresholds from `ISettingsService` instead of `IConfiguration`. The pharmacy name/address/phone also now appear on the printed Sales Receipt.

### Inventory

The Inventory module adds no new tables — it's a read/aggregation layer over `Medicine` + `Batch`, computing per-medicine total stock, low-stock flags, and near-expiry/expired flags live on every request, using thresholds from `ISettingsService` (see System Settings below). Manual stock adjustments update `Batch.QuantityRemaining` directly and are logged both to Serilog and to the permanent `AuditLog` table (see Audit Logs below).

### Expiry Management

Also table-free — it's the same `Batch` data as Inventory, but scoped across *all* medicines instead of one at a time, with **Expired / Near Expiry / All** tabs. "Write Off" on an expired batch reuses Inventory's stock-adjustment mechanism (zeroing `QuantityRemaining` with a fixed reason), so there's exactly one code path that changes batch quantities.

### Dashboard

Also no new tables — `DashboardService` composes `IInventoryService`, `IExpiryService`, and `IPurchaseRepository` rather than duplicating their queries. Shows active medicine count, low-stock/expired/near-expiry counts, total inventory value (`Σ QuantityRemaining × UnitCost` across all batches), pending purchase orders, a 6-month purchase-spend bar chart, a stock-by-category doughnut chart, and the 5 most recent purchases. It does not yet show a sales revenue chart — that's a reasonable next addition now that Sales data exists (see Future Improvements).

### Barcode & QR generation approach

Barcode/QR images are generated server-side using `ZXing.Net` (Code128 pixel data) and `QRCoder` (QR PNG), with a small custom PNG encoder (`Helpers/PngEncoder.cs`) turning ZXing's raw pixel matrix into a real `.png` file — this avoids needing `System.Drawing.Common` or an extra rendering package. A barcode is generated once, automatically, right after a medicine's first save, and is never regenerated afterward.

## Architecture

Every module follows the same layered pattern:

```
Controller  →  Service (business rules, validation)  →  Repository (data access)  →  ApplicationDbContext
```

A generic `IGenericRepository<T>` / `GenericRepository<T>` handles common CRUD so each module's repository only adds what's specific to it (e.g. `ICategoryRepository.NameExistsAsync`). Controllers return small JSON payloads consumed by DataTables (client-side) and SweetAlert2 on the front end, keeping the UI layer thin.

## User Roles

| Role | Description |
|---|---|
| Administrator | Full system access |
| Pharmacist | Medicine, inventory, and expiry management |
| Cashier | Point-of-sale operations |
| Store Manager | Inventory, purchases, and supplier oversight |

## Installation Guide

### Prerequisites

- Visual Studio 2022 Community (with ASP.NET and web development workload)
- SQL Server Express
- SQL Server Management Studio (SSMS)
- .NET 9 SDK

### Local Setup Guide

1. Clone the repository:
   ```
   git clone https://github.com/<your-username>/pharmatrack-pro.git
   ```
2. Open `PharmaTrackPro.csproj` in Visual Studio 2022.
3. Restore NuGet packages (Visual Studio does this automatically on build).
4. Update the connection string in `appsettings.json` if your SQL Server instance name differs from `.\SQLEXPRESS`.
5. Open the Package Manager Console and run:
   ```
   Update-Database
   ```
6. Set `PharmaTrackPro` as the startup project and run (F5).

### Running the Application

On first run, the app automatically seeds:
- Default roles: `Administrator`, `Pharmacist`, `Cashier`, `StoreManager`
- Default administrator account (see `appsettings.json` → `AppSettings:DefaultAdminEmail` / `DefaultAdminPassword` — **change this before any real deployment**)

Log in with the seeded administrator credentials at `/Account/Login`. There is intentionally no public self-registration page — staff accounts are created by an Administrator via **Users & Roles** (Administration section in the sidebar), matching how a real pharmacy back office works.

## Screenshots

_Not included here — add your own once you've run the app locally and populated it with sample data._

## Project Modules

See the **Features** checklist above — this list is updated as each module is completed.

## Future Improvements

Ideas beyond the original project spec, in rough priority order:

- **Sales revenue chart on the Dashboard** — now that Sales data exists, a revenue-over-time chart alongside the existing purchase-spend chart would round out the picture.
- **Email/SMS notifications** — the in-app notification bell (Step 15) covers real-time alerts; email digests for low-stock/near-expiry would help owners who aren't logged in daily.
- **Granular custom roles/permissions** — the current RBAC uses four fixed roles baked into `[Authorize(Roles=...)]` throughout the app; a permissions-table-driven system would allow custom roles without code changes.
- **"Prevent removing the last Administrator" guard** — User Management currently only prevents self-deactivation/self-deletion, not accidentally leaving the system with zero admins.
- Multi-branch / multi-pharmacy support
- REST API layer for a future mobile companion app

## License

MIT License — free to use, modify, and distribute for personal or portfolio purposes.
