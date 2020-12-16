using System;
using CA.Platform.Entities;
using CA.Platform.Entities.Interfaces;

namespace CA.Platform.Application.Contracts
{
    public abstract class BaseObjectDto<T>: BaseDto<T>, IBaseObject where T: BaseObject
    {
        public Guid? LastModifiedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public DateTime Created { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
    }
}