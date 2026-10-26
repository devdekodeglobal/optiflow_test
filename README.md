# OptiFlow Allocation Engine (.NET 10)

This directory contains the pure Domain-Driven Design (DDD) business logic, merchandising guardrails, and allocation waterfall calculation engine ported to **C# .NET 10**.

## Solution Structure

```
OptiFlow.slnx
├── Business/
│   ├── DEKODE.OptiFlow.Domain.Entities/        # Domain entities, records, enums, execution context
│   ├── DEKODE.OptiFlow.Domain.Interfaces/      # Contracts for parser, scorer, engine, and service
│   └── DEKODE.OptiFlow.Domain.BusinessServices/# Core business logic and Template Method Pattern implementation
└── Tests/
    └── DEKODE.OptiFlow.UnitTests/              # Comprehensive xUnit & FluentAssertions test suites
```

## Standards & Principles
- **SOLID / DRY / KISS / YAGNI**: Modular design, decoupled responsibility, extensible steps.
- **Template Method Pattern**: `AllocationEngineBase` defines invariant execution lifecycle.
- **Merchandising Guardrails**:
  - Max 2 units / SKU / store
  - Max 3 colors / model / store (strict block on 4th color)
  - 85% Assortment Breadth enforcement
  - Price band tolerance (±20% reference MRP)
  - FIFO stock age drain (oldest batches first with barcode tracking)

## Running Tests
```bash
dotnet test OptiFlow.slnx
```
