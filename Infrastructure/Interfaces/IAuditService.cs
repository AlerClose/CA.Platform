using System;
using System.Collections.Generic;
using System.Threading;
using CA.Platform.Entities;

namespace CA.Platform.Infrastructure.Interfaces
{
    public interface IAuditService
    {
        void WriteRecordsAsync(IEnumerable<AuditRecord> records, CancellationToken cancellationToken);

        bool NeedToWriteAudit(Type entityType);

        string GetFieldValue(object value);
    }
}