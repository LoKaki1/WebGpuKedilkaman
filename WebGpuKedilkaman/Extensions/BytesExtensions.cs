using System.Runtime.InteropServices;

namespace WebGpuKedilkaman.Extensions
{
    public static unsafe class BytesExtensions
    {

        public static string ToString(byte* message)
        {
            return Marshal.PtrToStringAnsi((nint)message) ?? string.Empty;
        }
    }
}
