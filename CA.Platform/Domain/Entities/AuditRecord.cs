using System;
using CA.Platform.Entities.Identity;

namespace CA.Platform.Entities
{
    public class AuditRecord
    {
        public Guid Id { get; set; }
        
        public Guid ObjectId { get; set; }
        
        public int FieldId { get; set; }
        
        public string OldValue { get; set; }
        
        public string NewValue { get; set; }
        
        public DateTime Date { get; set; }

        public Guid UserId { get; set; }
        
        public EntityOperationEnum Operation { get; set; }
    }
}