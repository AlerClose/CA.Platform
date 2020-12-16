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
    public class GetEntitiesListQuery: IRequest<List<EntityTypeDto>>
    {
        public class GetEntitiesListQueryHandler: IRequestHandler<GetEntitiesListQuery, List<EntityTypeDto>>
        {
            private readonly IDbContext _dbContext;
            private readonly IMapper _mapper;

            public GetEntitiesListQueryHandler(IDbContext dbContext, IMapper mapper)
            {
                _dbContext = dbContext;
                _mapper = mapper;
            }
            
            public Task<List<EntityTypeDto>> Handle(GetEntitiesListQuery request, CancellationToken cancellationToken)
            {
                var entities = _dbContext.GetDbSet<EntityType>().Where(t=> !t.IsDeleted).ToArray();
                return Task.FromResult(entities.Select(a => _mapper.Map<EntityTypeDto>(a)).ToList());
            }
        }
    }
}