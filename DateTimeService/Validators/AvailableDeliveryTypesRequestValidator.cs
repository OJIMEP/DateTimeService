using DateTimeService.Models.AvailableDeliveryTypes;
using FluentValidation;

namespace DateTimeService.Validators
{
    public class AvailableDeliveryTypesRequestValidator: AbstractValidator<RequestAvailableDeliveryTypesDTO>
    {
        public AvailableDeliveryTypesRequestValidator()
        {
            RuleFor(x => x.CityId)
                .NotEmpty()
                .WithMessage("Должен быть указан код города");

            RuleFor(x => x.PickupPoints)
                .NotEmpty()
                .WithMessage("Должен быть указан хоть один пункт самовывоза");

            RuleFor(x => x.OrderItems)
                .NotEmpty()
                .WithMessage("Должен быть указан хоть один товар");

            RuleForEach(x => x.OrderItems)
                .SetValidator(new AvailableDeliveryTypesItemValidator());
        }
    }
}
