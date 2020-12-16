using System;
using System.Collections.Generic;
using CA.Platform.Application.Entities.Queries;

namespace CA.Platform.Application.Default.Queries
{
    public class DefaultItemDto
    {
        public EntityTypeDto EntityType { get; set; }
        
        public DateTime Date => DateTime.Now;
        
        public List<FieldDto> Fields { get; set; }
        
        public class FieldDto
        {
            public EntityFieldDto EntityField { get; set; } 
            
            public string Value { get; set; }
        }
    }
}