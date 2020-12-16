using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Entities.Queries;
using CA.Platform.Entities;

namespace CA.Platform.Application.Interfaces
{
    public interface IEntityService
    {
        EntityFieldDto GetEntityFieldDto(Type entityType, string fieldName);

        EntityFieldDto GetEntityFieldById(int id);

        PropertyInfo GetEntityFieldPropertyById(int id);

        Type GetEntityTypeById(int id);

        EntityTypeDto GetEntityTypeDto(int id);
        
        int GetEntityTypeId(Type entityType);
        
        string GetFieldValueAsString(object getValue);

        void ClearCaches();
        
        Task SetEntityFieldValue<T>(T instance, int entityFieldId, string updateFieldValue, CancellationToken cancellationToken) where T : BaseObject;
        
        IEnumerable<LookupDto> ConvertToLookupDto<T>(IEnumerable<T> items) where T : BaseObject;
    }
}