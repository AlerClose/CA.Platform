using CA.Platform.Application.Contracts;
using CA.Platform.Entities;
using CA.Platform.Entities.Interfaces;

namespace CA.Platform.Application.Entities.Queries
{
    public class EntityTypeDto: BaseDto<EntityType>, IEntity
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Title { get; set; }
        
        public bool IsDeleted { get; set; }
    }
}