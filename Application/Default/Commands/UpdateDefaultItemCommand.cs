using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Common;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using CA.Platform.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Application.Default.Commands
{
    public class UpdateDefaultItemCommand: IRequest<ResultDto>
    {
        public int EntityTypeId { get; set; }
        
        public List<UpdateFieldDto> FieldsToUpdate { get; set; }
        
        public Guid ItemId { get; set; }

        public class UpdateDefaultItemCommandHandler : IRequestHandler<UpdateDefaultItemCommand, ResultDto>
        {
            private readonly IDbContext _dbContext;
            private readonly IEntityService _entityService;

            private static readonly GenericMethodInfoProvider<UpdateDefaultItemCommandHandler>
                UpdateItemMethodInfoProvider =
                    new GenericMethodInfoProvider<UpdateDefaultItemCommandHandler>(nameof(UpdateItem));

            public UpdateDefaultItemCommandHandler(IDbContext dbContext, IEntityService entityService)
            {
                _dbContext = dbContext;
                _entityService = entityService;
            }

            public async Task<ResultDto> Handle(UpdateDefaultItemCommand request, CancellationToken cancellationToken)
            {
                var entityType = _entityService.GetEntityTypeById(request.EntityTypeId);

                var task = UpdateItemMethodInfoProvider.GetGenericMethod(entityType)
                        .Invoke(this, new object[] {request.ItemId, request.FieldsToUpdate, cancellationToken}) as
                    Task<ResultDto>;
                if (task == null)
                    throw new NotSupportedException();

                return await task;
            }

            private async Task<ResultDto> UpdateItem<T>(Guid itemId, List<UpdateFieldDto> updateFields,
                CancellationToken cancellationToken) where T : BaseObject
            {
                T instance = await _dbContext.GetDbSet<T>().FirstOrDefaultAsync(t => t.Id == itemId && !t.IsDeleted, cancellationToken: cancellationToken);
                
                if (instance == null)
                    throw new NotFoundException(itemId, typeof(T));

                foreach (var updateField in updateFields)
                {
                    await _entityService.SetEntityFieldValue(instance, updateField.FieldId, updateField.Value,
                        cancellationToken);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                return new ResultDto(instance.Id);
            }
        }
    }
}