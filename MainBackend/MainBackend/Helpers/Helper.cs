namespace MainBackend.Helpers
{
    public static class Helper
    {
        public static bool CheckFileType(this IFormFile file, string FileType)
        {
            return file.ContentType.Contains(FileType);
        }

        public static bool CheckFileSize(this IFormFile file, int size)
        {
            return file.Length / 1024 > size;
        }

        public static string GenerateFileName(this IFormFile file)
        {
            return Guid.NewGuid().ToString() + "_" + file.FileName;
        }

        public static string GetFilePath(this string root, string folder, string fileName)
        {
            return Path.Combine(root, folder, fileName);
        }

        public static void SaveFile(this IFormFile file, string path)
        {
            using (FileStream fileStream = new(path, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
        }
    }
}
