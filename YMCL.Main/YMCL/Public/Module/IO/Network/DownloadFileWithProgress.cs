using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YMCL.Public.Controls;

namespace YMCL.Public.Module.IO.Network;

public class DownloadFileWithProgress
{
    public static async Task<bool> Download(string url, string path, TaskEntry task, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();
            using var response =
                await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
            await using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken),
                fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                    8192, true);
            long totalBytesRead = 0;
            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) != 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;
                var progressPercentage = (double)totalBytesRead / totalBytes * 100;
                task.UpdateValue(progressPercentage);
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            task.FinishWithError();
            return false;
        }
    }
}