using FluentValidation;

namespace MyStore.Application.Orders.Commands;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(v => v.CustomerName)
            .NotEmpty().WithMessage("CustomerName is required")
            .MaximumLength(100).WithMessage("CustomerName is too long");

        RuleFor(v => v.ProductName)
            .NotEmpty().WithMessage("ProductName is required");

        RuleFor(v => v.Price)
            .GreaterThan(0).WithMessage("Price should be greater than 0");
    }
}
