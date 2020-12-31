using CSharpe.Marshalling;
using CSharpe.Reflection;
using CSharpe.Types;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CSharpe.Marshalling
{
    public class AutoMarshal1Attribute : Attribute
    {

    }

    public class AutoMarshal1ArrayAttribute : AutoMarshal1Attribute
    {
        public readonly string arraySizeProperty;

        public AutoMarshal1ArrayAttribute(string arraySizeProperty)
        {
            this.arraySizeProperty = arraySizeProperty;
        }
    }

    public class AutoMarshal1ArraySizeAttribute : AutoMarshal1Attribute
    {
        public AutoMarshal1ArraySizeAttribute()
        {
        }
    }

    public unsafe static class AutoMarshal1
    {
        private static Dictionary<Type, List<SetterHold>> _cache = new Dictionary<Type, List<SetterHold>>();

        public static T Convert<T>(IntPtr ptr)
        {
            byte* p = (byte*)ptr.ToPointer();
            return (T)Convert(ref p, typeof(T));
        }

        public class SetterHold
        {
            public DynamicInvoke.DynamicInvoker invoker;
            public AutoMarshal1Attribute attr;
            public FieldInfo field;
        }

        public static object Convert(ref byte* ptr, Type type)
        {
            if (TypeUtils.IsUnManaged(type))
            {
                return Marshal.PtrToStructure(new IntPtr(ptr), type);
            }
            else
            {
                object nobj = Activator.CreateInstance(type);

                if (!_cache.ContainsKey(type))
                {
                    var list = new List<SetterHold>();
                    _cache.Add(type, list);
                    foreach (var field in type.GetFields())
                    {
                        SetterHold kap = new SetterHold
                        {
                            invoker = DynamicInvoke.BuildFieldSetter(field),
                            attr = field.GetCustomAttribute<AutoMarshal1Attribute>(true),
                            field = field
                        };
                        list.Add(kap);
                    }
                }

                Dictionary<string, uint> sizes = new Dictionary<string, uint>();

                foreach (var field in _cache[type])
                {
                    switch (field.attr)
                    {
                        case AutoMarshal1ArraySizeAttribute arraySizeAttribute:
                            uint size = (uint)Convert(ref ptr, field.field.FieldType);
                            sizes.Add(field.field.Name, size);
                            field.invoker(nobj, size);
                            break;
                        case AutoMarshal1ArrayAttribute arrayAttribute:
                            Type elementType = field.field.FieldType.GetElementType();
                            object array = MarshalUtils.CopyArray_BlockCopy(ref ptr, elementType, (uint)sizes[arrayAttribute.arraySizeProperty]);
                            field.invoker(nobj, array);
                            break;
                        default:
                            field.invoker(nobj, Convert(ref ptr, field.field.FieldType));
                            break;
                    }
                }
                return nobj;
            }
        }
    }
}