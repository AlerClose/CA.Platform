using System.Threading.Tasks;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Users.Commands;
using CA.Platform.Application.Users.Query;
using CA.WebPlatform;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CA.Platform.WebApp
{
    [Route("api/user")]
    public class UserController : BaseApiController
    {
        public UserController(IMediator mediator) : base(mediator)
        {
        }
        
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateCommand authenticateCommand)
        {
            var token = await Mediator.Send(authenticateCommand);
            if (token == null)
                return BadRequest();

            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand registerCommand)
        {
            var result = await Mediator.Send(registerCommand);
            if (!result.Ok)
                return BadRequest(result.Message);

            return Ok();
        }
        
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UserDto userDto)
        {
            var result = await Mediator.Send(new UpdateUserCommand(userDto));
            if (!result.Ok)
                return BadRequest(result.Message);
            
            return Ok();
        }
        
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand changePasswordCommand)
        {
            var result = await Mediator.Send(changePasswordCommand);
            if (!result.Ok)
                return BadRequest(result.Message);
            
            return Ok();
        }

        [HttpGet("permissions")]
        public async Task<string []> GetPermissions()
        {
            return await Mediator.Send(new GetPermissionsQuery());
        }
    }
}