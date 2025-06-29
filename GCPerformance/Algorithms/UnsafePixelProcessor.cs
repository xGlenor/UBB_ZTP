namespace GCPerformance.Algorithms;

public unsafe class UnsafePixelProcessor : IPixelProcessor
{
    public void ProcessPixels(ReadOnlySpan<byte> input, Span<byte> output, int width, int height)
    {
        fixed (byte* inputPtr = input)
        fixed (byte* outputPtr = output)
        {
            ProcessWithPointers(inputPtr, outputPtr, width, height);
        }
    }

    private static unsafe void ProcessWithPointers(byte* input, byte* output, int width, int height)
    {
        var radius = EdgeDetectionKernel.KernelRadius;
        
        for (var row = radius; row < height - radius; row++)
        {
            for (var col = radius; col < width - radius; col++)
            {
                int redSum = 0, greenSum = 0, blueSum = 0;

                for (var kernelRow = -radius; kernelRow <= radius; kernelRow++)
                {
                    for (var kernelCol = -radius; kernelCol <= radius; kernelCol++)
                    {
                        var pixelPtr = input + ((row + kernelRow) * width + (col + kernelCol)) * 3;
                        var weight = EdgeDetectionKernel.LaplacianMatrix[kernelRow + radius, kernelCol + radius];

                        redSum += pixelPtr[0] * weight;
                        greenSum += pixelPtr[1] * weight;
                        blueSum += pixelPtr[2] * weight;
                    }
                }

                var outputPtr = output + (row * width + col) * 3;
                outputPtr[0] = (byte)Math.Clamp(redSum, 0, 255);
                outputPtr[1] = (byte)Math.Clamp(greenSum, 0, 255);
                outputPtr[2] = (byte)Math.Clamp(blueSum, 0, 255);
            }
        }
    }
}