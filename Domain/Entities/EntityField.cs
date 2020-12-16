using CA.Platform.Entities.Interfaces;

namespace CA.Platform.Entities
{
    public class EntityField: IEntity
    {
        public int Id { get; set; }
        
        public EntityType EntityType { get; set; }
        
        public string Name { get; set; }
        
        public string Title { get; set; }
        
        public string TypeName { get; set; }
        
        public bool IsDeleted { get; set; }
    }
}