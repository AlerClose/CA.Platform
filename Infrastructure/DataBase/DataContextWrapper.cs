using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using CA.Platform.Exceptions;
using CA.Platform.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CA.Platform.Infrastructure.DataBase
{
    class DataContextWrapper<TContext>: IDbContext where TContext : BaseDbContext
    {
        private readonly TContext _dataContext;
        private readonly UserDto _currentUser;
        private readonly IAuditService _auditService;
        private readonly IEntityService _entityService;

        private readonly Dictionary<Type, PropertyInfo> _dbSets = new Dictionary<Type, PropertyInfo>();

        public DataContextWrapper(
            TContext dataContext, 
            IUserContext userContext, 
            IAuditService auditService,
            IEntityService entityService)
        {
            _dataContext = dataContext;
            _currentUser = userContext.GetCurrentUser();
            _auditService = auditService;
            _entityService = entityService;

            foreach (var property in _dataContext.GetDbSetProperties())
            {
                var entityType = property.PropertyType.GenericTypeArguments.First();
                _dbSets.Add(entityType, property);
            }
        }

        public DbSet<T> GetDbSet<T>() where T : class
        {
            if (!_dbSets.ContainsKey(typeof(T)))
                throw new NotRegistredClassException(typeof(T));
            
            return _dbSets[typeof(T)].GetValue(_dataContext) as DbSet<T>;
        }

        public DbSet<T> Set<T>() where T : class
        {
            return _dataContext.Set<T>();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (_dataContext.ChangeTracker != null)
            {
                var entities = _dataContext.ChangeTracker.Entries().ToArray();
                SetUpdateFields(entities);
                SetCreateFields(entities);
            }
            
            WriteAudit(cancellationToken);

            var result = await _dataContext.SaveChangesAsync(cancellationToken);

            return result;
        }

        private void WriteAudit(CancellationToken cancellationToken)
        {
            if (_dataContext.ChangeTracker == null)
                return;

            var auditRecords = new List<AuditRecord>();
            
            _dataContext.ChangeTracker.DetectChanges();
            
            var entries = _dataContext.ChangeTracker.Entries().ToArray();
            
            foreach (var entityEntry in entries)
            {
                auditRecords.AddRange(GetAuditRecords(entityEntry));
            }
            
            if (!auditRecords.Any())
                return;

            _auditService.WriteRecordsAsync(auditRecords, cancellationToken);
        }

        private void SetCreateFields(IEnumerable<EntityEntry> entities)
        {
            var itemsToCreate = entities.Where(t => t.State == EntityState.Added).ToArray();

            foreach (var itemToCreate in itemsToCreate)
            {
                var baseObject = itemToCreate.Entity as BaseObject;
                if (baseObject == null)
                    continue;

                baseObject.Created = DateTime.Now;
                baseObject.CreatedBy = (_currentUser?.Id).GetValueOrDefault();
            }
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
                    UserId = (_currentUser?.Id).GetValueOrDefault(),
                    ObjectId = baseObject.Id,
                    OldValue = originalValue,
                    NewValue = currentValue,
                    Id = Guid.NewGuid()
                };
            }
        }

        private void SetUpdateFields(IEnumerable<EntityEntry> entities)
        {
            var itemsToUpdate = entities.Where(t => t.State == EntityState.Modified).ToArray();

            foreach (var itemToUpdate in itemsToUpdate)
            {
                var baseObject = itemToUpdate.Entity as BaseObject;
                if (baseObject == null)
                    continue;

                baseObject.LastModified = DateTime.Now;
                baseObject.LastModifiedBy = (_currentUser?.Id).GetValueOrDefault();
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
                UserId = (_currentUser?.Id).GetValueOrDefault(),
                NewValue = true.ToString(),
                OldValue = false.ToString(),
                Id = Guid.NewGuid()
            };
        }

        public void Dispose()
        {
            _dataContext?.Dispose();
        }
    }
}