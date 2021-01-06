using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Default.Commands;
using CA.Platform.Application.Default.Queries;
using CA.WebPlatform;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CA.Platform.WebApp
{
    [Route("api/default")]
    public abstract class BaseDefaultController: BaseApiController
    {
        protected BaseDefaultController(IMediator mediator) : base(mediator)
        {
        }
        
        [HttpPost("{entityId}")]
        public async Task<List<LookupDto>> GetList(int entityId, [FromQuery] int page, [FromQuery] int pageSize,
            [FromBody] List<UpdateFieldDto> fields, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new GetDefaultItemListQuery()
            {
                EntityTypeId = entityId,
                Page = page,
                PageSize = pageSize,
                FilterFields = fields
            }, cancellationToken);
        }

        [HttpGet("{entityId}/{itemId}")]
        public async Task<DefaultItemDto> GetItem(int entityId, Guid itemId, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new GetDefaultItemQuery()
            {
                ItemId = itemId,
                EntityTypeId = entityId
            }, cancellationToken);
        }

        [HttpPost("{entityId}/{itemId}")]
        public async Task<ResultDto> UpdateItem([FromBody] List<UpdateFieldDto> fieldsToUpdate, int entityId, Guid itemId, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new UpdateDefaultItemCommand()
            {
                EntityTypeId = entityId,
                FieldsToUpdate = fieldsToUpdate,
                ItemId = itemId
            }, cancellationToken);
        }
        
        [HttpPost("{entityId}/create")]
        public async Task<ResultDto> CreateItem([FromBody] List<UpdateFieldDto> fieldsToUpdate, int entityId, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new CreateDefaultItemCommand()
            {
                EntityTypeId = entityId,
                FieldsToUpdate = fieldsToUpdate
            }, cancellationToken);
        }
        
        [HttpDelete("{entityId}/{itemId}")]
        public async Task<ResultDto> Delete(int entityId, Guid itemId, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new DeleteDefaultItemCommand()
            {
                EntityTypeId = entityId,
                ItemId = itemId
            }, cancellationToken);
        }
    }
}