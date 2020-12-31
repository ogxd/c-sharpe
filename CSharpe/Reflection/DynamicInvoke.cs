using System.Linq.Expressions;
using System.Reflection;

namespace CSharpe.Reflection
{
    public static class DynamicInvoke
    {
        public delegate object? DynamicInvoker(object? target, params object?[] args);

        private static Expression BuildFieldSetter(MemberExpression field, ParameterExpression arguments)
        {
            Expression valueArg = Expression.ArrayIndex(arguments, Expression.Constant(0));
            if (valueArg.Type != field.Type)
                valueArg = Expression.Convert(valueArg, field.Type);

            Expression body = Expression.Assign(field, valueArg);
            return Expression.Block(typeof(object), body, Expression.Default(typeof(object)));
        }

        public static DynamicInvoker BuildFieldSetter(FieldInfo field)
        {
            var target = Expression.Parameter(typeof(object));
            var arguments = Expression.Parameter(typeof(object[]));
            return Expression.Lambda<DynamicInvoker>(BuildFieldSetter(BuildFieldAccess(field, target), arguments), target, arguments).Compile();
        }

        private static MemberExpression BuildFieldAccess(FieldInfo field, ParameterExpression target)
        {
            Expression? owner;
            if (field.IsStatic)
                owner = null;
            else if (field.DeclaringType.IsValueType)
                owner = Expression.Unbox(target, field.DeclaringType);
            else
                owner = Expression.Convert(target, field.DeclaringType);

            return Expression.Field(owner, field);
        }
    }
}