using Grpc.Core;
using Grpc.Net.Client;
using GrpcPoc.Service.Protos;

namespace GrpcPoc.Client;

public interface IFileServiceFacade
{
    Task UploadFileAsync(FileStream inputStream, string fileKey);

    Task DownloadFileAsync(FileStream outputStream, string fileKey);
}

public class FileServiceClientFacade : IFileServiceFacade
{
    private FileService.FileServiceClient _client;

    public FileServiceClientFacade(string address)
    {
        var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions()
        {
            MaxReceiveMessageSize = null, // No limit on receive message size
            MaxSendMessageSize = null, // No limit on send message size
            HttpHandler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(10),
                KeepAlivePingDelay = TimeSpan.FromSeconds(30),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(15),
                EnableMultipleHttp2Connections = true
            },
        });

        _client = new FileService.FileServiceClient(channel);
    }

    public async Task UploadFileAsync(FileStream fileStream, string fileKey)
    {
        var upload = _client.UploadFile(headers: new Metadata()
        {
            { "file-name", fileKey }
        });

        var buffer = new byte[64 * 1024]; // 64KB chunks
        int bytesRead;
        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            var uploadRequest = new FileChunk
            {
                Content = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead),
                ChunkSize = bytesRead
            };
            
            await upload.RequestStream.WriteAsync(uploadRequest);
        }

        await upload.RequestStream.CompleteAsync();

        var uploadStatus = await upload;
        if (!uploadStatus.Success)
        {
            throw new Exception($"Upload failed: {uploadStatus.Message}");
        }
    }

    public async Task DownloadFileAsync(FileStream outputStream, string fileKey)
    {
        var download = _client.DownloadFile(new FileRequest { FileName = fileKey });

        await foreach (var chunk in download.ResponseStream.ReadAllAsync())
        {
            outputStream.Write(chunk.Content.ToByteArray());
        }
    }
}
