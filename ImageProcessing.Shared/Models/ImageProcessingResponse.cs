namespace ImageProcessing.Shared.Models;

public class ImageProcessingResponse
{
    public string ImageOutputPath { get; set; }
    public string RequestId { get; set; }
    public DateTime RequestTimestamp { get; set; }
    public DateTime ResponseTimestamp { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
}