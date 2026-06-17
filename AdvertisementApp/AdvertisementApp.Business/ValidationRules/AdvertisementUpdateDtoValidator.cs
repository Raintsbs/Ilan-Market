using AdvertisementApp.Dtos.AdvertisementDtos;
using FluentValidation;

namespace AdvertisementApp.Business.ValidationRules
{
    public class AdvertisementUpdateDtoValidator : AbstractValidator<AdvertisementUpdateDto>
    {
        public AdvertisementUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("İlan ID boş olamaz.");
            RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Kategori ID boş olamaz.");
            
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Başlık alanı boş geçilemez.")
                .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");
            
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama alanı boş geçilemez.")
                .MaximumLength(500).WithMessage("Kısa açıklama en fazla 500 karakter olabilir.");
                
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("İçerik alanı boş geçilemez.");
        }
    }
}
