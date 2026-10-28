using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DEKODE.OptiFlow.DTO.Allocation;
using DEKODE.OptiFlow.DTO.Auth;
using DEKODE.OptiFlow.DTO.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using DEKODE.OptiFlow.DAL.Repository;
using Xunit;

namespace DEKODE.OptiFlow.IntegrationTests;

public class WebApiEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WebApiEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext descriptor
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<OptiFlowDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<OptiFlowDbContext>(options =>
                {
                    options.UseInMemoryDatabase("OptiFlow_Test_InMemoryDb")
                           .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
            });
        }).CreateClient();
    }

    private async Task<string> AuthenticateAsync()
    {
        var loginRequest = new LoginRequestDto
        {
            Username = "admin",
            Password = "optiflow2026"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        return body.Data!.Token;
    }

    [Fact]
    public async Task CalculateAllocation_WithoutJwt_ReturnsUnauthorized()
    {
        var request = new AllocationExecutionRequestDto
        {
            PlanogramLines = new List<PlanogramLineInputDto>
            {
                new() { StoreName = "Store 1", BrandName = "Rayban", Facing = 2, BackStock = 1 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/allocation/calculate", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CalculateAllocation_WithJwt_ExecutesPureCalculationSuccessfully()
    {
        // 1. Authenticate with JWT
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. Prepare payload (nothing fetched directly from DB)
        var request = new AllocationExecutionRequestDto
        {
            RunBy = "IntegrationTest_User",
            SalesLookbackDays = 30,
            ActiveCategories = new List<string> { "A" },
            StrategyStoreLists = new Dictionary<string, List<string>>
            {
                ["A"] = new List<string> { "Store Alpha" }
            },
            PlanogramLines = new List<PlanogramLineInputDto>
            {
                new() { StoreName = "Store Alpha", BrandName = "Rayban", Commodity = "Frame", Facing = 1, BackStock = 1, StoreCategory = "A" }
            },
            WarehouseBatches = new List<WarehouseStockBatchInputDto>
            {
                new() { ItemCode = "SKU_TOP", ItemName = "M_TOP - Rayban - Male - Red - 53 - Full Frame - Rectangle - Metal", BatchStock = 10, Mrp = 1000m, Barcode = "BC_TOP", StockAgeInDays = 5 }
            },
            SalesRecords = new List<SalesRecordInputDto>
            {
                new() { FacilityName = "Store Alpha", ItemCode = "SKU_TOP", Quantity = 5, OrderDate = DateTime.UtcNow.AddDays(-5) }
            }
        };

        // 3. Post to pure calculate endpoint
        var response = await _client.PostAsJsonAsync("/api/v1/allocation/calculate", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AllocationResultDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsPersisted.Should().BeFalse();
        result.Data.Summary.Should().NotBeNull();
        result.Data.Summary!.TotalItemsAllocated.Should().BeGreaterThanOrEqualTo(1);
        result.Data.Lines.Should().Contain(l => l.AllocatedItemCode == "SKU_TOP" && l.MatchType == "Exact");
    }

    [Fact]
    public async Task RunAndPersistAllocation_WithJwt_ExecutesAndPersistsSuccessfully()
    {
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new AllocationExecutionRequestDto
        {
            RunBy = "PersistTest_User",
            ActiveCategories = new List<string> { "A" },
            StrategyStoreLists = new Dictionary<string, List<string>>
            {
                ["A"] = new List<string> { "Store Persist" }
            },
            PlanogramLines = new List<PlanogramLineInputDto>
            {
                new() { StoreName = "Store Persist", BrandName = "Rayban", Commodity = "Frame", Facing = 2, BackStock = 0, StoreCategory = "A" }
            },
            WarehouseBatches = new List<WarehouseStockBatchInputDto>
            {
                new() { ItemCode = "SKU_P", ItemName = "MP - Rayban - Male - Black - 54 - Full Frame - Round - Sheet", BatchStock = 5, Mrp = 1200m, Barcode = "BC_P", StockAgeInDays = 10 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/allocation/run", request);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"RunAndPersist failed with status {response.StatusCode}: {err}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AllocationResultDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Id.Should().NotBeNull();
        result.Data.IsPersisted.Should().BeTrue();

        // 4. Retrieve back by ID
        Guid allocationId = result.Data.Id!.Value;
        var getResponse = await _client.GetAsync($"/api/v1/allocation/{allocationId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto
        {
            Username = "admin",
            Password = "wrong_password_123"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CalculateAllocation_WithInvalidPayload_FailsValidation()
    {
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Empty planogram lines violates FluentValidation rule
        var emptyRequest = new AllocationExecutionRequestDto();

        var response = await _client.PostAsJsonAsync("/api/v1/allocation/calculate", emptyRequest);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
