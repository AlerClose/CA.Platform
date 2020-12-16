using System;
using System.Collections.Generic;

namespace CA.Platform.Entities.Identity
{
    public class User : BaseObject
    {
        public  string Email { get; set; }
        public string Login { get; set; }
        
        public string Password { get; set; }

        public DateTime? ActivateDate { get; set; }

        public bool IsActive { get; set; }
        
        public  string FirstName { get; set; }
        
        public  string LastName { get; set; }

        public ICollection<UserRole> Roles { get; } = new HashSet<UserRole>();
    }
}