using System;

namespace CSharpe.Marshalling
{
    public unsafe static class AutoMarshal3
    {
        public static object Convert(ref byte* ptr, Type type)
        {
            // Use generic attributes to do the marshalling with generics https://github.com/dotnet/csharplang/issues/124
            // Will be faster and more pratical !
            throw new NotImplementedException();
        }
    }
}