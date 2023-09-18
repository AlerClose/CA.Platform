using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Infrastructure.DataBase;

public interface IDbEntityModelExtender
{
    public void ExtendEntities(ModelBuilder modelBuilder);
}