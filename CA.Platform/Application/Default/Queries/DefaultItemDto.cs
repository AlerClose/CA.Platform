using System;
using System.Collections.Generic;
using CA.Platform.Application.Entities.Queries;
using CA.Platform.Entities;

namespace CA.Platform.Application.Default.Queries
{
    public class DefaultItemDto
    {
        public EntityTypeDto EntityType { get; set; }
        
        public DateTime Date => DateTime.Now;
        
        public List<FieldDto> Fields { get; set; }
        
        public class FieldDto
        {
            public EntityField EntityField { get; set; } 
            
            public string Value { get; set; }
        }
    }
}