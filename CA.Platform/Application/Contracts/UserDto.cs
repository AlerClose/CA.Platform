using System;
using CA.Platform.Entities.Identity;

namespace CA.Platform.Application.Contracts
{
    public class UserDto
    {
        public UserDto()
        {
            
        }
        
        public UserDto(User user)
        {
            Email = user.Email;
            Id = user.Id;
            Login = user.Login;
            FirstName = user.FirstName;
            IsActive = user.IsActive;
            LastName = user.LastName;
        }
        
        public Guid Id { get; set; }
        
        public  string Email { get; set; }
        
        public string Login { get; set; }

        public bool IsActive { get; set; }
        
        public  string FirstName { get; set; }
        
        public  string LastName { get; set; }
    }
}