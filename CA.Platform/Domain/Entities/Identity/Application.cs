using System.Collections;
using System.Collections.Generic;
using CA.Platform.Entities.Interfaces;

namespace CA.Platform.Entities.Identity
{
    public class Application: BaseObject, ITitle, IKey
    {
        public ICollection<Role> Roles { get; }  = new HashSet<Role>();
        
        public ICollection<Permission> Permissions { get; } = new HashSet<Permission>();
        
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public string Key { get; set; }
    }
}