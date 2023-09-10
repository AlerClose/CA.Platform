using System.Collections.Generic;
using System.Threading.Tasks;
using CA.Platform.Application.Entities.Queries;
using CA.WebPlatform;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CA.Platform.WebApp
{
    [Route("api/entities")]
    internal class EntityController : BaseApiController
    {
        public EntityController(IMediator mediator) : base(mediator)
        {
        }
        
        [HttpGet("types")]
        public async Task<List<EntityTypeDto>> GetTypes()
        {
            return await Mediator.Send(new GetEntitiesListQuery());
        }
        
        [HttpGet("types/{id}/fields")]
        public async Task<List<EntityFieldDto>> GetFields(int id)
        {
            return await Mediator.Send(new GetEntityFieldsQuery()
            {
                EntityId = id
            });
        }
    }
}