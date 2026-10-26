using DEKODE.OptiFlow.Domain.BusinessServices.Services;
using FluentAssertions;
using Xunit;

namespace DEKODE.OptiFlow.UnitTests;

public class AttributeParserAndSimilarityTests
{
    private readonly ItemAttributeParser _parser = new();
    private readonly WeightedAttributeSimilarityScorer _scorer = new();

    [Fact]
    public void Parse_ValidItemName_ExtractsAllAttributesCorrectly()
    {
        // Arrange
        var rawName = "RX6303I - Rayban - Male - 2502 - 53 - Rimless - Rectangle - Metal";

        // Act
        var attrs = _parser.Parse(rawName);

        // Assert
        attrs.Model.Should().Be("RX6303I");
        attrs.Brand.Should().Be("Rayban");
        attrs.Gender.Should().Be("Male");
        attrs.Color.Should().Be("2502");
        attrs.Size.Should().Be("53");
        attrs.FrameType.Should().Be("Rimless");
        attrs.Shape.Should().Be("Rectangle");
        attrs.Material.Should().Be("Metal");
    }

    [Fact]
    public void Parse_ShortOrIncompleteName_ToleratesMissingSegmentsGracefully()
    {
        // Arrange
        var shortName = "RB3025 - Rayban - Unisex";

        // Act
        var attrs = _parser.Parse(shortName);

        // Assert
        attrs.Model.Should().Be("RB3025");
        attrs.Brand.Should().Be("Rayban");
        attrs.Gender.Should().Be("Unisex");
        attrs.Color.Should().BeEmpty();
        attrs.Shape.Should().BeEmpty();
        attrs.Material.Should().BeEmpty();
    }

    [Fact]
    public void Parse_NullOrEmptyString_ReturnsEmptyDefaultAttributes()
    {
        _parser.Parse(null).Model.Should().BeEmpty();
        _parser.Parse("").Model.Should().BeEmpty();
        _parser.Parse("   ").Model.Should().BeEmpty();
    }

    [Fact]
    public void CalculateSimilarity_IdenticalAttributes_YieldsMaxScore100()
    {
        // Arrange
        var target = _parser.Parse("M1 - Rayban - Male - Red - 53 - Full Frame - Rectangle - Metal");
        var candidate = _parser.Parse("M2 - Rayban - Male - Red - 54 - Full Frame - Rectangle - Metal");

        // Act
        var score = _scorer.CalculateSimilarity(target, candidate);

        // Assert
        // Shape(30) + Material(25) + FrameType(20) + Color(15) + Gender(10) = 100
        score.Should().Be(100.0);
    }

    [Fact]
    public void CalculateSimilarity_CompletelyDifferentAttributes_YieldsZero()
    {
        // Arrange
        var target = _parser.Parse("M1 - Rayban - Male - Red - 53 - Full Frame - Rectangle - Metal");
        var candidate = _parser.Parse("M2 - Rayban - Female - Blue - 54 - Rimless - Round - Sheet");

        // Act
        var score = _scorer.CalculateSimilarity(target, candidate);

        // Assert
        score.Should().Be(0.0);
    }

    [Fact]
    public void CalculateSimilarity_PartialMatch_CalculatesCorrectWeights()
    {
        // Arrange
        var target = _parser.Parse("M1 - Rayban - Male - Red - 53 - Full Frame - Rectangle - Metal");
        // Matches Shape(30) and Material(25) only = 55
        var candidate = _parser.Parse("M2 - Rayban - Female - Blue - 54 - Rimless - Rectangle - Metal");

        // Act
        var score = _scorer.CalculateSimilarity(target, candidate);

        // Assert
        score.Should().Be(55.0);
    }
}
