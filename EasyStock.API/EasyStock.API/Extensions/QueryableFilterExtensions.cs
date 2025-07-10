using EasyStock.API.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace EasyStock.API.Extensions
{
    public static class QueryableFilterExtensions
    {
        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, List<FilterCondition> filters)
        {
            if (filters == null || filters.Count == 0)
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? finalExpr = null;

            foreach (var filter in filters)
            {
                var property = Expression.Property(parameter, filter.Field);
                var propertyType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;
                var typedValue = Convert.ChangeType(filter.Value, propertyType);

                var constant = Expression.Constant(typedValue, property.Type);

                Expression? comparison;

                if (propertyType == typeof(string))
                {
                    MethodInfo? method = filter.Operator.ToLower() switch
                    {
                        "contains" => typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                        "startswith" => typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                        "endswith" => typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
                        "=" => typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                        _ => throw new NotSupportedException($"Operator '{filter.Operator} is not supported for strings.")
                    };

                    comparison = Expression.Call(property, method!, constant);
                }
                else if (propertyType == typeof(bool))
                {
                    switch (filter.Operator)
                    {
                        case "true":
                            comparison = Expression.Equal(property, constant);
                            break;
                        case "false":
                            comparison = Expression.NotEqual(property, constant);
                            break;
                        case "any":
                            comparison = null;
                            break;
                        default:
                            throw new NotSupportedException($"'{filter.Operator}' operator not supported for boolean fields.");
                    }
                }
                else
                {
                    comparison = filter.Operator switch
                    {
                        "=" => Expression.Equal(property, constant),
                        ">" => Expression.GreaterThan(property, constant),
                        ">=" => Expression.GreaterThanOrEqual(property, constant),
                        "<" => Expression.LessThan(property, constant),
                        "<=" => Expression.LessThanOrEqual(property, constant),
                        _ => throw new NotSupportedException($"Operator '{filter.Operator}' is not supported.")
                    };
                }

                if (comparison != null)
                    finalExpr = finalExpr == null ? comparison : Expression.AndAlso(finalExpr, comparison);
            }

            if (finalExpr == null)
                return query;

            var lambda = Expression.Lambda<Func<T, bool>>(finalExpr, parameter);
            return query.Where(lambda);
        }
    }
}
