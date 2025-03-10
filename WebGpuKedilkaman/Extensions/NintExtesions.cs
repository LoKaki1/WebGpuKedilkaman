using System.Runtime.InteropServices;

namespace WebGpuKedilkaman.Extensions
{
    public static unsafe class NintExtesions
    {
        public static string ToAnsiString(this nint buffer)
        {
            var data = Marshal.PtrToStringAnsi(buffer);

            return data ?? string.Empty;
        }
    }
}
