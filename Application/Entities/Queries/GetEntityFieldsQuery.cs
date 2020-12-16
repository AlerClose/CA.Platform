using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using MediatR;

namespace CA.Platform.Application.Entities.Queries
{
    public class GetEntityFieldsQuery: IRequest<List<EntityFieldDto>>
    {
        public int EntityId { get; set; }
        
        public class GetEntityFieldsQueryHandler: IRequestHandler<GetEntityFieldsQuery, List<EntityFieldDto>>
        {
            private readonly IMapper _mapper;
            private readonly IDbContext _dbContext;

            public GetEntityFieldsQueryHandler(IMapper mapper, IDbContext dbContext)
            {
                _mapper = mapper;
                _dbContext = dbContext;
            }
            
            public Task<List<EntityFieldDto>> Handle(GetEntityFieldsQuery request, CancellationToken cancellationToken)
            {
                var fields = _dbContext.GetDbSet<EntityField>().Where(t => t.EntityType.Id == request.EntityId && !t.IsDeleted)
                    .ToArray();
                
                return Task.FromResult(fields.Select(a => _mapper.Map<EntityFieldDto>(a)).ToList());
            }
        }
    }
}