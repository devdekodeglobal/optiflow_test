using DEKODE.OptiFlow.Domain.BusinessServices.Services;
using DEKODE.OptiFlow.Domain.Entities.Enums;
using DEKODE.OptiFlow.Domain.Entities.Exceptions;
using FluentAssertions;
using System.Text;
using Xunit;

namespace DEKODE.OptiFlow.UnitTests;

public class PlanogramCsvImporterTests
{
    private readonly PlanogramCsvImporter _importer = new();

    [Fact]
    public async Task ImportFromCsvStreamAsync_ValidSample_ParsesAllFieldsAndComputesCapacity()
    {
        // Arrange
        var csvContent = new StringBuilder()
            .AppendLine("Store Name,Brand Name,Product Type,Dispatch Plan,Store Type,Brand Code,Store Display,Facing+Depth,SOH,SKU Count")
            .AppendLine("Agra Ashoka Plaza,Admyra,Frame,A-Weekly,SIS,AD,50,56,61,49")
            .AppendLine("Agra Ashoka Plaza,Ombra,Sunglass,A-Weekly,SIS,OB,0,33,32,32")
            .ToString();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var planogram = await _importer.ImportFromCsvStreamAsync(stream, "TestPlanogram_2026");

        // Assert
        planogram.Should().NotBeNull();
        planogram.Name.Should().Be("TestPlanogram_2026");
        planogram.Lines.Should().HaveCount(2);

        var line1 = planogram.Lines[0];
        line1.StoreName.Should().Be("Agra Ashoka Plaza");
        line1.BrandName.Should().Be("Admyra");
        line1.Commodity.Should().Be(CommodityType.Frame);
        line1.CategoryTier.Should().Be(StoreCategoryTier.A);
        line1.DispatchPlan.Should().Be(DispatchPlan.Weekly);
        line1.StoreType.Should().Be(StoreType.SIS);
        line1.Facing.Should().Be(50);
        line1.BackStock.Should().Be(6); // 56 - 50 = 6
        line1.TargetCapacity.Should().Be(56);
        line1.Soh.Should().Be(61);
        line1.SkuCount.Should().Be(49);

        var line2 = planogram.Lines[1];
        line2.BrandName.Should().Be("Ombra");
        line2.Commodity.Should().Be(CommodityType.Sunglass);
        line2.TargetCapacity.Should().Be(33);
    }

    [Fact]
    public async Task ImportFromCsvFileAsync_RealPlanogramCsv_SuccessfullyParsesAllRows()
    {
        // Arrange: Point to the actual real Planogram.csv provided by user
        var filePath = "/Users/vanosski/Main/Projects/Dekode/cfs_test/optiflow_main/backend/seed_data/Planogram.csv";

        // Act
        var planogram = await _importer.ImportFromCsvFileAsync(filePath, "SeedData_Planogram");

        // Assert
        planogram.Should().NotBeNull();
        planogram.Lines.Should().NotBeEmpty();
        planogram.Lines.Count.Should().BeGreaterThan(800);

        // Verify known sample row from seed data
        var raybanRow = planogram.Lines.FirstOrDefault(l => l.StoreName == "Agra Ashoka Plaza" && l.BrandName == "Rayban");
        raybanRow.Should().NotBeNull();
        raybanRow!.Commodity.Should().Be(CommodityType.Frame);
        raybanRow.BrandCode.Should().Be("RX");
        raybanRow.SupplierName.Should().Be("Luxottica");
        raybanRow.TargetCapacity.Should().Be(17);
        raybanRow.Facing.Should().Be(15);
        raybanRow.BackStock.Should().Be(2); // 17 - 15 = 2
    }

    [Fact]
    public async Task ImportFromCsvStreamAsync_EmptyStream_ThrowsPlanogramValidationException()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(""));

        var act = async () => await _importer.ImportFromCsvStreamAsync(stream, "Empty");

        await act.Should().ThrowAsync<PlanogramValidationException>()
            .WithMessage("*completely empty*");
    }

    [Fact]
    public async Task ImportFromCsvStreamAsync_InvalidHeader_ThrowsPlanogramValidationException()
    {
        var invalidCsv = "Col1,Col2,Col3\nVal1,Val2,Val3";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidCsv));

        var act = async () => await _importer.ImportFromCsvStreamAsync(stream, "Invalid");

        await act.Should().ThrowAsync<PlanogramValidationException>()
            .WithMessage("*Could not detect valid Planogram header row*");
    }
}
