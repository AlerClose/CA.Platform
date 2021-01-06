using System;
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
    public class UpdateUserCommand: IRequest<OkDto>
    {
        private const string EmailExists = "EmailExists";
        private const string EmailTooLarge = "EmailTooLarge";
        private const string EmailIsNotEmail = "EmailIsNotEmail";
        private const string EmailEmpty = "EmailEmpty";
        
        public string Email { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public UpdateUserCommand(UserDto dto)
        {
            Email = dto.Email;
            LastName = dto.LastName;
            FirstName = dto.FirstName;
        }

        class Handler: IRequestHandler<UpdateUserCommand, OkDto>
        {
            private readonly IDbContext _context;
            private readonly IUserContext _userContext;

            public Handler(IDbContext context, IUserContext userContext)
            {
                _context = context;
                _userContext = userContext;
            }
            
            public async Task<OkDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
            {
                var userId = _userContext.GetCurrentUserId();
                
                var user = await _context.GetDbSet<User>().FirstAsync(a => a.IsActive && !a.IsDeleted && a.Id == userId, cancellationToken: cancellationToken);
                if (user.Email != request.Email)
                    if (await _context.GetDbSet<User>().AnyAsync(a => !a.IsDeleted && a.Email == request.Email && a.Id != userId))
                        return new OkDto(EmailExists);
                
                user.Email = request.Email;
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;

                await _context.SaveChangesAsync(cancellationToken);
                return new OkDto();
            }
            
        }
        
        class Validator: AbstractValidator<UpdateUserCommand>
        {
            public Validator()
            {
                RuleFor(command => command.Email).NotEmpty().WithErrorCode(EmailEmpty);
                RuleFor(command => command.Email).MaximumLength(200).WithErrorCode(EmailTooLarge);
                RuleFor(command => command.Email).Matches("*@*.*").WithErrorCode(EmailIsNotEmail);
            }
        }
    }
}