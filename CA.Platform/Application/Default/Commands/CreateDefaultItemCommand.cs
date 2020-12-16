using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Common;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using MediatR;

namespace CA.Platform.Application.Default.Commands
{
    public class CreateDefaultItemCommand: IRequest<ResultDto>
    {
        public int EntityTypeId { get; set; }
        
        public List<UpdateFieldDto> FieldsToUpdate { get; set; }
        
        public class CreateDefaultItemCommandHandler: IRequestHandler<CreateDefaultItemCommand, ResultDto>
        {
            private readonly IDbContext _dbContext;
            private readonly IEntityService _entityService;

            private static readonly GenericMethodInfoProvider<CreateDefaultItemCommandHandler> CreateItemMethodInfoProvider = new GenericMethodInfoProvider<CreateDefaultItemCommandHandler>(nameof(CreateItem)); 

            public CreateDefaultItemCommandHandler(IDbContext dbContext, IEntityService entityService)
            {
                _dbContext = dbContext;
                _entityService = entityService;
            }

            public async Task<ResultDto> Handle(CreateDefaultItemCommand request, CancellationToken cancellationToken)
            {
                var entityType = _entityService.GetEntityTypeById(request.EntityTypeId);

                await _dbContext.SaveChangesAsync(cancellationToken);
                
                var task = CreateItemMethodInfoProvider.GetGenericMethod(entityType)
                    .Invoke(this, new object[] {request.FieldsToUpdate, cancellationToken}) as Task<ResultDto>;
                if (task == null)
                    throw new NotSupportedException();
                
                return await task;
            }

            private async Task<ResultDto> CreateItem<T>(List<UpdateFieldDto> updateFields, CancellationToken cancellationToken) where T: BaseObject
            {
                T instance = Activator.CreateInstance<T>();
                _dbContext.GetDbSet<T>().Add(instance);

                foreach (var updateField in updateFields)
                {
                    await _entityService.SetEntityFieldValue(instance, updateField.FieldId, updateField.Value, cancellationToken);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                
                return new ResultDto(instance.Id);
            }
        }
    }
}