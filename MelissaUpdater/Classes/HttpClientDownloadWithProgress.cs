using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace MelissaUpdater.Classes
{
  public class HttpClientDownloadWithProgress : IDisposable
  {
    private readonly string downloadUrl;
    private readonly string destinationFilePath;
    private HttpClient httpClient;

    public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

    public event ProgressChangedHandler ProgressChanged;

    public HttpClientDownloadWithProgress(string url, string path)
    {
      downloadUrl = url;
      destinationFilePath = path;
    }

    public async Task<string> StartDownload()
    {
      var hash = String.Empty;
      httpClient = new HttpClient() { Timeout = TimeSpan.FromDays(1) };

      using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
        hash = await DownloadFileFromHttpResponseMessage(response);

      return hash;
    }

    private async Task<string> DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
    {
      response.EnsureSuccessStatusCode();

      var totalBytes = response.Content.Headers.ContentLength;
      var hash = String.Empty;

      using (var contentStream = await response.Content.ReadAsStreamAsync())
        hash = await ProcessContentStream(totalBytes, contentStream);

      return hash;
    }

    private async Task<string> ProcessContentStream(long? totalDownloadSize, Stream contentStream)
    {
      using SHA256 sha256 = SHA256.Create();

      var totalBytesRead = 0L;
      var readCount = 0L;
      var buffer = new byte[8192];
      var isMoreToRead = true;

      using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
      {
        do
        {
          var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
          if (bytesRead == 0)
          {
            isMoreToRead = false;
            TriggerProgressChanged(totalDownloadSize, totalBytesRead);
            continue;
          }

          await fileStream.WriteAsync(buffer, 0, bytesRead);
          sha256.TransformBlock(buffer, 0, bytesRead, buffer, 0);

          totalBytesRead += bytesRead;
          readCount += 1;

          if (readCount % 100 == 0)
            TriggerProgressChanged(totalDownloadSize, totalBytesRead);
        }
        while (isMoreToRead);

        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        // Here's the SHA-256 hash:
        string hash = BitConverter.ToString(sha256.Hash).Replace("-", "");

        return hash.ToLower();
      }
    }

    private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
    {
      if (ProgressChanged == null)
        return;

      double? progressPercentage = null;
      if (totalDownloadSize.HasValue)
        progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 0);

      ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
    }

    public void Dispose()
    {
      httpClient?.Dispose();
    }
  }

}
