using System;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CA.Platform.Infrastructure.DataBase;

class DefaultPropsHandler: IEntitySaveHandler
{
    private readonly IUserContext _userContext;

    public DefaultPropsHandler(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public void Save(EntityEntry itemToCreate)
    {
        if (itemToCreate.Entity is not BaseObject baseObject)
            return;
        
        if (itemToCreate.State == EntityState.Added)
        {
            baseObject.Created = DateTime.Now;
            baseObject.CreatedBy = _userContext.GetCurrentUserId();
        }
        
        if (itemToCreate.State is EntityState.Modified or EntityState.Deleted)
        {
            baseObject.LastModified = DateTime.Now;
            baseObject.LastModifiedBy = _userContext.GetCurrentUserId();
        }
    }
}