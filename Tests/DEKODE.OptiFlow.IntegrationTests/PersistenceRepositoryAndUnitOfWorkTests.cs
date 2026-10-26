using DEKODE.OptiFlow.DAL.Repository;
using DEKODE.OptiFlow.Domain.BusinessServices.Services;
using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Entities.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DEKODE.OptiFlow.IntegrationTests;

public class PersistenceRepositoryAndUnitOfWorkTests
{
    private OptiFlowDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<OptiFlowDbContext>()
            .UseInMemoryDatabase(databaseName: $"OptiFlow_TestDb_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new OptiFlowDbContext(options);
    }

    [Fact]
    public async Task UnitOfWork_AddPlanogramWithLines_PersistsAndLoadsCorrectly()
    {
        await using var context = CreateInMemoryDbContext();
        await using var uow = new UnitOfWork(context);

        var planogram = new Planogram
        {
            Name = "Q4_Planogram",
            SeasonOrMonth = "Oct'26",
            EffectiveDate = DateTime.UtcNow,
            IsActive = true
        };

        planogram.AddLine(new PlanogramLine
        {
            StoreName = "Mumbai Store 1",
            BrandName = "Rayban",
            Commodity = CommodityType.Frame,
            Facing = 10,
            BackStock = 5,
            StoreCategory = "A++",
            CategoryTier = StoreCategoryTier.APlusPlus
        });

        // Act
        await uow.Planograms.AddAsync(planogram);
        var affected = await uow.SaveChangesAsync();

        // Assert
        affected.Should().BeGreaterThan(0);

        var retrieved = await uow.Planograms.GetWithLinesByIdAsync(planogram.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Q4_Planogram");
        retrieved.Lines.Should().HaveCount(1);
        retrieved.Lines[0].StoreName.Should().Be("Mumbai Store 1");
        retrieved.Lines[0].TargetCapacity.Should().Be(15);
    }

    [Fact]
    public async Task UnitOfWork_TransactionExecution_CommitsAtomicChanges()
    {
        await using var context = CreateInMemoryDbContext();
        await using var uow = new UnitOfWork(context);

        // Act: Execute inside transactional boundary
        var result = await uow.ExecuteInTransactionAsync(async () =>
        {
            var p = new Planogram { Name = "Tx_Planogram" };
            p.AddLine(new PlanogramLine { StoreName = "Delhi Store", BrandName = "Oakley", Facing = 5, BackStock = 2 });

            await uow.Planograms.AddAsync(p);
            await uow.SaveChangesAsync();
            return p.Id;
        });

        // Assert
        result.Should().BeGreaterThan(0);
        var saved = await uow.Planograms.GetByIdAsync(result);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Tx_Planogram");
    }

    [Fact]
    public async Task UnitOfWork_ImportRealPlanogramAndPersist_SavesAllLinesIntegrally()
    {
        await using var context = CreateInMemoryDbContext();
        await using var uow = new UnitOfWork(context);
        var importer = new PlanogramCsvImporter();

        // Act: Import from actual seed Planogram.csv
        var filePath = "/Users/vanosski/Main/Projects/Dekode/cfs_test/optiflow_main/backend/seed_data/Planogram.csv";
        var planogram = await importer.ImportFromCsvFileAsync(filePath, "Persisted_Real_Planogram");

        await uow.Planograms.AddAsync(planogram);
        await uow.SaveChangesAsync();

        // Query repository for lines
        var linesForAgra = await uow.Planograms.GetLinesByStoreAsync("Agra Ashoka Plaza");
        linesForAgra.Should().NotBeEmpty();
        linesForAgra.Should().Contain(l => l.BrandName == "Rayban" && l.Commodity == CommodityType.Frame);
    }
}
