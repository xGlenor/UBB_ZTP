using System.Buffers;
using System.Numerics;

namespace GCPerformance.Algorithms;

public class VectorizedPixelProcessor : IPixelProcessor
{
    public void ProcessPixels(ReadOnlySpan<byte> input, Span<byte> output, int width, int height)
    {
        var intInputBuffer = ArrayPool<int>.Shared.Rent(input.Length);
        var intOutputBuffer = ArrayPool<int>.Shared.Rent(output.Length);

        try
        {
            // Convert to int for SIMD processing
            for (var i = 0; i < input.Length; i++)
                intInputBuffer[i] = input[i];

            ProcessWithVectors(intInputBuffer, intOutputBuffer, width, height);

            // Convert back to byte
            for (var i = 0; i < output.Length; i++)
                output[i] = (byte)Math.Clamp(intOutputBuffer[i], 0, 255);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(intInputBuffer);
            ArrayPool<int>.Shared.Return(intOutputBuffer);
        }
    }

    private static void ProcessWithVectors(int[] input, int[] output, int width, int height)
    {
        var vectorSize = Vector<int>.Count;
        var radius = EdgeDetectionKernel.KernelRadius;

        for (var row = radius; row < height - radius; row++)
        {
            var col = radius;
            
            // Process in vector chunks
            for (; col <= width - radius - vectorSize; col += vectorSize)
            {
                ProcessVectorChunk(input, output, width, row, col, vectorSize);
            }
            
            // Process remaining pixels
            for (; col < width - radius; col++)
            {
                ProcessScalarPixel(input, output, width, row, col);
            }
        }
    }

    private static void ProcessVectorChunk(int[] input, int[] output, int width, int row, int col, int vectorSize)
    {
        var redSums = Vector<int>.Zero;
        var greenSums = Vector<int>.Zero;
        var blueSums = Vector<int>.Zero;

        for (var kernelRow = -EdgeDetectionKernel.KernelRadius; kernelRow <= EdgeDetectionKernel.KernelRadius; kernelRow++)
        {
            for (var kernelCol = -EdgeDetectionKernel.KernelRadius; kernelCol <= EdgeDetectionKernel.KernelRadius; kernelCol++)
            {
                var weight = EdgeDetectionKernel.LaplacianMatrix[kernelRow + EdgeDetectionKernel.KernelRadius, kernelCol + EdgeDetectionKernel.KernelRadius];
                var weightVector = new Vector<int>(weight);

                var baseIndex = ((row + kernelRow) * width + (col + kernelCol)) * 3;
                
                var reds = new Vector<int>(input.AsSpan(baseIndex, vectorSize));
                var greens = new Vector<int>(input.AsSpan(baseIndex + vectorSize, vectorSize));
                var blues = new Vector<int>(input.AsSpan(baseIndex + vectorSize * 2, vectorSize));

                redSums += reds * weightVector;
                greenSums += greens * weightVector;
                blueSums += blues * weightVector;
            }
        }

        var outputIndex = (row * width + col) * 3;
        redSums.CopyTo(output.AsSpan(outputIndex, vectorSize));
        greenSums.CopyTo(output.AsSpan(outputIndex + vectorSize, vectorSize));
        blueSums.CopyTo(output.AsSpan(outputIndex + vectorSize * 2, vectorSize));
    }

    private static void ProcessScalarPixel(int[] input, int[] output, int width, int row, int col)
    {
        int redSum = 0, greenSum = 0, blueSum = 0;

        for (var kernelRow = -EdgeDetectionKernel.KernelRadius; kernelRow <= EdgeDetectionKernel.KernelRadius; kernelRow++)
        {
            for (var kernelCol = -EdgeDetectionKernel.KernelRadius; kernelCol <= EdgeDetectionKernel.KernelRadius; kernelCol++)
            {
                var pixelIndex = ((row + kernelRow) * width + (col + kernelCol)) * 3;
                var weight = EdgeDetectionKernel.LaplacianMatrix[kernelRow + EdgeDetectionKernel.KernelRadius, kernelCol + EdgeDetectionKernel.KernelRadius];

                redSum += input[pixelIndex] * weight;
                greenSum += input[pixelIndex + 1] * weight;
                blueSum += input[pixelIndex + 2] * weight;
            }
        }

        var outputIndex = (row * width + col) * 3;
        output[outputIndex] = redSum;
        output[outputIndex + 1] = greenSum;
        output[outputIndex + 2] = blueSum;
    }
}