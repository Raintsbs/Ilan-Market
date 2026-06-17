using AdvertisementApp.Business.ValidationRules;
using AdvertisementApp.Dtos.CategoryDtos;

namespace AdvertisementApp.Tests.Validation
{
    public class CategoryCreateDtoValidatorTests
    {
        private readonly CategoryCreateDtoValidator _validator = new();

        [Fact]
        public async Task Validate_EmptyName_ShouldFail()
        {
            var dto = new CategoryCreateDto { Name = "" };
            var result = await _validator.ValidateAsync(dto);
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task Validate_ValidName_ShouldPass()
        {
            var dto = new CategoryCreateDto { Name = "Elektronik", Description = "Test" };
            var result = await _validator.ValidateAsync(dto);
            Assert.True(result.IsValid);
        }
    }
}
