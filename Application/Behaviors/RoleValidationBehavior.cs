using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Interfaces;
using CA.Platform.Attributes;
using CA.Platform.Exceptions;
using MediatR;

namespace CA.Platform.Application.Behaviors
{
    public class RoleValidationBehavior<TRequest, TResponse>: IPipelineBehavior<TRequest, TResponse>
    {
        private const string AccessDenied = "AccessDenied";
        
        private readonly IUserContext _userContext;

        public RoleValidationBehavior(IUserContext userContext)
        {
            _userContext = userContext;
        }
        
        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var attributes = request.GetType().GetCustomAttributes(typeof(AccessAttribute), false).Cast<AccessAttribute>().ToArray();

            if (!attributes.Any())
                return next();
            
            if (!_userContext.GetPermissions().Any(a => attributes.Any(b => b.PermissionKey == a)))
                throw new ValidationException(AccessDenied);

            return next();
        }
    }
}