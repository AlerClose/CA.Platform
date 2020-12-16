using CA.Platform.Application.Contracts;
using CA.Platform.Entities;

namespace CA.Platform.Application.Entities.Queries
{
    public class EntityFieldDto: BaseDto<EntityField>
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Title { get; set; }
        
        public string TypeName { get; set; }
    }
}