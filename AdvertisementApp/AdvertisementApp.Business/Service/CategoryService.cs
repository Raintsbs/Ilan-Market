using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Result;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Interface;
using AdvertisementApp.Dtos.CategoryDtos;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Entities;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.Business.Service
{
    public class CategoryService : GenericService<Category, CategoryListDto, CategoryCreateDto, CategoryUpdateDto>, ICategoryService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IAppCacheService _cache;

        public CategoryService(
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<CategoryCreateDto> createValidator,
            IValidator<CategoryUpdateDto> updateValidator,
            AdvertisementAppDbContext db,
            IAppCacheService cache)
            : base(uow, mapper, createValidator, updateValidator)
        {
            _db = db;
            _cache = cache;
        }

        public Task<List<CategoryTreeDto>> GetTreeAsync() =>
            _cache.GetOrCreateAsync("categories:tree", TimeSpan.FromMinutes(30), async () =>
            {
                var all = await _db.Categories.AsNoTracking()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                    .Select(c => new CategoryTreeDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        ParentId = c.ParentId,
                        SortOrder = c.SortOrder,
                        Slug = c.Slug,
                        FieldSchemaJson = c.FieldSchemaJson,
                    })
                    .ToListAsync();

                var lookup = all.ToLookup(c => c.ParentId);
                List<CategoryTreeDto> Build(int? parentId) =>
                    lookup[parentId].Select(c =>
                    {
                        c.Children = Build(c.Id);
                        return c;
                    }).ToList();

                return Build(null);
            });

        public async Task<List<int>> GetDescendantCategoryIdsAsync(int categoryId)
        {
            var all = await _db.Categories.AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, c.ParentId })
                .ToListAsync();

            var result = new List<int> { categoryId };
            void Collect(int parent)
            {
                foreach (var child in all.Where(c => c.ParentId == parent))
                {
                    result.Add(child.Id);
                    Collect(child.Id);
                }
            }
            Collect(categoryId);
            return result;
        }

        public override async Task<IResult> CreateAsync(CategoryCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Result.Fail(string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var entity = _mapper.Map<Category>(dto);
            entity.CreatedTime = DateTime.Now;
            entity.IsActive = true;

            await _uow.GetRepository<Category>().CreateAsync(entity);
            await _uow.SaveChanges();
            await _cache.RemoveAsync("categories:tree");
            return Result.Ok("Kategori oluşturuldu.");
        }
    }
}
