using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Entities.Enums;
using DEKODE.OptiFlow.Domain.Entities.Exceptions;
using DEKODE.OptiFlow.Domain.Interfaces;
using DEKODE.OptiFlow.Domain.Interfaces.Parsing;
using System.Globalization;

namespace DEKODE.OptiFlow.Domain.BusinessServices.Services;

/// <summary>
/// Parser utility for raw merchandising CSVs (e.g. Planogram.csv).
/// Implements IPlanogramCsvParser for pure in-memory parsing and IPlanogramImporter for backward compatibility.
/// </summary>
public class PlanogramCsvImporter : IPlanogramImporter, IPlanogramCsvParser
{
    public Task<Planogram> ParseFromCsvFileAsync(string filePath, string planogramName, CancellationToken cancellationToken = default) =>
        ImportFromCsvFileAsync(filePath, planogramName, cancellationToken);

    public Task<Planogram> ParseFromCsvStreamAsync(Stream csvStream, string planogramName, CancellationToken cancellationToken = default) =>
        ImportFromCsvStreamAsync(csvStream, planogramName, cancellationToken);

    public async Task<Planogram> ImportFromCsvFileAsync(string filePath, string planogramName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Planogram CSV file not found: {filePath}");

        await using var stream = File.OpenRead(filePath);
        return await ImportFromCsvStreamAsync(stream, planogramName, cancellationToken);
    }

    public async Task<Planogram> ImportFromCsvStreamAsync(Stream csvStream, string planogramName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(csvStream);

        if (string.IsNullOrWhiteSpace(planogramName))
            planogramName = $"Planogram_{DateTime.UtcNow:yyyyMMdd_HHmmss}";

        var planogram = new Planogram
        {
            Name = planogramName,
            EffectiveDate = DateTime.UtcNow,
            IsActive = true
        };

        using var reader = new StreamReader(csvStream);
        var rawLines = new List<string>();

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                rawLines.Add(line);
            }
        }

        if (rawLines.Count == 0)
        {
            throw new PlanogramValidationException("Supplied planogram CSV stream is completely empty.");
        }

        // Find header row: Look for row containing "Store Name" or "Store-Brand"
        int headerRowIndex = -1;
        string[] headers = Array.Empty<string>();

        for (int i = 0; i < Math.Min(10, rawLines.Count); i++)
        {
            var parsedCols = ParseCsvLine(rawLines[i]);
            var normalized = parsedCols.Select(NormalizeHeader).ToArray();

            if (normalized.Contains("store_name") || normalized.Contains("store_brand_product_type"))
            {
                headerRowIndex = i;
                headers = normalized;
                break;
            }
        }

        if (headerRowIndex == -1 || headers.Length == 0)
        {
            throw new PlanogramValidationException("Could not detect valid Planogram header row in CSV (expected 'Store Name' column).");
        }

        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int c = 0; c < headers.Length; c++)
        {
            if (!string.IsNullOrEmpty(headers[c]) && !colMap.ContainsKey(headers[c]))
            {
                colMap[headers[c]] = c;
            }
        }

        int storeNameIdx = GetColIndex(colMap, "store_name");
        int brandNameIdx = GetColIndex(colMap, "brand_name");
        int brandCodeIdx = GetColIndex(colMap, "brand_code");
        int productTypeIdx = GetColIndex(colMap, "product_type", "commodity");
        int dispatchPlanIdx = GetColIndex(colMap, "dispatch_plan", "store_category");
        int storeTypeIdx = GetColIndex(colMap, "store_type");
        int brandTypeIdx = GetColIndex(colMap, "brand_type");
        int brandCatIdx = GetColIndex(colMap, "brand_category");
        int supplierNameIdx = GetColIndex(colMap, "supplier_name");
        int storeBrandProductTypeIdx = GetColIndex(colMap, "store_brand_product_type");
        int startMonthIdx = GetColIndex(colMap, "start_month");

        int facingDepthIdx = GetColIndex(colMap, "facing+depth", "facing_depth");
        int storeDisplayIdx = GetColIndex(colMap, "store_display", "facing");
        int sohIdx = GetColIndex(colMap, "soh", "soh_total");
        int skuCountIdx = GetColIndex(colMap, "sku_count", "sku_count_total");
        int saleQty6MIdx = GetColIndex(colMap, "sale_qty__(6m)", "sale_qty_6m", "sale_qty_(6m)");
        int lmSaleQtyIdx = GetColIndex(colMap, "lm_sale_qty");
        int uniquenessIdx = GetColIndex(colMap, "uniqueness_(%)", "uniqueness");
        int avgMonthlySaleIdx = GetColIndex(colMap, "avg_monthly_sale");

        // Parse data rows
        for (int r = headerRowIndex + 1; r < rawLines.Count; r++)
        {
            var rowCols = ParseCsvLine(rawLines[r]);
            if (rowCols.Length == 0 || rowCols.All(string.IsNullOrWhiteSpace))
                continue;

            try
            {
                string storeName = GetValue(rowCols, storeNameIdx);
                string brandName = GetValue(rowCols, brandNameIdx);

                if (string.IsNullOrWhiteSpace(storeName) && string.IsNullOrWhiteSpace(brandName))
                    continue; // Skip trailing empty summary rows

                string rawCommodity = GetValue(rowCols, productTypeIdx);
                string storeBrandKey = GetValue(rowCols, storeBrandProductTypeIdx);
                CommodityType commodity = ResolveCommodity(rawCommodity, storeBrandKey);

                string rawDispatch = GetValue(rowCols, dispatchPlanIdx);
                var categoryTier = StoreCategoryExtensions.ParseTier(rawDispatch);
                var dispatchPlan = ParseDispatchPlan(rawDispatch);
                var storeType = ParseStoreType(GetValue(rowCols, storeTypeIdx));

                double storeDisplay = ParseDouble(GetValue(rowCols, storeDisplayIdx));
                double totalCapacity = ParseDouble(GetValue(rowCols, facingDepthIdx));

                double facing = storeDisplay > 0 ? storeDisplay : (totalCapacity > 0 ? totalCapacity : 0);
                double backStock = Math.Max(0.0, totalCapacity - facing);
                double soh = ParseDouble(GetValue(rowCols, sohIdx));

                if (string.IsNullOrEmpty(planogram.SeasonOrMonth) && startMonthIdx >= 0)
                {
                    planogram.SeasonOrMonth = GetValue(rowCols, startMonthIdx);
                }

                var planogramLine = new PlanogramLine
                {
                    StoreBrandProductTypeKey = storeBrandKey,
                    StoreName = storeName,
                    BrandName = brandName,
                    BrandCode = GetValue(rowCols, brandCodeIdx),
                    BrandType = GetValue(rowCols, brandTypeIdx),
                    BrandCategory = GetValue(rowCols, brandCatIdx),
                    SupplierName = GetValue(rowCols, supplierNameIdx),
                    Commodity = commodity,
                    StoreCategory = categoryTier.ToTierCode(),
                    CategoryTier = categoryTier,
                    StoreType = storeType,
                    DispatchPlan = dispatchPlan,
                    Facing = facing,
                    BackStock = backStock,
                    StoreDisplay = storeDisplay,
                    Soh = soh,
                    SkuCount = ParseNullableInt(GetValue(rowCols, skuCountIdx)),
                    SaleQty6M = ParseNullableDouble(GetValue(rowCols, saleQty6MIdx)),
                    LastMonthSaleQty = ParseNullableDouble(GetValue(rowCols, lmSaleQtyIdx)),
                    UniquenessRatio = ParseNullableDouble(GetValue(rowCols, uniquenessIdx)),
                    AvgMonthlySale = ParseNullableDouble(GetValue(rowCols, avgMonthlySaleIdx))
                };

                planogram.AddLine(planogramLine);
            }
            catch (Exception ex) when (ex is not PlanogramImportException)
            {
                throw new PlanogramImportException($"Failed to import row: {ex.Message}", r + 1, ex);
            }
        }

        return planogram;
    }

    private static CommodityType ResolveCommodity(string productType, string key)
    {
        var combined = $"{productType} {key}".ToLowerInvariant();
        if (combined.Contains("sunglass") || combined.Contains("goggles"))
        {
            return CommodityType.Sunglass;
        }
        return CommodityType.Frame;
    }

    private static StoreType ParseStoreType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return StoreType.SIS;
        var clean = value.Trim().ToUpperInvariant();
        if (clean.Contains("SIS")) return StoreType.SIS;
        if (clean.Contains("EBO")) return StoreType.EBO;
        if (clean.Contains("OPTICAL")) return StoreType.Optical;
        return StoreType.Other;
    }

    private static DispatchPlan ParseDispatchPlan(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return DispatchPlan.Weekly;
        var clean = value.Trim().ToUpperInvariant();
        if (clean.Contains("WEEKLY") && !clean.Contains("BI")) return DispatchPlan.Weekly;
        if (clean.Contains("BIWEEKLY") || clean.Contains("BI-WEEKLY")) return DispatchPlan.BiWeekly;
        if (clean.Contains("MONTHLY")) return DispatchPlan.Monthly;
        return DispatchPlan.Custom;
    }

    private static int GetColIndex(Dictionary<string, int> colMap, params string[] candidates)
    {
        foreach (var c in candidates)
        {
            if (colMap.TryGetValue(c, out int idx))
                return idx;
        }
        return -1;
    }

    private static string GetValue(string[] cols, int idx)
    {
        return idx >= 0 && idx < cols.Length ? cols[idx].Trim() : string.Empty;
    }

    private static double ParseDouble(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0.0;
        return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double val) ? val : 0.0;
    }

    private static double? ParseNullableDouble(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double val) ? val : null;
    }

    private static int? ParseNullableInt(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out int val) ? val : null;
    }

    private static string NormalizeHeader(string header)
    {
        return header.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    current.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
