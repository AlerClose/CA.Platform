using CA.Platform.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CA.Platform.Application.Interfaces;

public interface IEntitySaveHandler
{
    void Save(EntityEntry entry);
}