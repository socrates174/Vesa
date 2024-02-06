using System.Linq.Expressions;

namespace IoCloud.Shared.Querying.Sql.Abstractions
{
    public class OrderByExpression<TEntity>
          where TEntity : class
    {

        public OrderByExpression()
        {
        }

        public OrderByExpression(Expression<Func<TEntity, object>> expression, bool isDescending = false)
        {
            Expression = expression;
            IsDescending = isDescending;
        }

        public Expression<Func<TEntity, object>> Expression { get; set; }
        public bool IsDescending { get; set; }
    }
}
