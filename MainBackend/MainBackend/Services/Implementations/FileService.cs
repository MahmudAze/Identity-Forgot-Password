using MainBackend.Services.Interfaces;

namespace MainBackend.Services.Implementations
{
    public class FileService : IFileService
    {
        public async Task<string> ReadFile(string path)
        {
            using StreamReader streamReader = new("wwwroot/template/verify.html");

            string emailBody = await streamReader.ReadToEndAsync();

            return emailBody;
        }
    }
}
