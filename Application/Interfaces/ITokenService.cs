using System.Collections.Generic;
using CA.Platform.Entities.Identity;

namespace CA.Platform.Application.Interfaces
{
    public interface ITokenService
    {
        string GetToken(User user, List<Permission> permissions);
    }
}