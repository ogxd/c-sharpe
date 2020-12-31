using System.Linq.Expressions;
using System.Reflection;

namespace CSharpe.Reflection
{
    public delegate void ActionRef<T1, T2>(ref T1 arg1, T2 arg2);

    public static class ReflectionUtils
    {
        public static ActionRef<TContainer, TField> BuildUntypedSetter<TContainer, TField>(FieldInfo fieldInfo)
        {
            var targetType = fieldInfo.DeclaringType;
            var exInstance = Expression.Parameter(targetType.MakeByRefType(), "t");

            var exMemberAccess = Expression.MakeMemberAccess(exInstance, fieldInfo);

            var exValue = Expression.Parameter(typeof(TField), "p");
            var exConvertedValue = Expression.Convert(exValue, fieldInfo.FieldType);
            var exBody = Expression.Assign(exMemberAccess, exConvertedValue);

            var lambda = Expression.Lambda<ActionRef<TContainer, TField>>(exBody, exInstance, exValue);
            var action = lambda.Compile();
            return action;
        }
    }
}