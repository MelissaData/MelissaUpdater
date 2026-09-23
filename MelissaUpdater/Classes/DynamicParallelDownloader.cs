using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace MelissaUpdater.Classes
{
  /// <summary>
  /// Downloads a file using parallel HTTP range requests and computes a SHA-256 hash of the downloaded content.
  /// Falls back to a single-stream download when the server does not support range requests,
  /// when the file is small, or when the caller explicitly asks for one job.
  /// </summary>
  public class DynamicParallelDownloader
  {
    private const long MinChunkBytes = 32L * 1024 * 1024;
    private const long MaxChunkBytes = 128L * 1024 * 1024;
    private const int ReadBufferSize = 4 * 1024 * 1024;

    /// <summary>Maximum attempts for an individual chunk before the download is abandoned.</summary>
    private const int MaxChunkRetries = 3;

    /// <summary>Maximum attempts for the single-stream fallback download.</summary>
    private const int MaxFallbackRetries = 3;

    /// <summary>Files larger than this use parallel workers when no job count was specified.</summary>
    private const long AutoParallelThresholdBytes = 100L * 1000 * 1000;

    /// <summary>Job count applied automatically to files above <see cref="AutoParallelThresholdBytes"/>.</summary>
    private const int AutoJobsNumber = 4;

    /// <summary>Upper bound on the job count a caller may request.</summary>
    private const int MaxJobsNumber = 20;

    private readonly string DownloadUrl;
    private readonly string DestinationFilePath;
    private readonly int JobsNumber;
    private readonly bool IsQuiet;

    /// <summary>
    /// Represents a progress callback for download operations.
    /// </summary>
    /// <param name="total">Total number of bytes expected.</param>
    /// <param name="downloaded">Total number of bytes downloaded so far.</param>
    public delegate void ProgressHandler(long total, long downloaded);

    /// <summary>
    /// Occurs when download progress changes.
    /// </summary>
    public event ProgressHandler OnProgress;

    private record WorkerStat(
        int Index,
        int ChunksProcessed,
        long BytesDownloaded,
        TimeSpan Elapsed);

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicParallelDownloader"/> class.
    /// </summary>
    /// <param name="url">Source URL to download.</param>
    /// <param name="destPath">Destination file path.</param>
    /// <param name="jobsNumber">
    /// Number of worker jobs to use. Pass 0 to let the downloader choose: files larger than
    /// 100 MB use <see cref="AutoJobsNumber"/> workers, smaller files use a single stream.
    /// Pass 1 to force a single-stream download.
    /// </param>
    /// <param name="quiet">Whether quiet logging is enabled.</param>
    public DynamicParallelDownloader(
        string url,
        string destPath,
        int jobsNumber = 0,
        bool quiet = false)
    {
      DownloadUrl = url;
      DestinationFilePath = destPath;
      JobsNumber = jobsNumber <= 0 ? 0 : Math.Clamp(jobsNumber, 1, MaxJobsNumber);
      IsQuiet = quiet;
    }

    /// <summary>
    /// Downloads the file to the configured destination and returns its SHA-256 hash.
    /// </summary>
    /// <returns>The lowercase hexadecimal SHA-256 hash of the downloaded file.</returns>
    public async Task<string> DownloadAsync()
    {
      var totalSw = Stopwatch.StartNew();

      // Caller explicitly asked for a single stream — no need to probe the server.
      if (JobsNumber == 1)
      {
        Utilities.Log($"Forced single-stream download.", IsQuiet);
        return await FallbackDownloadWithRetryAsync();
      }

      using var http = new HttpClient { Timeout = TimeSpan.FromDays(1) };

      // ── 1. Probe GET: test range support and extract total file size ──
      bool supportsRanges;
      long? fileSize;

      try
      {
        using var probeReq = new HttpRequestMessage(HttpMethod.Get, DownloadUrl);
        probeReq.Headers.Range = new RangeHeaderValue(0, 0);
        probeReq.Headers.Add("Accept-Encoding", "identity");

        using var probeResp = await http.SendAsync(probeReq,
            HttpCompletionOption.ResponseHeadersRead);
        probeResp.EnsureSuccessStatusCode();

        supportsRanges = probeResp.StatusCode == System.Net.HttpStatusCode.PartialContent;
        fileSize = probeResp.Content.Headers.ContentRange?.Length;
      }
      catch (Exception ex)
      {
        Utilities.Log($"Unable to probe the server for range support ({ex.Message}) — falling back to single-stream download.", IsQuiet);
        return await FallbackDownloadWithRetryAsync();
      }

      if (!supportsRanges || !fileSize.HasValue)
      {
        Utilities.Log("Server does not support range requests — falling back to single-stream download.", IsQuiet);
        return await FallbackDownloadWithRetryAsync();
      }

      // If the file is small, prefer single-stream download regardless of requested job count.
      if (fileSize.Value <= AutoParallelThresholdBytes)
      {
        Utilities.Log($"File is {FormatBytes(fileSize.Value)}, equal or less than {FormatBytes(AutoParallelThresholdBytes)} — using single-stream download.", IsQuiet);
        return await FallbackDownloadWithRetryAsync();
      }

      // ── 2. Resolve the worker count ──
      //    A caller-supplied count wins; otherwise use the automatic jobs number.
      int jobs = AutoJobsNumber;
      string jobsNumberStr = "(default)";

      if (JobsNumber != 0)
      {
        jobs = JobsNumber;
        jobsNumberStr = "(caller-specified)";
      }
      Utilities.Log($"Using {jobs} parallel download jobs {jobsNumberStr}.", IsQuiet);

      long chunkSize = Math.Clamp(fileSize.Value / (jobs * 4), MinChunkBytes, MaxChunkBytes);

      // ── 3. Build work queue ──
      var queue = new ConcurrentQueue<(long Start, long End)>();
      for (long offset = 0; offset < fileSize.Value; offset += chunkSize)
        queue.Enqueue((offset, Math.Min(offset + chunkSize - 1, fileSize.Value - 1)));

      int totalChunks = queue.Count;

      // ── 4. Pre-allocate file — ReadWrite so SHA-256 can read it back ──
      using var fs = new FileStream(
          DestinationFilePath, FileMode.Create, FileAccess.ReadWrite,
          FileShare.None, ReadBufferSize, FileOptions.Asynchronous);
      fs.SetLength(fileSize.Value);

      long bytesWritten = 0L;
      var statSlots = new WorkerStat[jobs];
      var downloadSw = Stopwatch.StartNew();

      // ── 5. Worker tasks — each pulls chunks until the queue is empty ──
      var workers = new Task[jobs];
      for (int i = 0; i < jobs; i++)
      {
        int workerIndex = i;

        workers[workerIndex] = Task.Run(async () =>
        {
          var workerSw = Stopwatch.StartNew();
          int chunksHandled = 0;
          long bytesHandled = 0L;
          var buffer = new byte[ReadBufferSize];

          while (queue.TryDequeue(out var chunk))
          {
            bool success = false;

            for (int attempt = 0; attempt < MaxChunkRetries && !success; attempt++)
            {
              // Bytes credited to the shared counter during this attempt. A failed
              // attempt rolls them back so a retry cannot double-count progress.
              long chunkBytes = 0L;

              try
              {
                if (attempt > 0)
                  Utilities.Log($"  Worker {workerIndex,2} | retry {attempt} | chunk {chunk.Start}-{chunk.End}", IsQuiet);

                using var req = new HttpRequestMessage(HttpMethod.Get, DownloadUrl);
                req.Headers.Range = new RangeHeaderValue(chunk.Start, chunk.End);
                req.Headers.Add("Accept-Encoding", "identity");

                using var resp = await http.SendAsync(req,
                    HttpCompletionOption.ResponseHeadersRead);
                resp.EnsureSuccessStatusCode();

                await using var stream = await resp.Content.ReadAsStreamAsync();
                long position = chunk.Start;
                int read;

                while ((read = await stream.ReadAsync(buffer.AsMemory())) > 0)
                {
                  await RandomAccess.WriteAsync(
                      fs.SafeFileHandle, buffer.AsMemory(0, read), position);

                  position += read;
                  chunkBytes += read;

                  long done = Interlocked.Add(ref bytesWritten, read);
                  OnProgress?.Invoke(fileSize.Value, done);
                }

                bytesHandled += chunkBytes;
                chunksHandled++;
                success = true;
              }
              catch when (attempt < MaxChunkRetries - 1)
              {
                Interlocked.Add(ref bytesWritten, -chunkBytes);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
              }
            }

            if (!success)
              throw new IOException(
                  $"Worker {workerIndex}: chunk {chunk.Start}-{chunk.End} " +
                  $"failed after {MaxChunkRetries} attempts.");
          }

          workerSw.Stop();
          statSlots[workerIndex] = new WorkerStat(
              workerIndex, chunksHandled, bytesHandled, workerSw.Elapsed);

        });
      }

      await Task.WhenAll(workers);
      downloadSw.Stop();

      fs.Flush(flushToDisk: true);

      // ── 6. SHA-256 pass over the completed file ──
      var hashSw = Stopwatch.StartNew();
      fs.Seek(0, SeekOrigin.Begin);
      byte[] hashBytes = await SHA256.HashDataAsync(fs);
      hashSw.Stop();

      totalSw.Stop();

      return Convert.ToHexString(hashBytes).ToLower();
    }

    /// <summary>
    /// Runs the single-stream fallback download, retrying up to
    /// <see cref="MaxFallbackRetries"/> times with exponential backoff.
    /// </summary>
    /// <returns>The hash returned by the fallback downloader.</returns>
    private async Task<string> FallbackDownloadWithRetryAsync()
    {
      for (int attempt = 1; attempt <= MaxFallbackRetries; attempt++)
      {
        try
        {
          return await FallbackDownloadAsync();
        }
        catch (Exception ex) when (attempt < MaxFallbackRetries)
        {
          int delaySeconds = (int)Math.Pow(2, attempt - 1);

          Utilities.Log("", IsQuiet);
          Utilities.Log($"Single-stream download attempt {attempt} of {MaxFallbackRetries} failed: {ex.Message}", IsQuiet);
          Utilities.Log($"Retrying in {delaySeconds} second(s)...", IsQuiet);

          await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
      }

      // Unreachable: the final attempt either returns or lets its exception propagate.
      throw new IOException($"Single-stream download failed after {MaxFallbackRetries} attempts.");
    }

    /// <summary>
    /// Downloads the file using a single-stream fallback strategy.
    /// </summary>
    /// <returns>The hash returned by the fallback downloader.</returns>
    private async Task<string> FallbackDownloadAsync()
    {
      using (var fallback = new HttpClientDownloadWithProgress(DownloadUrl, DestinationFilePath))
      {
        fallback.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
        {
          Utilities.DownloadProgressStatus(totalFileSize, totalBytesDownloaded, progressPercentage, IsQuiet);
        };
        return await fallback.StartDownload();
      }
    }

    private static string FormatBytes(long bytes) =>
      bytes switch
      {
        >= 1L * 1000 * 1000 * 1000 => $"{bytes / (1000.0 * 1000 * 1000):F2} GB",
        >= 1L * 1000 * 1000 => $"{bytes / (1000.0 * 1000):F2} MB",
        >= 1L * 1000 => $"{bytes / 1000.0:F2} KB",
        _ => $"{bytes} B"
      };
  }
}
