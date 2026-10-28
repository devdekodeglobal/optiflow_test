using DEKODE.OptiFlow.DTO.Allocation;
using FluentValidation;

namespace DEKODE.OptiFlow.DTO.Validators;

public class PlanogramLineInputDtoValidator : AbstractValidator<PlanogramLineInputDto>
{
    public PlanogramLineInputDtoValidator()
    {
        RuleFor(x => x.StoreName).NotEmpty().WithMessage("Store name is required.");
        RuleFor(x => x.BrandName).NotEmpty().WithMessage("Brand name is required.");
        RuleFor(x => x.Facing).GreaterThanOrEqualTo(0).WithMessage("Facing must be non-negative.");
        RuleFor(x => x.BackStock).GreaterThanOrEqualTo(0).WithMessage("BackStock must be non-negative.");
    }
}

public class WarehouseStockBatchInputDtoValidator : AbstractValidator<WarehouseStockBatchInputDto>
{
    public WarehouseStockBatchInputDtoValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().WithMessage("ItemCode is required.");
        RuleFor(x => x.BatchStock).GreaterThanOrEqualTo(0).WithMessage("BatchStock must be non-negative.");
        RuleFor(x => x.Mrp).GreaterThanOrEqualTo(0).WithMessage("Mrp must be non-negative.");
    }
}

public class AllocationExecutionRequestDtoValidator : AbstractValidator<AllocationExecutionRequestDto>
{
    public AllocationExecutionRequestDtoValidator()
    {
        RuleFor(x => x.PlanogramLines)
            .NotEmpty().WithMessage("Allocation execution request must contain at least one planogram line.");

        RuleForEach(x => x.PlanogramLines).SetValidator(new PlanogramLineInputDtoValidator());
        RuleForEach(x => x.WarehouseBatches).SetValidator(new WarehouseStockBatchInputDtoValidator());
    }
}
