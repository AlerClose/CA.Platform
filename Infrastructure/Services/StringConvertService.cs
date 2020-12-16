using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Common;
using CA.Platform.Application.Contracts;
using CA.Platform.Entities;
using CA.Platform.Exceptions;
using CA.Platform.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace CA.Platform.Infrastructure.Services
{
    class StringConvertService<TContext> where TContext: BaseDbContext
    {
        private readonly  TContext _dataContext;

        private static readonly GenericMethodInfoProvider<StringConvertService<TContext>> SetLookupItemMethodInfoProvider = new GenericMethodInfoProvider<StringConvertService<TContext>>(nameof(SetLookupItem));
        
        private static readonly GenericMethodInfoProvider<StringConvertService<TContext>> SetLookupItemCollectionMethodInfoProvider = new GenericMethodInfoProvider<StringConvertService<TContext>>(nameof(SetLookupItemCollection));

        public StringConvertService(TContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public string GetStringValue(object value)
        {
            if (value == null)
                return null;

            if (value is BaseObject baseObject)
                return JsonSerializer.Serialize(ConvertToLookupDto(baseObject));

            if (value is IEnumerable enumerable &&
                value.GetType().IsGenericType && 
                value.GetType().GetGenericTypeDefinition() == typeof(HashSet<>))
                return JsonSerializer.Serialize(ConvertToLookupCollectionDto(enumerable)); 
            
            return value.ToString();
        }

        public IEnumerable<LookupDto> ConvertToLookupCollectionDto(IEnumerable collection)
        {
            foreach (var item in collection)
            {
                if (item is BaseObject baseObject)
                    yield return ConvertToLookupDto(baseObject);
            }
        }

        private LookupDto ConvertToLookupDto(BaseObject baseObject)
        {
            return new LookupDto()
            {
                Id = baseObject.Id,
                Title = baseObject.GetDisplayName()
            };
        }

        public Task SetStringValue<T>(T instance, PropertyInfo propertyInfo, string updateFieldValue,
            CancellationToken cancellationToken) where T : BaseObject
        {
            if (updateFieldValue == null)
            {
                propertyInfo.SetValue(instance, null);
                return Task.CompletedTask;
            }

            if (propertyInfo.PropertyType == typeof(string))
            {
                propertyInfo.SetValue(instance, updateFieldValue);
                return Task.CompletedTask;
            }

            if (propertyInfo.PropertyType.IsValueType && !propertyInfo.PropertyType.IsGenericType)
            {
                propertyInfo.SetValue(instance, ConvertFromString(updateFieldValue, propertyInfo.PropertyType));
                return Task.CompletedTask;
            }
            
            if (propertyInfo.PropertyType.IsGenericType &&
                propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var fieldType = propertyInfo.PropertyType.GenericTypeArguments[0];
                propertyInfo.SetValue(instance, ConvertFromString(updateFieldValue, fieldType));
                return Task.CompletedTask;
            }

            if (typeof(BaseObject).IsAssignableFrom(propertyInfo.PropertyType))
            {
                return SetLookupItemMethodInfoProvider.GetGenericMethod(propertyInfo.PropertyType)
                    .Invoke(this,
                        new object[] {instance, propertyInfo, Guid.Parse(updateFieldValue), cancellationToken}) as Task;
            }

            if (propertyInfo.PropertyType.IsGenericType &&
                propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                var propertyType = propertyInfo.PropertyType.GenericTypeArguments.First();
                return SetLookupItemCollectionMethodInfoProvider.GetGenericMethod(propertyType)
                    .Invoke(this,
                        new object[]
                        {
                            instance, propertyInfo, updateFieldValue.Split(",").Select(Guid.Parse).ToArray(),
                            cancellationToken
                        }) as Task;
            }

            return Task.CompletedTask;
        }

        private object ConvertFromString(string value, Type type)
        {
            if (type == typeof(int))
                return DateTime.Parse(value);
            
            if (type == typeof(float))
                return float.Parse(value);
            
            if (type == typeof(double))
                return double.Parse(value);
            
            if (type == typeof(DateTime))
                return DateTime.Parse(value);
            
            if (type == typeof(bool))
                return bool.Parse(value);

            return value;
        }

        private async Task SetLookupItem<T>(object instance, PropertyInfo property, Guid lookupItemId,
            CancellationToken cancellationToken) where T : BaseObject
        {
            var lookupItem = await _dataContext.Set<T>()
                .FirstOrDefaultAsync(a => a.Id == lookupItemId && !a.IsDeleted, cancellationToken);
            if (lookupItem == null)
                throw new NotFoundException(lookupItemId, typeof(T));

            property.SetValue(instance, lookupItem);
        }

        private async Task SetLookupItemCollection<T>(object instance, PropertyInfo property, Guid[] lookupItemIds,
            CancellationToken cancellationToken) where T : BaseObject
        {
            var lookupItems = await _dataContext.Set<T>().Where(a => lookupItemIds.Contains(a.Id) && !a.IsDeleted)
                .ToArrayAsync(cancellationToken);

            property.SetValue(instance, lookupItems.ToHashSet());
        }
    }
}