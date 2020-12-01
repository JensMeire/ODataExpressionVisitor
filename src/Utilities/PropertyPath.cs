using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Utilities
{
    public static class PropertyPath<TSource>
    {
        public static Expression Get<TResult>(Expression<Func<TSource, TResult>> path,
            Expression parameterExpression)
        {
            var pathParts = Get(path);
            foreach (var part in pathParts)
                parameterExpression = Expression.PropertyOrField(parameterExpression, part.Name);
            return parameterExpression;
        }
        
        public static IReadOnlyList<MemberInfo> Get<TResult>(Expression<Func<TSource, TResult>> expression)
        {
            var visitor = new PropertyVisitor();
            visitor.Visit(expression.Body);
            visitor.Path.Reverse();
            return visitor.Path;
        }

        private class PropertyVisitor : ExpressionVisitor
        {
            internal readonly List<MemberInfo> Path = new List<MemberInfo>();

            protected override Expression VisitMember(MemberExpression node)
            {
                if (!(node.Member is PropertyInfo))
                {
                    throw new ArgumentException("The path can only contain properties", nameof(node));
                }

                this.Path.Add(node.Member);
                return base.VisitMember(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if(!(node.Value is string path))throw new ArgumentException("The path can only be a path of properties", nameof(node));
                var parts = path.Split('.');
                if (parts.Length > 1) throw new NotImplementedException("Nested path as string not supported");
                Path.Add(typeof(TSource).GetMember(parts[0])[0]);
                return base.VisitConstant(node);
            }
        }
    }}