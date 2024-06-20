// See https://aka.ms/new-console-template for more information

using GrpcPoc.Client;

var fileServiceFacade = new FileServiceClientFacade("http://localhost:5000");

try
{
    // Upload File
    var filePath =  @"C:\Users\abruno\Downloads\largefile.dat";

    if (!File.Exists(filePath))
    {
        GenerareFile();
    }

    string fileKey = "uploaded_file.pdf";
    
    await using var fileStream = File.OpenRead(filePath);
    
    await fileServiceFacade.UploadFileAsync(fileStream, fileKey);
    
    Console.WriteLine("File uploaded successfully");

    using (var fileOutputStream = new FileStream("C:\\Temp\\result.pdf", FileMode.Create, FileAccess.Write))
    {
        await fileServiceFacade.DownloadFileAsync(fileOutputStream, fileKey);
    }
    
    Console.WriteLine("File downloaded successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

void GenerareFile()
{
    string filePath = "C:\\Users\\abruno\\Downloads\\largefile.dat";
    long fileSize = 5L * 1024 * 1024 * 1024; // 5 GB in bytes
    int bufferSize = 1024 * 1024; // 1 MB buffer size

    byte[] buffer = new byte[bufferSize];
    Random random = new Random();

    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
    {
        long totalBytesWritten = 0;
        while (totalBytesWritten < fileSize)
        {
            random.NextBytes(buffer);
            long bytesToWrite = Math.Min(bufferSize, fileSize - totalBytesWritten);
            fs.Write(buffer, 0, (int)bytesToWrite);
            totalBytesWritten += bytesToWrite;
            Console.WriteLine($"Progress: {totalBytesWritten * 100 / fileSize}%");
        }
    }

    Console.WriteLine("File generation complete.");
}