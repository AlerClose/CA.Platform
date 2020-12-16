using System;
using System.Collections.Generic;
using CA.Platform.Application.Contracts;

namespace CA.Platform.Application.Interfaces
{
    public interface IUserContext
    {
        UserDto GetCurrentUser();

        Guid GetCurrentUserId();

        List<string> GetPermissions();
    }
}