using System.Collections.Concurrent;
using System.Drawing;
using System.Text;
using System.Text.Json;
using ImageProcessing.Shared.Models;
using ImageProcessing.Shared.Services;
using ImageProcessing.Shared.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ImageProcessing.Client;

internal class Program
{
    private static readonly string ExchangeName = "image_processing_exchange";
    private static readonly string RoutingKey = "image_processing";
    
    private static readonly ConcurrentDictionary<string, TaskCompletionSource<ImageProcessingResponse>>
        pendingRequests =
            new();

    private static async Task Main(string[] args)
    {

        var factory = new ConnectionFactory { HostName = "127.0.0.1", UserName = "admin", Password = "admin123", Port = 5672, ClientProvidedName = "Client", RequestedFrameMax = 536870912};
        
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);
        
        var replyQueueName = await channel.QueueDeclareAsync();
        var consumer = new AsyncEventingBasicConsumer(channel);
        
        consumer.ReceivedAsync += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var response = JsonSerializer.Deserialize<ImageProcessingResponse>(Encoding.UTF8.GetString(body));
            var correlationId = ea.BasicProperties.CorrelationId;
            
            if (pendingRequests.TryRemove(correlationId, out var tcs)) tcs.SetResult(response);
            return Task.CompletedTask;
        };
        
        await channel.BasicConsumeAsync(
            consumer: consumer,
            queue: replyQueueName,
            autoAck: true);

        while (true)
        {
            Console.WriteLine("\nImage Processing Client");
            Console.WriteLine("1. Process local image");
            Console.WriteLine("2. Exit");
            Console.Write("\nSelect an option: ");

            var choice = Console.ReadLine();
            if (choice == "2") break;

            if (choice == "1")
            {
                var imagePath = BitmapUtil.GetImagePath();
                
                Console.WriteLine("\nAvailable operations:");
                Console.WriteLine("1. Grayscale");
                Console.WriteLine("2. Blur");
                Console.Write("Select operation: ");

                var operation = Console.ReadLine() switch
                {
                    "1" => "grayscale",
                    "2" => "blur",
                    _ => "grayscale"
                };

                try
                {
                    var request = new ImageProcessingRequest
                    {
                        ImagePath = imagePath,
                        Operation = operation,
                        RequestId = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.UtcNow,
                    };
                    
                    
                    var response = await SendRequest(channel, request, replyQueueName);

                    if (response.Success)
                    {
                        Console.WriteLine("\nProcessing completed successfully!");
                        Console.WriteLine($"Processing time: {response.ProcessingTime.TotalMilliseconds:F2}ms");
                        Console.WriteLine($"Processed image saved as: {response.ImageOutputPath}");
                    }
                    else
                    {
                        Console.WriteLine($"\nProcessing failed: {response.Error}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message} \n {ex}");
                }
            }
        }
    }


    private static async Task<ImageProcessingResponse> SendRequest(IChannel channel, ImageProcessingRequest request,
        string replyQueue)
    {
        var tcs = new TaskCompletionSource<ImageProcessingResponse>();
        var correlationId = Guid.NewGuid().ToString();
        
        var props = new BasicProperties
        {
            CorrelationId = correlationId,
            ReplyTo = replyQueue
        };

        var options = new JsonSerializerOptions
        {
            MaxDepth = int.MaxValue,
        };
        
        var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request, options));
        
        pendingRequests[correlationId] = tcs;
        
        await channel.BasicPublishAsync(
            ExchangeName,
            RoutingKey,
            false,
            props,
            new ReadOnlyMemory<byte>(messageBytes));


        return await tcs.Task;
    }
}