using System.Collections.Generic;
using CA.Platform.Entities.Interfaces;

namespace CA.Platform.Entities
{
    public class EntityType: IEntity
    {
        public EntityType()
        {
            Fields = new HashSet<EntityField>();
        }
        
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Title { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public ICollection<EntityField> Fields { get; set; }
    }
}