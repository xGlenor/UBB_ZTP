using System.Runtime.InteropServices;

namespace ImageProcessing.CUDA;

internal static class NativeMethods
{
    [DllImport("ImageProcCuda.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ProcessImage(
        IntPtr imageData, int width, int height, int channels,
        [MarshalAs(UnmanagedType.LPStr)] string filterName,
        out double gpuTimeMs
    );
}