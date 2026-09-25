using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DataCore.Interfaces;

public record FileStorageResult(
    string FileName,
    string RelativeUrl,
    string Hash,
    long Size,
    bool AlreadyExisted);

public interface IFileStorageService
{
    (bool Ok, string Message) ValidateFile(string fileName, string contentType, long length);

    Task<FileStorageResult> SaveFileAsync(
        Stream stream,
        string originalFileName,
        string section = "portfolio",
        CancellationToken ct = default);

    bool DeleteFile(string relativeUrl);

    List<string> ListFiles(string section = "portfolio");
}
