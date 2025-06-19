using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessing.CUDA;

internal class Program
{
    private static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\nImage Processing with CUDA");
            Console.Write("Enter image path: ");
            var inPath = Console.ReadLine();

            Console.WriteLine("\nAvailable operations:");
            Console.WriteLine("1. Grayscale");
            Console.WriteLine("2. Blur");
            Console.Write("Select operation: ");

            var filter = Console.ReadLine().ToLower();

            var outPath = Path.GetFileNameWithoutExtension(inPath)
                          + "_" + filter + Path.GetExtension(inPath);

            // 1. Wczytaj obraz jako bitmapa
            var bmp = new Bitmap(inPath);
            int w = bmp.Width, h = bmp.Height;
            var rect = new Rectangle(0, 0, w, h);
            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var bytes = Math.Abs(bmpData.Stride) * h;

            // 2. Przekaż wskaźnik i rozmiary
            var sw = Stopwatch.StartNew();
            bool ok;
            double gpuTime;
            ok = NativeMethods.ProcessImage(
                bmpData.Scan0, w, h, 3, filter, out gpuTime
            );
            sw.Stop();
            bmp.UnlockBits(bmpData);

            if (!ok)
            {
                Console.WriteLine("Processing failed.");
                return;
            }
            Console.WriteLine($"Total round-trip time: {sw.Elapsed.TotalMilliseconds} ms");
            Console.WriteLine($"GPU kernel time:         {gpuTime:F2} ms");

            // 3. Załaduj z powrotem dane i zapisz
            var outBmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            var outData = outBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                var srcPtr = (byte*)bmpData.Scan0;
                var dstPtr = (byte*)outData.Scan0;
                long byteCount = bytes; // rzutujemy na long
                Buffer.MemoryCopy(srcPtr, dstPtr, byteCount, byteCount);
            }

            outBmp.UnlockBits(outData);
            outBmp.Save(outPath);
            Console.WriteLine($"Saved result to {outPath}");
        }
    }
}