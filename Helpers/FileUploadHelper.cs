namespace PharmaTrackPro.Helpers
{
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxSizeBytes = 2 * 1024 * 1024; // 2 MB

        /// <summary>
        /// Saves an uploaded image under wwwroot/uploads/{subfolder}/ with a generated
        /// filename, and returns the web-relative path to store on the entity.
        /// </summary>
        public static async Task<(bool Success, string? RelativePath, string? Error)> SaveImageAsync(
            IFormFile file, string subfolder, IWebHostEnvironment env)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return (false, null, "Only JPG, PNG, and WEBP images are allowed.");
            }

            if (file.Length > MaxSizeBytes)
            {
                return (false, null, "Image must be smaller than 2 MB.");
            }

            var folder = Path.Combine(env.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (true, $"/uploads/{subfolder}/{fileName}", null);
        }

        /// <summary>Writes raw bytes (e.g. a generated barcode/QR PNG) to wwwroot/uploads/{subfolder}/{fileName}.</summary>
        public static async Task<string> SaveBytesAsync(byte[] bytes, string subfolder, string fileName, IWebHostEnvironment env)
        {
            var folder = Path.Combine(env.WebRootPath, "uploads", subfolder);
            Directory.CreateDirectory(folder);

            var fullPath = Path.Combine(folder, fileName);
            await File.WriteAllBytesAsync(fullPath, bytes);

            return $"/uploads/{subfolder}/{fileName}";
        }

        public static void DeleteIfExists(string? relativePath, IWebHostEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            var fullPath = Path.Combine(
                env.WebRootPath,
                relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
