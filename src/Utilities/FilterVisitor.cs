using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Utilities
{
  public class FilterVisitor<TAggregate, TColumn>: QueryNodeVisitor<Expression> 
        where TAggregate: class  
        where TColumn: struct, Enum 
    {
        private readonly Func<Type, MemberInfo, TColumn?> _getColumn;
        private readonly Func<TColumn?, Expression<Func<TAggregate, object>>> _getProperty;
        private readonly ParameterExpression _parameter;

        public FilterVisitor(Func<Type, MemberInfo, TColumn?> getColumn, Func<TColumn?, Expression<Func<TAggregate, object>>> getProperty)
        {
            _getColumn = getColumn;
            _getProperty = getProperty;
            _parameter = Expression.Parameter(typeof(TAggregate), typeof(TAggregate).Name.ToLower());
        }

        public ParameterExpression GetParameterExpression() =>
            _parameter;

        public override Expression Visit(BinaryOperatorNode nodeIn)
        {
            var left = nodeIn.Left.Accept(this);
            var right = nodeIn.Right.Accept(this);
            var expression = GetOperatorExpression(nodeIn.OperatorKind, left, right);
            return expression;
        }

        public override Expression Visit(ConstantNode nodeIn)
        {
            return Expression.Constant(nodeIn.Value);
        }

        public override Expression Visit(SingleValuePropertyAccessNode nodeIn)
        {
            return GetPropertyExpression(nodeIn.Property);
        }

        public override Expression Visit(SingleValueFunctionCallNode nodeIn)
        {
            return nodeIn.Name.ToLowerInvariant() switch
            {
                "contains" => CreateContainsExpression(nodeIn),
                "startswith" => CreateStartsWithExpression(nodeIn),
                "endswith" => CreateEndsWithExpression(nodeIn),
                _ => throw new NotImplementedException()
            };
        }

        private Expression GetOperatorExpression(BinaryOperatorKind operatorKind, Expression left, Expression right)
        {
            return operatorKind switch
            {
                BinaryOperatorKind.Equal => Expression.Equal(left, right),
                BinaryOperatorKind.NotEqual => Expression.NotEqual(left, right),
                BinaryOperatorKind.GreaterThan => Expression.GreaterThan(left, right),
                BinaryOperatorKind.GreaterThanOrEqual => Expression.GreaterThanOrEqual(left, right),
                BinaryOperatorKind.LessThan => Expression.LessThan(left, right),
                BinaryOperatorKind.LessThanOrEqual => Expression.LessThanOrEqual(left, right),
                BinaryOperatorKind.And => Expression.AndAlso(left, right),
                BinaryOperatorKind.Or => Expression.OrElse(left, right),
                _ => throw new NotImplementedException()
            };
        }

        private Expression CreateContainsExpression(SingleValueFunctionCallNode nodeIn)
        {
            var (property, constant) = GetExpressionForm2ParameterFunction(nodeIn.Parameters, "Contains");
            var contains = typeof(string).GetMethod("Contains", new[] {typeof(string)});
            return Expression.Call(property, contains, constant);
        }

        private Expression CreateStartsWithExpression(SingleValueFunctionCallNode nodeIn)
        {
            var (property, constant) = GetExpressionForm2ParameterFunction(nodeIn.Parameters, "Contains");
            var startsWith = typeof(string).GetMethod("StartsWith", new[] {typeof(string)});
            return Expression.Call(property, startsWith, constant);
        }

        private Expression CreateEndsWithExpression(SingleValueFunctionCallNode nodeIn)
        {
            var (property, constant) = GetExpressionForm2ParameterFunction(nodeIn.Parameters, "Contains");
            var endsWith = typeof(string).GetMethod("EndsWith", new[] {typeof(string)});
            return Expression.Call(property, endsWith, constant);
        }

        private (Expression, Expression) GetExpressionForm2ParameterFunction(IEnumerable<QueryNode> parameters,
            string function)
        {
            var parametersList = parameters.ToList();
            if (parametersList.Count != 2)
                throw new UriFormatException($"'{function}'' function needs 2 parameters");

            if (!(parametersList[0] is SingleValuePropertyAccessNode property))
                throw new UriFormatException($"'{function}' function first parameter cannot be NULL");

            if (!(parametersList[1] is ConstantNode constant))
                throw new UriFormatException($"'{function}' function second parameter cannot be NULL");
            return (property.Accept(this), constant.Accept(this));
        }

        public override Expression Visit(InNode nodeIn)
        {
            var property = nodeIn.Left.Accept(this);
            var values = nodeIn.Right.Accept(this);
            var contains = typeof(List<object>).GetMethod("Contains", new[] {typeof(List<object>)});
            return Expression.Call(values, contains, property);
        }

        public override Expression Visit(CollectionConstantNode nodeIn)
        {
            return Expression.Constant(nodeIn.Collection.Select(x => x.Value).ToList());
        }

        private Expression GetPropertyExpression(IEdmProperty property)
        {
            if (property?.DeclaringType == null)
                throw new UriFormatException("No property given");

            var type = GetType(property.DeclaringType.ToString());
            if (type == null)
                throw new UriFormatException("Unknown property");

            var member = type.GetMember(property.Name).FirstOrDefault();
            if (member == null)
                throw new UriFormatException("Unknown property");

            var col = _getColumn(type, member);
            if (col == null)
                throw new NotImplementedException("No column mapping");

            var func = _getProperty(col);
            if(func == null)
                throw new NotImplementedException("No column mapping");

            return PropertyPath<TAggregate>.Get(func, GetParameterExpression());
        }
        
        private Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}