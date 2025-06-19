using System.Drawing;
using System.Text;
using System.Text.Json;
using ImageProcessing.Shared.Models;
using ImageProcessing.Shared.Services;
using ImageProcessing.Shared.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ImageProcessing.Server;

internal class Program
{
    private static readonly string QueueName = "image_processing_queue";
    private static readonly string ExchangeName = "image_processing_exchange";
    private static readonly string RoutingKey = "image_processing";

    private static async Task Main(string[] args)
    {

        var factory = new ConnectionFactory { HostName = "127.0.0.1", UserName = "admin", Password = "admin123", Port = 5672, ClientProvidedName = "Server", RequestedFrameMax = 536870912};
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();


        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);
        await channel.QueueDeclareAsync(QueueName, false, false, false, null);
        await channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey);

        Console.WriteLine(" [*] Image Processing Server started. Waiting for requests...");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<ImageProcessingRequest>(message);

                Console.WriteLine($" [x] Received request {request.RequestId} for operation: {request.Operation}");


                var response = ProcessImage(request);

                var replyProps = new BasicProperties();
                replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                await channel.BasicPublishAsync(
                    "",
                    ea.BasicProperties.ReplyTo,
                    false,
                    replyProps,
                    new ReadOnlyMemory<byte>(responseBytes));
                
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [!] Error processing request: {ex.Message}");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };
        
        await channel.BasicConsumeAsync(QueueName,
            false,
            consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
    
    private static ImageProcessingResponse ProcessImage(ImageProcessingRequest request)
    {
        var response = new ImageProcessingResponse
        {
            RequestId = request.RequestId,
            RequestTimestamp = request.Timestamp,
            ResponseTimestamp = DateTime.UtcNow,
            Success = true,
        };

        try
        {
            byte[] processedImage = null;

            var startTime = DateTime.UtcNow;
            using var bitmap = new Bitmap(request.ImagePath);
            var imageBytes = BitmapUtil.BitmapToByteArray(bitmap);
            
            switch (request.Operation.ToLower())
            {
                case "grayscale":
                    processedImage = ImageProcessor.ApplyGrayscaleInPlace(imageBytes);
                    break;
                case "blur":
                    processedImage = BitmapUtil.BitmapToByteArray(BlurProcessor.ApplyBlur(bitmap, 60));
                    break;
                default:
                    throw new ArgumentException($"Unsupported operation: {request.Operation}");
            }
            
            response.ProcessingTime = DateTime.UtcNow - startTime;

            var outputPath = BitmapUtil.GenerateOutputPath(request.ImagePath, response.RequestId);
            
            BitmapUtil.SaveProcessedImage(processedImage, outputPath, bitmap.Width, bitmap.Height);
            response.ImageOutputPath = outputPath;
            
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Error = ex.Message;
        }

        return response;
    }
}