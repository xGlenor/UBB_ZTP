
# Image Processing Server

## Overview
The **Image Processing Server** is a C# application that processes image transformation requests (e.g., grayscale, blur) using RabbitMQ for message queuing. The server listens for incoming requests, processes the images, and sends back the results.

**Note:** This project is a university assignment.

## Features
- Supports image processing operations such as:
    - Grayscale
    - Blur
- Uses RabbitMQ for asynchronous communication.
- Handles large image files with configurable message size limits.

## Prerequisites
- [.NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (for RabbitMQ setup)
- RabbitMQ Docker image with management plugin.

## Setup

### 1. Clone the Repository
```bash
git clone https://github.com/xGlenor/ImageProcessingSystem
cd ImageProcessingSystem
```

### 2. Start RabbitMQ
Use the provided `docker-compose.yml` file to start RabbitMQ:
```bash
cd docker
docker-compose up -d
```

### 3. Configure RabbitMQ
The RabbitMQ configuration is defined in `docker/rabbitmq.conf`. It increases the maximum message size to 512MB.

### 4. Run the Server
Navigate to the `ImageProcessing.Server` project and run the application:
```bash
cd ImageProcessing.Server
dotnet run
```

## Usage
1. Send a message to the `image_processing_queue` queue with the following JSON structure:
   ```json
   {
     "RequestId": "unique-id",
     "Timestamp": "2023-01-01T00:00:00Z",
     "ImagePath": "path/to/image.jpg",
     "Operation": "grayscale"
   }
   ```
2. The server processes the image and sends the response to the `ReplyTo` queue specified in the request.

## Project Structure
- `ImageProcessing.Server`: Main server application.
- `ImageProcessing.Shared`: Shared utilities and services for image processing.
- `docker`: RabbitMQ configuration and Docker setup.

## Configuration
- RabbitMQ connection settings can be modified in `ImageProcessing.Server/Program.cs`.
- Image processing logic is implemented in `ImageProcessing.Shared/Services`.

## License
This project is licensed under the MIT License.
