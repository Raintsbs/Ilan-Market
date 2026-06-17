using AdvertisementApp.Dtos.CategoryDtos;
using FluentValidation;

namespace AdvertisementApp.Business.ValidationRules
{
    public class CategoryUpdateDtoValidator : AbstractValidator<CategoryUpdateDto>
    {
        public CategoryUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Kategori ID boş olamaz.");
            
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Kategori adı boş geçilemez.")
                .MaximumLength(100).WithMessage("Kategori adı en fazla 100 karakter olabilir.");
        }
    }
}
