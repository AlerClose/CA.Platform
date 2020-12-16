using System;

namespace CA.Platform.Entities.Identity
{
    public class RolePermission
    {
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        public Guid PermissionId { get; set; }
        public  Permission Permission { get; set; }
    }
}