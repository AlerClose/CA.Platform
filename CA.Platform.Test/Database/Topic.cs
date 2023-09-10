using CA.Platform.Entities;
using CA.Platform.Entities.Interfaces;

namespace CA.Platform.Test.Database;

public class Topic : BaseObject, ITitle
{
    public string Title { get; set; }
    
    public string? Description { get; set; }
}