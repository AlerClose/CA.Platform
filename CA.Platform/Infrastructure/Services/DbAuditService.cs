using System;
using System.Collections.Generic;
using System.Threading;
using CA.Platform.Entities;
using CA.Platform.Infrastructure.DataBase;
using CA.Platform.Infrastructure.Interfaces;

namespace CA.Platform.Infrastructure.Services
{
    class DbAuditService<TContext>: IAuditService where TContext: BaseDbContext
    {
        private readonly TContext _dbContext;

        public DbAuditService(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddRecordsAsync(IEnumerable<AuditRecord> records, CancellationToken cancellationToken)
        {
            _dbContext.AuditRecords.AddRange(records);
        }

        public bool NeedToWriteAudit(Type entityType)
        {
            return true;
        }

        public string GetFieldValue(object value)
        {
            if (value == null)
                return null;

            var type = value.GetType();

            if (!type.IsClass)
                return value.ToString();

            if (value is BaseObject baseObject)
                return baseObject.Id.ToString();

            return value.ToString();
        }
    }
}