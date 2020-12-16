using System;
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
    public class DeleteDefaultItemCommand: IRequest<ResultDto>
    {
        public int EntityTypeId { get; set; }
        
        public Guid ItemId { get; set; }
        
        public class DeleteDefaultItemCommandHandler: IRequestHandler<DeleteDefaultItemCommand, ResultDto>
        {
            private readonly IEntityService _entityService;
            private readonly IDbContext _dbContext;
            
            private static readonly GenericMethodInfoProvider<DeleteDefaultItemCommandHandler> DeleteItemMethodInfoProvider = new GenericMethodInfoProvider<DeleteDefaultItemCommandHandler>(nameof(DeleteItem));

            public DeleteDefaultItemCommandHandler(IEntityService entityService, IDbContext dbContext)
            {
                _entityService = entityService;
                _dbContext = dbContext;
            }
            
            public async Task<ResultDto> Handle(DeleteDefaultItemCommand request, CancellationToken cancellationToken)
            {
                var entityType = _entityService.GetEntityTypeById(request.EntityTypeId);

                var task = DeleteItemMethodInfoProvider.GetGenericMethod(entityType)
                    .Invoke(this, new object[] {request.ItemId, cancellationToken}) as Task<ResultDto>;
                if (task == null)
                    throw new NotSupportedException();

                return await task;
            }

            private async Task<ResultDto> DeleteItem<T>(Guid itemId, CancellationToken cancellationToken) where T : BaseObject
            {
                var item = await _dbContext.GetDbSet<T>().FirstOrDefaultAsync(t => !t.IsDeleted && t.Id == itemId, cancellationToken: cancellationToken);
                if (item == null)
                    throw new NotFoundException(itemId, typeof(T));

                item.IsDeleted = true;

                await _dbContext.SaveChangesAsync(cancellationToken);
                
                return new ResultDto(itemId);
            }
        }
    }
}