namespace GCPerformance.Algorithms;

public interface IPixelProcessor
{
    void ProcessPixels(ReadOnlySpan<byte> input, Span<byte> output, int width, int height);
}