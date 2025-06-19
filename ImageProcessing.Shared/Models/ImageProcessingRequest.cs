namespace ImageProcessing.Shared.Models;

public class ImageProcessingRequest
{
    public string ImagePath { get; set; }
    public string Operation { get; set; }
    public string RequestId { get; set; }
    public DateTime Timestamp { get; set; }
    
}