using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Interfaces;
using MediatR;

namespace CA.Platform.Application.Users.Query
{
    public class GetPermissionsQuery: IRequest<string []>
    {
        class GetPermissionsQueryHandler: IRequestHandler<GetPermissionsQuery, string []>
        {
            private readonly IUserContext _userContext;

            public GetPermissionsQueryHandler(IUserContext userContext)
            {
                _userContext = userContext;
            }
            public Task<string[]> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_userContext.GetPermissions().ToArray());
            }
        }
    }
}