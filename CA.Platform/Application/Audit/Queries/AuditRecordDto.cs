using System;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Entities.Queries;
using CA.Platform.Entities;

namespace CA.Platform.Application.Audit.Queries
{
    public class AuditRecordDto: BaseDto<AuditRecord>
    {
        public Guid Id { get; set; }
        
        public Guid ObjectId { get; set; }
        
        public EntityFieldDto Field { get; set; }
        
        public string OldValue { get; set; }
        
        public string NewValue { get; set; }
        
        public DateTime Date { get; set; }

        public UserDto User { get; set; }
        
        public EntityOperationEnum Operation { get; set; }
    }
}