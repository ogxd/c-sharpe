using CSharpe.Marshalling;
using CSharpe.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CSharpe.Marshalling
{
    public unsafe abstract class Marshaller<O> where O : new()
    {
        internal string _fieldName;

        public abstract void BuildSetter(FieldInfo fieldInfo);

        public abstract void ConvertSet(ref byte* ptr, ref O o, Dictionary<string, int> sizes);
    }

    public unsafe abstract class Marshaller<O, T> : Marshaller<O> where O : new()
    {
        //internal Action<object, T> _setter;
        internal ActionRef<O, T> _setter;

        public override void BuildSetter(FieldInfo fieldInfo)
        {
            //_setter = Magic2.CompileSetter<T>(fieldInfo);
            _setter = ReflectionUtils.BuildUntypedSetter<O, T>(fieldInfo);
        }
    }

    public unsafe class MarshallerBlittable<O, T> : Marshaller<O, T> where T : unmanaged where O : new()
    {
        public override void ConvertSet(ref byte* ptr, ref O o, Dictionary<string, int> sizes)
        {
            T* src = (T*)ptr;
            T result = src[0];
            //T result = Marshal.PtrToStructure<T>(new IntPtr(ptr)); // slow
            ptr = &ptr[sizeof(T)];
            _setter(ref o, result);
        }
    }

    public unsafe class MarshallerBlittableSize<O> : Marshaller<O, uint> where O : new()
    {
        public override void ConvertSet(ref byte* ptr, ref O o, Dictionary<string, int> sizes)
        {
            uint* src = (uint*)ptr;
            uint size = src[0];
            //uint size = Marshal.PtrToStructure<uint>(new IntPtr(ptr)); // slow
            ptr = &ptr[sizeof(uint)];
            sizes.Add(_fieldName, (int)size);
            AutoMarshal2.TMP_SIZE = (int)size;
            _setter(ref o, size);
        }
    }

    public unsafe class MarshallerBlittableArray<O, T> : Marshaller<O, T[]> where T : unmanaged where O : new()
    {
        public override void ConvertSet(ref byte* ptr, ref O o, Dictionary<string, int> sizes)
        {
            T[] result = MarshalUtils.CopyArray_PtrIterate<T>(ref ptr, AutoMarshal2.TMP_SIZE);
            _setter(ref o, result);
        }
    }

    public class AutoMarshal2Attribute : Attribute
    {
        public readonly Type _type;

        public AutoMarshal2Attribute(Type type)
        {
            _type = type;
        }
    }

    public unsafe class MarshallerObj<O> where O : new()
    {
        private static Dictionary<Type, List<Marshaller<O>>> _cache = new Dictionary<Type, List<Marshaller<O>>>();

        public O Convert(ref byte* ptr)
        {
            Type type = typeof(O);
            O nobj = new O();

            if (!_cache.ContainsKey(type))
            {
                var list = new List<Marshaller<O>>();
                _cache.Add(type, list);
                foreach (var field in type.GetFields())
                {
                    AutoMarshal2Attribute k = field.GetCustomAttribute<AutoMarshal2Attribute>(true);
                    Marshaller<O> marshaller = (Marshaller<O>)Activator.CreateInstance(k._type);
                    marshaller._fieldName = field.Name;
                    marshaller.BuildSetter(field);
                    list.Add(marshaller);
                }
            }

            var sizes = new Dictionary<string, int>();
            foreach (Marshaller<O> field in _cache[type])
            {
                field.ConvertSet(ref ptr, ref nobj, sizes);
            }

            return nobj;
        }
    }

    public static class AutoMarshal2
    {
        public static int TMP_SIZE; // Todo : clean this thing...
    }

    public unsafe static class AutoMarshal2<O> where O : new()
    {
        private static Dictionary<Type, MarshallerObj<O>> _cache = new Dictionary<Type, MarshallerObj<O>>();

        public static O Convert(IntPtr ptr)
        {
            byte* p = (byte*)ptr.ToPointer();
            Type type = typeof(O);

            if (!_cache.ContainsKey(type))
            {
                var attr = type.GetCustomAttribute<AutoMarshal2Attribute>(true);
                MarshallerObj<O> marshaller = (MarshallerObj<O>)Activator.CreateInstance(attr._type);
                _cache.Add(type, marshaller);
            }

            return _cache[type].Convert(ref p);
        }
    }
}