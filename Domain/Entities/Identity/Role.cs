using System.Collections;
using System.Collections.Generic;
using CA.Platform.Entities.Interfaces;

namespace CA.Platform.Entities.Identity
{
    public class Role: BaseObject, ITitle, IKey
    {
        public ICollection<UserRole> Users { get; } = new HashSet<UserRole>();
        public ICollection<RolePermission> Permissions { get; } = new HashSet<RolePermission>();
        
        public Application Application { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
    }
}