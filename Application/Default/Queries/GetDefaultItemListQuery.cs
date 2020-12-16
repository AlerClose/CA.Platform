using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CA.Platform.Application.Common;
using CA.Platform.Application.Contracts;
using CA.Platform.Application.Default.Commands;
using CA.Platform.Application.Interfaces;
using CA.Platform.Entities;
using MediatR;

namespace CA.Platform.Application.Default.Queries
{
    public class GetDefaultItemListQuery : IRequest<List<LookupDto>>
    {
        public int EntityTypeId { get; set; }

        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public List<UpdateFieldDto> FilterFields { get; set; }

        public class GetDefaultItemListQueryHandler: IRequestHandler<GetDefaultItemListQuery, List<LookupDto>>
        {
            private readonly IDbContext _dbContext;
            private readonly IEntityService _entityService;
            
            private static readonly GenericMethodInfoProvider<GetDefaultItemListQueryHandler> GetItemsMethodProvider = new GenericMethodInfoProvider<GetDefaultItemListQueryHandler>(nameof(GetItems));

            public GetDefaultItemListQueryHandler(IDbContext dbContext, IEntityService entityService)
            {
                _dbContext = dbContext;
                _entityService = entityService;
            }
            
            public Task<List<LookupDto>> Handle(GetDefaultItemListQuery request, CancellationToken cancellationToken)
            {
                var result = GetItemsMethodProvider.GetGenericMethod(_entityService.GetEntityTypeById(request.EntityTypeId))
                    .Invoke(this, null) as List<LookupDto>;
                
                return Task.FromResult(result);
            }

            private List<LookupDto> GetItems<T>(GetDefaultItemListQuery request) where T: BaseObject
            {
                var items = _dbContext.GetDbSet<T>()
                    .Where(t => !t.IsDeleted)
                    .Where(GetFilterExpression<T>(request.FilterFields))
                    .Skip(request.Page * request.PageSize)
                    .Take(request.PageSize)
                    .ToArray();

                return _entityService.ConvertToLookupDto(items).ToList();
            }

            private Expression<Func<T, bool>> GetFilterExpression<T>(List<UpdateFieldDto> fields)
            {
                if (fields.Count == 0)
                    return arg => true;
                
                List<BinaryExpression> fieldsExpresion = fields.Select(ConvertUpdateFieldsToBinaryExpression<T>).ToList();
                var resultBinaryExpression = JoinExpressionsByAnd<T>(fieldsExpresion);
                
                return Expression.Lambda<Func<T, bool>>(resultBinaryExpression);
            }

            private BinaryExpression ConvertUpdateFieldsToBinaryExpression<T>(UpdateFieldDto fieldDto)
            {
                var fieldType = _entityService.GetEntityFieldPropertyById(fieldDto.FieldId);

                var parameterExpression = Expression.Parameter(typeof(T));

                var leftPartExpression =
                    Expression.Property(parameterExpression, typeof(T).GetProperty(fieldType.Name));
                
                if (fieldType.PropertyType == typeof(string))
                    return Expression.Equal(leftPartExpression, Expression.Constant(fieldDto.Value));
                
                if (fieldType.PropertyType == typeof(int))
                    return Expression.Equal(leftPartExpression, Expression.Constant(int.Parse(fieldDto.Value)));
                
                if (fieldType.PropertyType == typeof(DateTime))
                    return Expression.Equal(leftPartExpression, Expression.Constant(DateTime.Parse(fieldDto.Value)));
                
                if (fieldType.PropertyType == typeof(float))
                    return Expression.Equal(leftPartExpression, Expression.Constant(float.Parse(fieldDto.Value)));
                
                throw new NotSupportedException();
            }

            private BinaryExpression JoinExpressionsByAnd<T>(List<BinaryExpression> fieldsExpresion)
            {
                var baseExpression = fieldsExpresion[0];
                if (fieldsExpresion.Count == 1)
                    return baseExpression;

                for (var i = 1; i < fieldsExpresion.Count; i++)
                {
                    baseExpression = Expression.And(baseExpression, fieldsExpresion[i]);
                }

                return baseExpression;
            }
        }
    }
}