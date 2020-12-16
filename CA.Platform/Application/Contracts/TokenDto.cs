using System.Collections.Generic;

namespace CA.Platform.Application.Contracts
{
    public class TokenDto
    {
        public string Token { get; set; }
        
        public UserDto User { get; set; }
        
        public List<PermissionDto> Permissions { get; set; }
    }
}