namespace CA.Platform.Entities.Interfaces
{
    public interface IEntity
    {
        int Id { get; set; }
        
        string Name { get; set; }
        
        string Title { get; set; }
        
        bool IsDeleted { get; set; }
    }
}