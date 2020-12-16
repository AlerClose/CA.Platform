using System;
using System.Linq;
using System.Threading.Tasks;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Default.Commands;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using CA.Platform.Infrastructure.DataBase;
using CA.Platform.Infrastructure.Interfaces;
using CA.Platform.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CA.Platform.Tests
{
    public abstract class BaseEntityTest<TContext>: BaseTest<TContext> where TContext: BaseDbContext
    {
        protected IEntityService EntityService => ServiceProvider?.GetService<IEntityService>();

        protected override void AddEntityService(ServiceCollection services)
        {
            services.AddScoped<IEntityService, DbEntityService<TContext>>();
        }

        protected override void AddAuditService(ServiceCollection services)
        {
            services.AddScoped<IAuditService, DbAuditService<TContext>>();
        }

        protected UpdateFieldDto GetFieldDto<T>(object value, string fieldName) where T : BaseObject
        {
            var stringConvertService = ServiceProvider.GetService<StringConvertService<TContext>>();
            return new UpdateFieldDto()
            {
                Value = stringConvertService.GetStringValue(value),
                FieldId = EntityService.GetEntityFieldDto(typeof(T), fieldName).Id
            };
        }

        protected Task<ResultDto> CreateEntity<T>(params UpdateFieldDto [] fields) where T : BaseObject
        {
            var entityTypeId = EntityService.GetEntityTypeId(typeof(T));
            return Mediator.Send(new CreateDefaultItemCommand()
            {
                EntityTypeId = entityTypeId,
                FieldsToUpdate = fields.ToList()
            });
        }
        
        protected Task<ResultDto> UpdateEntity<T>(Guid itemId, params UpdateFieldDto [] fields) where T : BaseObject
        {
            var entityTypeId = EntityService.GetEntityTypeId(typeof(T));
            return Mediator.Send(new UpdateDefaultItemCommand()
            {
                ItemId = itemId,
                EntityTypeId = entityTypeId,
                FieldsToUpdate = fields.ToList()
            });
        }
    }
}