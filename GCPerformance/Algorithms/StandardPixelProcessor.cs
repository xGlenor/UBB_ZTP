namespace GCPerformance.Algorithms;

public class StandardPixelProcessor: IPixelProcessor
{
    public void ProcessPixels(ReadOnlySpan<byte> input, Span<byte> output, int width, int height)
    {
        var kernel = EdgeDetectionKernel.LaplacianMatrix;
        var radius = EdgeDetectionKernel.KernelRadius;

        for (var row = radius; row < height - radius; row++)
        {
            for (var col = radius; col < width - radius; col++)
            {
                ProcessSinglePixel(input, output, width, row, col, kernel, radius);
            }
        }
    }
    private static void ProcessSinglePixel(
        ReadOnlySpan<byte> input, 
        Span<byte> output, 
        int width, 
        int row, 
        int col, 
        int[,] kernel, 
        int radius)
    {
        int redSum = 0, greenSum = 0, blueSum = 0;

        for (var kernelRow = 0; kernelRow < EdgeDetectionKernel.KernelSize; kernelRow++)
        {
            for (var kernelCol = 0; kernelCol < EdgeDetectionKernel.KernelSize; kernelCol++)
            {
                var imageRow = row + kernelRow - radius;
                var imageCol = col + kernelCol - radius;
                var pixelIndex = (imageRow * width + imageCol) * 3;
                var weight = kernel[kernelRow, kernelCol];

                redSum += input[pixelIndex] * weight;
                greenSum += input[pixelIndex + 1] * weight;
                blueSum += input[pixelIndex + 2] * weight;
            }
        }

        var outputIndex = (row * width + col) * 3;
        output[outputIndex] = ClampToByte(redSum);
        output[outputIndex + 1] = ClampToByte(greenSum);
        output[outputIndex + 2] = ClampToByte(blueSum);
    }

    private static byte ClampToByte(int value) => (byte)Math.Clamp(value, 0, 255);
}