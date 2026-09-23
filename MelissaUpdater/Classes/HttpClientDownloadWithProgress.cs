using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace MelissaUpdater.Classes
{
  /// <summary>
  /// Downloads a file over HTTP with progress reporting and computes a SHA-256 hash while streaming.
  /// </summary>
  public class HttpClientDownloadWithProgress : IDisposable
  {
    private readonly string downloadUrl;
    private readonly string destinationFilePath;
    private HttpClient httpClient;

    /// <summary>
    /// Represents a callback that receives download progress updates.
    /// </summary>
    /// <param name="totalFileSize">The total file size in bytes when known; otherwise <see langword="null"/>.</param>
    /// <param name="totalBytesDownloaded">The number of bytes downloaded so far.</param>
    /// <param name="progressPercentage">The rounded percentage complete when total size is known; otherwise <see langword="null"/>.</param>
    public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

    /// <summary>
    /// Occurs when download progress changes.
    /// </summary>
    public event ProgressChangedHandler ProgressChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientDownloadWithProgress"/> class.
    /// </summary>
    /// <param name="url">The source URL to download.</param>
    /// <param name="path">The destination file path.</param>
    public HttpClientDownloadWithProgress(string url, string path)
    {
      downloadUrl = url;
      destinationFilePath = path;
    }

    /// <summary>
    /// Starts downloading the configured file and returns its SHA-256 hash.
    /// </summary>
    /// <returns>The lowercase hexadecimal SHA-256 hash of the downloaded file.</returns>
    public async Task<string> StartDownload()
    {
      var hash = String.Empty;
      httpClient = new HttpClient() { Timeout = TimeSpan.FromDays(1) };

      using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
        hash = await DownloadFileFromHttpResponseMessage(response);

      return hash;
    }

    /// <summary>
    /// Validates the HTTP response and downloads content to the target file.
    /// </summary>
    /// <param name="response">The HTTP response containing file content.</param>
    /// <returns>The lowercase hexadecimal SHA-256 hash of the downloaded file.</returns>
    private async Task<string> DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
    {
      response.EnsureSuccessStatusCode();

      var totalBytes = response.Content.Headers.ContentLength;
      var hash = String.Empty;

      using (var contentStream = await response.Content.ReadAsStreamAsync())
        hash = await ProcessContentStream(totalBytes, contentStream);

      return hash;
    }

    /// <summary>
    /// Streams HTTP content to disk, emits progress updates, and computes SHA-256.
    /// </summary>
    /// <param name="totalDownloadSize">The total content length in bytes when available; otherwise <see langword="null"/>.</param>
    /// <param name="contentStream">The source content stream.</param>
    /// <returns>The lowercase hexadecimal SHA-256 hash of the streamed file.</returns>
    private async Task<string> ProcessContentStream(long? totalDownloadSize, Stream contentStream)
    {
      using SHA256 sha256 = SHA256.Create();

      var totalBytesRead = 0L;
      var readCount = 0L;
      var buffer = new byte[4 * 1024 * 1024];
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

    /// <summary>
    /// Raises the <see cref="ProgressChanged"/> event with computed progress information.
    /// </summary>
    /// <param name="totalDownloadSize">The total content length in bytes when available; otherwise <see langword="null"/>.</param>
    /// <param name="totalBytesRead">The number of bytes downloaded so far.</param>
    private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
    {
      if (ProgressChanged == null)
        return;

      double? progressPercentage = null;
      if (totalDownloadSize.HasValue)
        progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 0);

      ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
    }

    /// <summary>
    /// Releases managed resources used by this downloader.
    /// </summary>
    public void Dispose()
    {
      httpClient?.Dispose();
    }
  }

}
