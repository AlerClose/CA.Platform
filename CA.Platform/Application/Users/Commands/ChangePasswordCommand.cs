using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Application.Users.Commands
{
    public class ChangePasswordCommand: IRequest<OkDto>
    {
        public string OldPassword { get; set; }
        
        public string NewPassword { get; set; }

        class Handler : IRequestHandler<ChangePasswordCommand, OkDto>
        {
            private readonly IDbContext _dbContext;
            private readonly IUserContext _userContext;
            private readonly IStringHashService _hashService;

            public Handler(IDbContext dbContext, IUserContext userContext, IStringHashService hashService)
            {
                _dbContext = dbContext;
                _userContext = userContext;
                _hashService = hashService;
            }
            
            public async Task<OkDto> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
            {
                var userId = _userContext.GetCurrentUserId();
                var passwordHash = _hashService.GetHash(request.OldPassword);
                
                var user = await _dbContext.GetDbSet<User>().FirstOrDefaultAsync(a => a.IsActive && !a.IsDeleted && a.Id == userId && a.Password == passwordHash, cancellationToken: cancellationToken);
                if (user == null)
                    return new OkDto("PasswordNotCorrect");

                user.Password = _hashService.GetHash(request.NewPassword);

                await _dbContext.SaveChangesAsync(cancellationToken);
                return new OkDto();
            }
        }
    }
}