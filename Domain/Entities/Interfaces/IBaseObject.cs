using System;

namespace CA.Platform.Entities.Interfaces
{
    public interface IBaseObject
    {
        Guid? LastModifiedBy { get; set; }
        
        DateTime? LastModified { get; set; }
        
        DateTime Created { get; set; }
        
        Guid CreatedBy { get; set; }
        
        Guid Id { get; set; }
        
        bool IsDeleted { get; set; }
    }
}