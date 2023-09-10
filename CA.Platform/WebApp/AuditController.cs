using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Audit.Queries;
using CA.WebPlatform;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CA.Platform.WebApp
{
    [Route("api/audit")]
    internal class AuditController : BaseApiController
    {
        public AuditController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet("{itemId}")]
        public async Task<List<AuditRecordDto>> GetAuditForItem(Guid itemId, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new GetItemAuditQuery()
            {
                ItemId = itemId
            }, cancellationToken);
        }
    }
}