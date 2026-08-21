using MaleFashion.Application.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _rootPath;

        private static readonly string[] AllowedExtensions =
        {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

        private const long MaxFileSize = 5 * 1024 * 1024;

        public FileStorageService(string rootPath)
        {
            _rootPath = rootPath;
        }

        public async Task<string> SaveImageAsync(
            Stream stream,
            string originalFileName,
            string folder,
            CancellationToken cancellationToken = default)
        {
            if (stream == null || stream.Length == 0)
                throw new Exception("Empty file.");

            if (stream.Length > MaxFileSize)
                throw new Exception("File too large.");

            var extension = Path.GetExtension(originalFileName)
                .ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                throw new Exception("Invalid file type.");

            var fileName = $"{Guid.NewGuid():N}{extension}";

            var uploadFolder = Path.Combine(
                _rootPath,
                "uploads",
                folder.ToLower());

            Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(
                uploadFolder,
                fileName);

            await using var fileStream = new FileStream(
                filePath,
                FileMode.Create);

            stream.Position = 0;

            await stream.CopyToAsync(
                fileStream,
                cancellationToken);

            return fileName;
        }

        public Task DeleteImageAsync(string imageName, string folder, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return Task.CompletedTask;

            // Prevent path traversal
            imageName = Path.GetFileName(imageName);

            var uploadFolder = Path.Combine(
                _rootPath,
                "uploads",
                folder.ToLowerInvariant());

            var filePath = Path.Combine(
                uploadFolder,
                imageName);

            if (!File.Exists(filePath))
                return Task.CompletedTask;

            File.Delete(filePath);

            return Task.CompletedTask;
        }
    }
}
