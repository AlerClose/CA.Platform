using System;
using CA.Platform.Entities.Identity;

namespace CA.Platform.Application.Contracts
{
    public class PermissionDto
    {
        public  Guid Id { get; set; }
        
        public  string Key { get; set; }
        
        public  string Title { get; set; }
        
        public  string AppKey { get; set; }

        public PermissionDto(Permission permission)
        {
            Id = permission.Id;
            Key = permission.Key;
            Title = permission.Title;
            AppKey = permission.Application?.Key;
        }

        public PermissionDto()
        {
            
        }
    }
}