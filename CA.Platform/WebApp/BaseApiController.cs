using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CA.Platform.WebApp
{
    [Authorize]
    [ApiController]
    public abstract class BaseApiController: ControllerBase
    {
        protected IMediator Mediator { get; }

        protected BaseApiController(IMediator mediator)
        {
            Mediator = mediator;
        }
    }
}