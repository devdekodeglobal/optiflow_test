namespace DEKODE.OptiFlow.Domain.Entities.Enums;

public static class StoreCategoryExtensions
{
    extension(StoreCategoryTier tier)
    {
        public string ToTierCode() => tier switch
        {
            StoreCategoryTier.APlusPlus => "A++",
            StoreCategoryTier.APlus => "A+",
            StoreCategoryTier.A => "A",
            StoreCategoryTier.BPlus => "B+",
            StoreCategoryTier.B => "B",
            StoreCategoryTier.C => "C",
            _ => "B"
        };
    }

    public static StoreCategoryTier ParseTier(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return StoreCategoryTier.B;

        var clean = value.Trim().ToUpperInvariant();
        if (clean.StartsWith("A++")) return StoreCategoryTier.APlusPlus;
        if (clean.StartsWith("A+")) return StoreCategoryTier.APlus;
        if (clean.StartsWith("A")) return StoreCategoryTier.A;
        if (clean.StartsWith("B+")) return StoreCategoryTier.BPlus;
        if (clean.StartsWith("B")) return StoreCategoryTier.B;
        if (clean.StartsWith("C")) return StoreCategoryTier.C;

        return StoreCategoryTier.B;
    }
}
