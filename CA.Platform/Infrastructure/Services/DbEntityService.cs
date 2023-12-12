using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Entities.Queries;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using CA.Platform.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Infrastructure.Services
{
    class DbEntityService<TContext> : IEntityService where TContext: BaseDbContext
    {
        private static readonly Dictionary<Type, Dictionary<string, EntityField>> EntityFieldsCollection = new();
        
        private static readonly Dictionary<int, Type> EntityTypesCollection = new();
        
        private static readonly Dictionary<Type, int> EntityTypeIdCollection = new();
        
        private static readonly Dictionary<int, PropertyInfo> EntityPropertiesAndFields = new();
        
        private static readonly Dictionary<int, EntityField> EntityFieldDtoCollection = new();
        
        private static readonly Dictionary<int, EntityType> EntityTypeDtoCollection = new();

        private static readonly PropertyInfo[] SystemProperties = typeof(BaseObject).GetProperties();
        
        private readonly StringConvertService<TContext> _convertService;

        private readonly TContext _dataContext;

        public DbEntityService(StringConvertService<TContext> convertService, TContext dataContext)
        {
           
            _convertService = convertService;
            _dataContext = dataContext;

            if (EntityFieldsCollection.Count == _dataContext.GetDbSetProperties().Count()) 
                return;
            
            ActualizeAllEntities();
            FillCaches();
        }

        private void FillCaches()
        {
            var dbEntities = _dataContext.EntityTypes.Include(a => a.Fields).ToArray();
            var types = _dataContext.GetDbSetProperties().Select(property => property.PropertyType.GenericTypeArguments.First()).ToDictionary(a=>a.FullName);
            
            foreach (var dbEntity in dbEntities)
            {
                var type = types[dbEntity.Name];
                
                if (!EntityTypesCollection.ContainsKey(dbEntity.Id))
                    EntityTypesCollection.Add(dbEntity.Id, type);
                
                if (!EntityTypeIdCollection.ContainsKey(type))
                    EntityTypeIdCollection.Add(type, dbEntity.Id);
                
                if (!EntityFieldsCollection.ContainsKey(type))
                    EntityFieldsCollection.Add(type, new Dictionary<string, EntityField>());
                
                if (!EntityTypeDtoCollection.ContainsKey(dbEntity.Id))
                    EntityTypeDtoCollection.Add(dbEntity.Id, dbEntity);

                foreach (var field in dbEntity.Fields)
                {
                    var property = type.GetProperty(field.Name);
                    
                    if (!EntityFieldsCollection[type].ContainsKey(field.Name))
                        EntityFieldsCollection[type].Add(field.Name, field);
                    
                    if (!EntityPropertiesAndFields.ContainsKey(field.Id))
                        EntityPropertiesAndFields.Add(field.Id, property);
                    
                    if (!EntityFieldDtoCollection.ContainsKey(field.Id))
                        EntityFieldDtoCollection.Add(field.Id, field);
                }
            }
        }

        private void UpdateEntities(List<Type> entities)
        {
            var dbEntities = _dataContext.EntityTypes.Include(a => a.Fields).ToArray();

            foreach (var entity in entities)
            {
                var dbEntity = ActualizeEntityInfo(entity, dbEntities);
                ActualizeEntityFieldInfo(entity, dbEntity);
            }

            RemoveDeletedEntities(dbEntities, entities);

            _dataContext.SaveChanges();
        }

        private void ActualizeEntityFieldInfo(Type entity, EntityType dbEntity)
        {
            var properties = entity.GetProperties();

            foreach (var property in properties)
            {
                var dbField = dbEntity.Fields.SingleOrDefault(t => t.Name == property.Name);
                if (dbField == null)
                {
                    dbField = new EntityField()
                    {
                        Name = property.Name,
                        Title = property.Name,
                        TypeName = property.PropertyType.Name
                    };
                    dbEntity.Fields.Add(dbField);
                }

                dbField.IsDeleted = false;
                dbField.Title = property.Name;
                dbField.TypeName = property.PropertyType.Name;
            }

            var dbFieldsToRemove = dbEntity.Fields.Where(t => properties.All(property => t.Name != property.Name));
            
            foreach (var field in dbFieldsToRemove)
            {
                field.IsDeleted = true;
            }
        }

        private void RemoveDeletedEntities(EntityType[] dbEntities, List<Type> types)
        {
            var entitiesToRemove =
                dbEntities.Where(t => types.All(type => type.FullName != t.Name));

            foreach (var entityToRemove in entitiesToRemove)
            {
                entityToRemove.IsDeleted = true;
            }
        }

        private EntityType ActualizeEntityInfo(Type entity, EntityType[] dbEntities)
        {
            var dbEntity = dbEntities.FirstOrDefault(t => t.Name == entity.FullName);
            if (dbEntity == null)
            {
                dbEntity = new EntityType()
                {
                    Name = entity.FullName
                };
                _dataContext.EntityTypes.Add(dbEntity);
            }

            dbEntity.IsDeleted = false;

            return dbEntity;
        }

        public EntityField GetEntityFieldDto(Type entityType, string fieldName)
        {
            if (!EntityFieldsCollection.ContainsKey(entityType))
                return null;

            return !EntityFieldsCollection[entityType].ContainsKey(fieldName) ? null : EntityFieldsCollection[entityType][fieldName];
        }

        private void ActualizeAllEntities()
        {
            var dbSetProperties = _dataContext.GetDbSetProperties();

            var entities = dbSetProperties.Select(property => property.PropertyType.GenericTypeArguments.First()).ToList();
            
            UpdateEntities(entities);
        }
        
        public EntityField GetEntityFieldById(int id)
        {
            if (!EntityFieldDtoCollection.ContainsKey(id))
                throw new NotSupportedException($"Not found field with id {id}");
            
            return EntityFieldDtoCollection[id];
        }

        public PropertyInfo GetEntityFieldPropertyById(int id)
        {
            if (!EntityPropertiesAndFields.ContainsKey(id))
                throw new NotSupportedException($"Not found field with id {id}");

            return EntityPropertiesAndFields[id];
        }

        public Type GetEntityTypeById(int id)
        {
            if (!EntityTypesCollection.ContainsKey(id))
                throw new NotSupportedException($"Not found entity with id {id}");
            
            return EntityTypesCollection[id];
        }

        public EntityType GetEntityTypeDto(int id)
        {
            if (!EntityFieldDtoCollection.ContainsKey(id))
                throw new NotSupportedException($"Not found entity with id {id}");

            return EntityTypeDtoCollection[id];
        }

        public int GetEntityTypeId(Type entityType)
        {
            if (!EntityTypeIdCollection.ContainsKey(entityType))
                throw new NotSupportedException($"Not found entity with type {entityType.FullName}");
            
            return EntityTypeIdCollection[entityType];
        }

        public string GetFieldValueAsString(object getValue)
        {
            return _convertService.GetStringValue(getValue);
        }

        public void ClearCaches()
        {
            EntityPropertiesAndFields.Clear();
            EntityFieldsCollection.Clear();
            EntityTypesCollection.Clear();
            EntityTypeIdCollection.Clear();
        }

        public Task SetEntityFieldValue<T>(T instance, int entityFieldId, string updateFieldValue,
            CancellationToken cancellationToken) where T : BaseObject
        {
            var propertyInfo = EntityPropertiesAndFields[entityFieldId];
            if (!propertyInfo.CanWrite)
                throw new NotSupportedException($"Property {propertyInfo.Name} of type {typeof(T).FullName} has no setter");
            
            if (SystemProperties.Any(a=> a.Name == propertyInfo.Name))
                throw new NotSupportedException($"System field {propertyInfo.Name} of type {typeof(T).FullName} cannot be set by user");
            
            return _convertService.SetStringValue(instance, propertyInfo, updateFieldValue, cancellationToken);
        }

        public IEnumerable<LookupDto> ConvertToLookupDto<T>(IEnumerable<T> items) where T : BaseObject
        {
            return _convertService.ConvertToLookupCollectionDto(items);
        }
    }
}