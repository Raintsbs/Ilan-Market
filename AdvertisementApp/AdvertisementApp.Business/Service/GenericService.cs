using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Result;
using AdvertisementApp.DataAccess.Interface;
using AdvertisementApp.Entities;
using AutoMapper;
using FluentValidation;

namespace AdvertisementApp.Business.Service
{
    public class GenericService<TEntity, TListDto, TCreateDto, TUpdateDto> : IGenericService<TEntity, TListDto, TCreateDto, TUpdateDto>
        where TEntity : class
        where TListDto : class
        where TCreateDto : class
        where TUpdateDto : class
    {
        protected readonly IUnitOfWork _uow;
        protected readonly IMapper _mapper;
        protected readonly IValidator<TCreateDto> _createValidator;
        protected readonly IValidator<TUpdateDto> _updateValidator;

        public GenericService(IUnitOfWork uow, IMapper mapper, IValidator<TCreateDto> createValidator, IValidator<TUpdateDto> updateValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public virtual async Task<IResult> CreateAsync(TCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Result.Fail(string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var entity = _mapper.Map<TEntity>(dto);
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.CreatedTime = DateTime.Now;
                baseEntity.IsActive = false; // Onay bekliyor, varsayılan pasif
            }

            await _uow.GetRepository<TEntity>().CreateAsync(entity);
            await _uow.SaveChanges();
            return Result.Ok("Kayıt oluşturuldu.");
        }

        public virtual async Task<IResult> DeleteAsync(int id)
        {
            var entity = await _uow.GetRepository<TEntity>().FindAsync(id);
            if (entity == null)
                return Result.Fail("Kayıt bulunamadı.");

            try
            {
                _uow.GetRepository<TEntity>().Delete(entity);
                await _uow.SaveChanges();
                return Result.Ok("Kayıt başarıyla silindi.");
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                if (inner.Contains("REFERENCE") || inner.Contains("FK") || inner.Contains("foreign key"))
                    return Result.Fail("Bu kayıt başka verilerle ilişkili olduğu için silinemez.");
                return Result.Fail($"Silme işlemi başarısız: {inner}");
            }
        }

        public virtual async Task<IDataResult<List<TListDto>>> GetAllAsync()
        {
            var entities = await _uow.GetRepository<TEntity>().GetAllAsync();
            var list = _mapper.Map<List<TListDto>>(entities);
            return DataResult<List<TListDto>>.Ok(list);
        }

        public virtual async Task<IDataResult<TListDto>> GetByIdAsync(int id)
        {
            var entity = await _uow.GetRepository<TEntity>().FindAsync(id);
            if (entity == null)
                return DataResult<TListDto>.Fail("Kayıt bulunamadı.");

            return DataResult<TListDto>.Ok(_mapper.Map<TListDto>(entity));
        }

        public virtual async Task<IResult> UpdateAsync(TUpdateDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Result.Fail(string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var idProperty = typeof(TUpdateDto).GetProperty("Id");
            if (idProperty == null)
                return Result.Fail("Güncelleme için Id alanı gerekli.");

            var idValue = (int)idProperty.GetValue(dto)!;
            var unchangedEntity = await _uow.GetRepository<TEntity>().FindAsync(idValue);
            if (unchangedEntity == null)
                return Result.Fail("Kayıt bulunamadı.");

            var entity = _mapper.Map<TEntity>(dto);
            if (entity is BaseEntity mapped && unchangedEntity is BaseEntity existing)
            {
                mapped.CreatedTime = existing.CreatedTime;
                mapped.UpdatedTime = DateTime.Now;
            }

            _uow.GetRepository<TEntity>().Update(entity, unchangedEntity);
            await _uow.SaveChanges();
            return Result.Ok("Kayıt güncellendi.");
        }
    }
}
