using System.Collections;
using System.Collections.Generic;
using CA.Platform.Entities.Interfaces;

namespace CA.Platform.Entities.Identity
{
    public class Permission: BaseObject, ITitle, IKey
    {
        public Application Application { get; set; }
        
        public ICollection<RolePermission> Roles { get; } = new HashSet<RolePermission>();
        public string Title { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
    }
}