using System;
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
                var typedValue = propertyType == typeof(DateTime) ? "" : Convert.ChangeType(filter.Value, propertyType);

                var constant = Expression.Constant(typedValue, property.Type);

                Expression? comparison;

                if (propertyType == typeof(string))
                {
                    var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

                    var loweredProperty = Expression.Call(property, toLowerMethod!);

                    var loweredFilterValue = filter.Value?.ToString()?.ToLower() ?? string.Empty;

                    MethodInfo? method;

                    if (string.IsNullOrEmpty(loweredFilterValue))
                    {
                        if (filter.Operator == "=" || filter.Operator.ToLower() == "equals")
                        {
                            // property == null
                            comparison = Expression.Equal(property, Expression.Constant(null, property.Type));
                        }
                        else if (filter.Operator == "!=" || filter.Operator.ToLower() == "notequals" || filter.Operator == "<>")
                        {
                            // property != null
                            comparison = Expression.NotEqual(property, Expression.Constant(null, property.Type));
                        }
                        else
                        {
                            // fallback, treat empty string literally
                            var loweredConstant = Expression.Constant(loweredFilterValue);
                            method = typeof(string).GetMethod("Equals", new[] { typeof(string) })!;
                            var equalsCall = Expression.Call(loweredProperty, method, loweredConstant);
                            comparison = equalsCall;
                        }
                    }
                    else
                    {
                        var loweredConstant = Expression.Constant(loweredFilterValue);
                        method = filter.Operator.ToLower() switch
                        {
                            "contains" => typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                            "startswith" => typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                            "endswith" => typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
                            "equals" or "=" => typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                            "notequals" or "!=" or "<>" => typeof(string).GetMethod("Equals", new[] { typeof(string) }),
                            _ => throw new NotSupportedException($"Operator '{filter.Operator} is not supported for strings.")
                        };

                        if (filter.Operator.ToLower() == "notequals" || filter.Operator == "<>" || filter.Operator == "!=")
                        {
                            var equalsCall = Expression.Call(loweredProperty, method!, loweredConstant);
                            comparison = Expression.Not(equalsCall);
                        }
                        else
                        {
                            comparison = Expression.Call(loweredProperty, method!, loweredConstant);
                        }

                    }
                }
                else if (propertyType == typeof(bool))
                {
                    switch (filter.Operator.ToLower())
                    {
                        case "true":
                            comparison = Expression.Equal(property, Expression.Constant(true));
                            break;
                        case "false":
                            comparison = Expression.NotEqual(property, Expression.Constant(true));
                            break;
                        case "any":
                        case "all":
                            comparison = null;
                            break;
                        default:
                            throw new NotSupportedException($"'{filter.Operator}' operator not supported for boolean fields.");
                    }
                }
                else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                {

                    var dateOnlyProperty = Expression.Property(property, nameof(DateTime.Date));

                    if (!DateTime.TryParse(filter.Value?.ToString(), out var parsedDate))
                        throw new ArgumentException($"Invalid date value: {filter.Value}");

                    var dateOnlyUtc = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);

                    var dateConstant = Expression.Constant(dateOnlyUtc, typeof(DateTime));

                    var specifyKindMethod = typeof(DateTime).GetMethod(nameof(DateTime.SpecifyKind), new[] { typeof(DateTime), typeof(DateTimeKind) })!;

                    var dateOnlyPropertyUtc = Expression.Call(
                        specifyKindMethod,
                        dateOnlyProperty,
                        Expression.Constant(DateTimeKind.Utc)
                    );

                    // Now build the comparison with normalized UTC dates
                    var op = filter.Operator.Trim().ToLowerInvariant();

                    comparison = op switch
                    {
                        "equals" or "=" => Expression.Equal(dateOnlyPropertyUtc, dateConstant),
                        "notequals" or "!=" or "<>" => Expression.NotEqual(dateOnlyPropertyUtc, dateConstant),
                        "greaterthan" or ">" or "gt" => Expression.GreaterThan(dateOnlyPropertyUtc, dateConstant),
                        "greaterthanorequal" or ">=" or "gte" => Expression.GreaterThanOrEqual(dateOnlyPropertyUtc, dateConstant),
                        "lessthan" or "<" or "lt" => Expression.LessThan(dateOnlyPropertyUtc, dateConstant),
                        "lessthanorequal" or "<=" or "lte" => Expression.LessThanOrEqual(dateOnlyPropertyUtc, dateConstant),
                        _ => throw new NotSupportedException($"Operator '{filter.Operator}' is not supported for dates.")
                    };
                }
                else
                {
                    comparison = filter.Operator.ToLower() switch
                    {
                        "equals" or "=" => Expression.Equal(property, constant),
                        "greaterthan" or ">" => Expression.GreaterThan(property, constant),
                        "greaterthanorequal" or ">=" => Expression.GreaterThanOrEqual(property, constant),
                        "lessthan" or "<" => Expression.LessThan(property, constant),
                        "lessthanorequal" or "<=" => Expression.LessThanOrEqual(property, constant),
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
