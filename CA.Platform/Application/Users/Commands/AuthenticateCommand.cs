using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Application.Users.Commands
{
    public class AuthenticateCommand: IRequest<TokenDto>
    {
        public  string Email { get; set; }
        
        public string Password { get; set; }
        
        public class AuthenticateCommandHandler: IRequestHandler<AuthenticateCommand, TokenDto>
        {
            private readonly IDbContext _dbContext;
            private readonly IStringHashService _hashService;
            private readonly ITokenService _tokenService;

            public AuthenticateCommandHandler(IDbContext dbContext, IStringHashService hashService, ITokenService tokenService)
            {
                _dbContext = dbContext;
                _hashService = hashService;
                _tokenService = tokenService;
            }
            
            public async Task<TokenDto> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
            {
                string password = _hashService.GetHash(request.Password);
                var user = await _dbContext.GetDbSet<User>()
                    .Include(a => a.Roles)
                        .FirstOrDefaultAsync(t =>
                    !t.IsDeleted && t.Email == request.Email &&
                    t.Password == password, cancellationToken: cancellationToken);
                
                if (user == null)
                    return null;

                var roles = user.Roles.Select(a => a.RoleId);

                var permissions = await _dbContext.GetDbSet<Permission>()
                    .Where(t => !t.IsDeleted && t.Roles.Any(b => !b.Role.IsDeleted && roles.Contains(b.RoleId)))
                    .Include(a=>a.Application)
                    .ToListAsync(cancellationToken: cancellationToken);
                
                var token = _tokenService.GetToken(user, permissions);

                return new TokenDto()
                {
                    Token = token,
                    Permissions = permissions.Select(a => new PermissionDto(a)).ToList(),
                    User = new UserDto(user)
                };
            }
        }
    }
}