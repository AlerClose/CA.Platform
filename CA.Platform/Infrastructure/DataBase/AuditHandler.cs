using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using CA.Platform.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CA.Platform.Infrastructure.DataBase;

public class AuditHandler : IEntitySaveHandler
{
    private readonly IEntityService _entityService;
    private readonly IAuditService _auditService;
    private readonly IUserContext _userContext;

    public AuditHandler(IEntityService entityService, IAuditService auditService, IUserContext userContext)
    {
        _entityService = entityService;
        _auditService = auditService;
        _userContext = userContext;
    }

    public void Save(EntityEntry entity)
    {
        if (entity.Entity is not BaseObject)
            return;

        _auditService.AddRecordsAsync(GetAuditRecords(entity), CancellationToken.None);
    }

    private IEnumerable<AuditRecord> ConvertToAuditRecords(EntityEntry entityEntry, BaseObject baseObject,
        EntityOperationEnum entityOperation)
    {
        var properties = entityOperation == EntityOperationEnum.Create
            ? entityEntry.Properties
            : entityEntry.Properties.Where(t => t.IsModified);

        foreach (var property in properties)
        {
            if (new[]
                {
                    nameof(BaseObject.Created),
                    nameof(BaseObject.CreatedBy),
                    nameof(BaseObject.LastModified),
                    nameof(BaseObject.LastModifiedBy)
                }.Contains(property.Metadata.Name))
                continue;

            var field = _entityService.GetEntityFieldDto(entityEntry.Entity.GetType(), property.Metadata.Name);
            if (field == null)
                continue;

            var originalValue = entityOperation == EntityOperationEnum.Create
                ? null
                : _auditService.GetFieldValue(property.OriginalValue);

            var currentValue = _auditService.GetFieldValue(property.CurrentValue);

            yield return new AuditRecord()
            {
                Date = DateTime.Now,
                FieldId = field.Id,
                Operation = entityOperation,
                UserId = _userContext.GetCurrentUserId(),
                ObjectId = baseObject.Id,
                OldValue = originalValue,
                NewValue = currentValue,
                Id = Guid.NewGuid()
            };
        }
    }

    private IEnumerable<AuditRecord> GetAuditRecords(EntityEntry entityEntry)
    {
        var baseObject = entityEntry.Entity as BaseObject;

        if (baseObject == null || !_auditService.NeedToWriteAudit(entityEntry.Entity.GetType()))
            return new List<AuditRecord>();

        if (entityEntry.State == EntityState.Added)
            return ConvertToAuditRecords(entityEntry, baseObject, EntityOperationEnum.Create);

        if (baseObject.IsDeleted && entityEntry.Properties.Any(a =>
                a.IsModified && a.Metadata.Name == nameof(BaseObject.IsDeleted)))
            return new[] { GetDeletedAuditRecord(entityEntry, baseObject) };

        return ConvertToAuditRecords(entityEntry, baseObject, EntityOperationEnum.Update);
    }

    private AuditRecord GetDeletedAuditRecord(EntityEntry entityEntry, BaseObject baseObject)
    {
        return new AuditRecord()
        {
            Operation = EntityOperationEnum.Delete,
            ObjectId = baseObject.Id,
            Date = DateTime.Now,
            FieldId = _entityService.GetEntityFieldDto(entityEntry.Entity.GetType(), nameof(BaseObject.IsDeleted)).Id,
            UserId = _userContext.GetCurrentUserId(),
            NewValue = true.ToString(),
            OldValue = false.ToString(),
            Id = Guid.NewGuid()
        };
    }
}