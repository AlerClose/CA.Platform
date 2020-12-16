using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Application.Users.Commands
{
    public class RegisterUserCommand: IRequest<OkDto>
    {
        private const string EmailExists = "EmailExists";
        private const string EmailTooLarge = "EmailTooLarge";
        private const string EmailIsNotEmail = "EmailIsNotEmail";
        private const string EmailEmpty = "EmailEmpty";
        private const string PasswordEmpty = "PasswordEmpty";
        
        public string Password { get; set; }
        
        public string Email { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        class RegisterUserCommandHandler: IRequestHandler<RegisterUserCommand, OkDto>
        {
            private readonly IDbContext _dbContext;
            private readonly IStringHashService _hashService;

            public RegisterUserCommandHandler(IDbContext dbContext, IStringHashService hashService)
            {
                _dbContext = dbContext;
                _hashService = hashService;
            }
            
            public async Task<OkDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
            {
                var dbSet = _dbContext.GetDbSet<User>();
                if (await dbSet.AnyAsync(a => !a.IsDeleted && request.Email == a.Email, cancellationToken))
                    return new OkDto(EmailExists);

                dbSet.Add(new User()
                {
                    Email = request.Email,
                    Login = request.Email,
                    Password = _hashService.GetHash(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
                
                return new OkDto();
            }
        }

        class RegisterUserCommandValidator: AbstractValidator<RegisterUserCommand>
        {
            public RegisterUserCommandValidator()
            {
                RuleFor(command => command.Email).NotEmpty().WithErrorCode(EmailEmpty);
                RuleFor(command => command.Email).MaximumLength(200).WithErrorCode(EmailTooLarge);
                RuleFor(command => command.Email).Matches("*@*.*").WithErrorCode(EmailIsNotEmail);
                
                RuleFor(command => command.Password).NotEmpty().WithErrorCode(PasswordEmpty);
            }
        }
    }
}