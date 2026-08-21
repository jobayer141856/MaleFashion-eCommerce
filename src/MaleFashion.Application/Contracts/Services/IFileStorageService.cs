using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Contracts.Services
{
    public interface IFileStorageService
    {
        Task DeleteImageAsync(string imageName, string folder, CancellationToken cancellationToken);
        Task<string> SaveImageAsync(
            Stream stream,
            string originalFileName,
            string folder,
            CancellationToken cancellationToken = default);
    }
}
