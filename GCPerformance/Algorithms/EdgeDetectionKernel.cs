namespace GCPerformance.Algorithms;

public static class EdgeDetectionKernel
{
    public static readonly int[,] LaplacianMatrix = 
    {
        { 0,  0, -1,  0,  0 },
        { 0, -1, -2, -1,  0 },
        {-1, -2, 16, -2, -1 },
        { 0, -1, -2, -1,  0 },
        { 0,  0, -1,  0,  0 }
    };
    
    public const int KernelRadius = 2;
    public const int KernelSize = 5;
}