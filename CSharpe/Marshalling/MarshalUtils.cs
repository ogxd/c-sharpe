using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CSharpe.Marshalling
{
    public unsafe static class MarshalUtils
    {
        public static unsafe T[] CopyArray_BlockCopy<T>(ref byte* ptr, uint length)
        {
            uint size = length * (uint)Marshal.SizeOf<T>();
            T[] array = new T[length];
            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            void* dst = handle.AddrOfPinnedObject().ToPointer();
            Unsafe.CopyBlock(dst, ptr, size);
            handle.Free();
            ptr = &ptr[size];
            return array;
        }

        public static unsafe Array CopyArray_BlockCopy(ref byte* ptr, Type elementType, uint length)
        {
            uint size = length * (uint)Marshal.SizeOf(elementType);
            var array = Array.CreateInstance(elementType, length);
            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            void* dst = handle.AddrOfPinnedObject().ToPointer();
            Unsafe.CopyBlock(dst, ptr, size);
            handle.Free();
            ptr = &ptr[size];
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] CopyArray_PtrIterate<T>(ref byte* ptr, int length) where T : unmanaged
        {
            int size = sizeof(T);
            T* itr = (T*)ptr;
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
                result[i] = itr[i];
            ptr = &ptr[size];
            return result;
        }
    }
}