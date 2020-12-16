using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CA.Platform.Application.Common;
using CA.Platform.Application.Entities.Queries;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using CA.Platform.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Application.Default.Queries
{
    public class GetDefaultItemQuery: IRequest<DefaultItemDto>
    {
        public int EntityTypeId { get; set; }

        public Guid ItemId { get; set; }

        public class GetDefaultItemQueryHandler : IRequestHandler<GetDefaultItemQuery, DefaultItemDto>
        {
            private readonly IDbContext _dbContext;
            private readonly IEntityService _entityService;
            private readonly IMapper _mapper;

            private static readonly GenericMethodInfoProvider<GetDefaultItemQueryHandler> GetItemMethodInfoProvider =
                new GenericMethodInfoProvider<GetDefaultItemQueryHandler>(nameof(GetItem));

            public GetDefaultItemQueryHandler(IDbContext dbContext, IEntityService entityService, IMapper mapper)
            {
                _dbContext = dbContext;
                _entityService = entityService;
                _mapper = mapper;
            }

            public async Task<DefaultItemDto> Handle(GetDefaultItemQuery request, CancellationToken cancellationToken)
            {
                var entityType = _entityService.GetEntityTypeById(request.EntityTypeId);

                var task =
                    GetItemMethodInfoProvider.GetGenericMethod(entityType)
                            .Invoke(this,
                                new object[] {request.ItemId, _entityService.GetEntityTypeDto(request.EntityTypeId)}) as
                        Task<DefaultItemDto>;

                if (task == null)
                    throw new NotSupportedException();

                return await task;
            }

            private DefaultItemDto ConvertToDto<T>(T item, EntityTypeDto entityTypeDto) where T : BaseObject
            {
                var result = new DefaultItemDto()
                {
                    EntityType = entityTypeDto,
                    Fields = GetFields(item).ToList()
                };
                return result;
            }

            private IEnumerable<DefaultItemDto.FieldDto> GetFields<T>(T item) where T : BaseObject
            {
                var entityType = typeof(T);
                var properties = entityType.GetProperties();

                foreach (var property in properties)
                {
                    var entityField = _entityService.GetEntityFieldDto(entityType, property.Name);

                    if (entityField == null)
                        continue;

                    var result = new DefaultItemDto.FieldDto()
                    {
                        EntityField = entityField,
                        Value = _entityService.GetFieldValueAsString(property.GetValue(item))
                    };
                    yield return result;
                }
            }

            private async Task<DefaultItemDto> GetItem<T>(Guid id, EntityTypeDto entityTypeDto) where T : BaseObject
            {
                T item = await _dbContext.GetDbSet<T>().FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
                if (item == null)
                    throw new NotFoundException(id, typeof(T));

                return ConvertToDto(item, entityTypeDto);
            }
        }
    }
}