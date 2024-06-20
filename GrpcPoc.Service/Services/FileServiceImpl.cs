using Google.Protobuf;
using Grpc.Core;
using GrpcPoc.Service.Protos;
using static GrpcPoc.Service.Protos.FileService;

namespace GrpcPoc.Service.Services
{
    public class FileServiceImpl : FileServiceBase
    {
        private string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "storage");
        
        public override async Task<UploadStatus> UploadFile(IAsyncStreamReader<FileChunk> requestStream, ServerCallContext context)
        {
            var headers = context.RequestHeaders;
            
            var fileName = headers.FirstOrDefault(h => h.Key == "file-name")?.Value ?? throw new ArgumentNullException("fileName");
            
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        
            var filePath = Path.Combine(_storagePath, fileName!);
            
            Console.WriteLine($"Uploading to {filePath}");
        
            try
            {
                await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                
                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    Console.WriteLine("Writing Chunk....");
                    
                    await fs.WriteAsync(chunk.Content.ToByteArray());
                }
        
                return new UploadStatus { Success = true, Message = "File uploaded successfully" };
            }
            catch (Exception ex)
            {
                return new UploadStatus { Success = false, Message = ex.Message };
            }
        
        }

        public override async Task DownloadFile(FileRequest request, IServerStreamWriter<FileChunk> responseStream, ServerCallContext context)
        {
            var filePath = Path.Combine(_storagePath, request.FileName);

            if (!File.Exists(filePath))
            {
                throw new RpcException(new Status(StatusCode.NotFound, request.FileName));
            }

            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var buffer = new byte[64 * 1024]; // 64KB chunks
            int bytesRead;
            while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                Console.WriteLine("Downloading Chunk");
                
                await responseStream.WriteAsync(new FileChunk
                {
                    Content = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead),
                    ChunkSize = bytesRead
                });
            }
        }
    }
}
