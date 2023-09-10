using CA.Platform.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Test.Database;

public class DataContext : BaseDbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
    public DataContext()
    {
        
    }
    
    public DbSet<Topic> Topics { get; set; }
}