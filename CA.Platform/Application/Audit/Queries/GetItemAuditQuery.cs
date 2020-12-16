using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using MediatR;

namespace CA.Platform.Application.Audit.Queries
{
    public class GetItemAuditQuery: IRequest<List<AuditRecordDto>>
    {
        public Guid ItemId { get; set; }

        public class GetItemAuditQueryHandler: IRequestHandler<GetItemAuditQuery, List<AuditRecordDto>>
        {
            private readonly IDbContext _dbContext;
            private readonly IMapper _mapper;

            public GetItemAuditQueryHandler(IDbContext dbContext, IMapper mapper)
            {
                _dbContext = dbContext;
                _mapper = mapper;
            }
            
            public Task<List<AuditRecordDto>> Handle(GetItemAuditQuery request, CancellationToken cancellationToken)
            {
                var records = _dbContext.GetDbSet<AuditRecord>().Where(t => t.ObjectId == request.ItemId).ToList();
                return Task.FromResult(records.Select(a=> _mapper.Map<AuditRecordDto>(a)).ToList());
            }
        }
    }
}