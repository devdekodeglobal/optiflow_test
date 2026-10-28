# OptiFlow Allocation Engine (.NET 10)

Enterprise inventory allocation, planogram replenishment, and multi-supplier catalog system built in **.NET 10** following Clean Architecture & Domain-Driven Design (DDD).

---

## 🏗️ Architecture & Solution Structure

```
OptiFlow.slnx
├── Application/
│   ├── DEKODE.OptiFlow.DTO/                    # Request/Response contracts & FluentValidation rules
│   └── DEKODE.OptiFlow.WebAPI/                 # REST Web API (Kestrel, JWT Auth, Swagger & Scalar UI)
├── Business/
│   ├── DEKODE.OptiFlow.Domain.Entities/        # Pure domain models (zero external dependencies)
│   ├── DEKODE.OptiFlow.Domain.Interfaces/      # Service & Repository abstractions
│   └── DEKODE.OptiFlow.Domain.BusinessServices/# Core waterfall allocation engine & planogram service
├── Infrastructure/
│   └── DEKODE.OptiFlow.Infrastructure/         # Composition Root wiring DI, EF Core, & Npgsql
├── Persistence/
│   └── DEKODE.OptiFlow.DAL.Repository/         # Normalized PostgreSQL schema, EF Core migrations, UnitOfWork
└── Tests/
    ├── DEKODE.OptiFlow.UnitTests/              # 18 unit tests (Merchandising rules & allocation logic)
    └── DEKODE.OptiFlow.IntegrationTests/       # 8 integration tests (In-memory WebApplicationFactory)
```

---

## 🗄️ Normalized Relational Database Schema (3NF)

The database schema cleanly eliminates duplicate text records and models real-world retail supply-chain semantics:

* **Facilities**: Distinguishes `CorporateOffice` / Central Warehouse from Retail Stores and SIS locations.
* **Brands & Categories**: Normalized brand taxonomy (`Luxury`, `Intl Brand`, `Private Label`, etc.).
* **Suppliers & ProductSuppliers**: Many-to-many relationship supporting multiple vendors per product/commodity with standard costs, lead times, and primary vendor flags.
* **Products & ProductItems**: Style/Model level master connected to SKU / Barcode level items with MRP, purchase costs, and GST rates.
* **InventoryStocks**: Tracks physical batch stock per facility with exact supplier provenance and batch numbers.
* **SalesRecords & Planograms**: Normalized point-of-sale history and store target capacity lines.

---

## 🚀 Getting Started

### 1. Start Database via Docker (PostgreSQL 16 + pgAdmin)
```bash
docker compose up -d
```
* **PostgreSQL Port**: `5433` (Host) -> `5432` (Container)
* **pgAdmin 4 Web UI**: `http://localhost:5050` (Login: `admin@dekode.com` / `admin`)

### 2. Apply Migrations to Database
```bash
dotnet ef database update --project Persistence/DEKODE.OptiFlow.DAL.Repository --startup-project Persistence/DEKODE.OptiFlow.DAL.Repository
```

### 3. Run the Web API & Swagger UI
```bash
dotnet run --project Application/DEKODE.OptiFlow.WebAPI
```
* **Swagger UI**: [http://localhost:5106/swagger](http://localhost:5106/swagger)
* **Scalar Docs**: [http://localhost:5106/scalar/v1](http://localhost:5106/scalar/v1)

### 4. Run Test Suite
```bash
dotnet test OptiFlow.slnx
```
*(All 26 unit and integration tests execute and pass out of the box).*
