using System.Collections.Generic;
using System.Threading.Tasks;
using CA.Platform.Application.Entities.Queries;
using CA.WebPlatform;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CA.Platform.WebApp
{
    [Route("api/entities")]
    public abstract class BaseEntityController : BaseApiController
    {
        protected BaseEntityController(IMediator mediator) : base(mediator)
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