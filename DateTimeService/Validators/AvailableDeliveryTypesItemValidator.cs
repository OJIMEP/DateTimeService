using DateTimeService.Models.AvailableDeliveryTypes;
using FluentValidation;

namespace DateTimeService.Validators
{
    public class AvailableDeliveryTypesItemValidator : AbstractValidator<RequestAvailableDeliveryTypesItemDTO>
    {
        public AvailableDeliveryTypesItemValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Количество товара не может быть 0");

            RuleFor(x => x.SalesCode)
                .Must( salesCode => salesCode == null || salesCode.Trim() != "" )
                .WithMessage("Поле уценки не должно быть пустой строкой - либо заполнено, либо поле в принципе отсутствует");
        }
    }
}
