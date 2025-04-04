using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Runtime.InteropServices;
using ByteSizeLib;

namespace MelissaUpdater.Classes
{
  class Utilities
  {
    private static readonly SHA256 Sha256 = SHA256.Create();
    private static readonly HttpClient Client = new HttpClient();

    /// <summary>
    /// Calculate hash
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string GetHashSha256(string filename)
    {
      using FileStream stream = System.IO.File.OpenRead(filename);
      byte[] bytes = Sha256.ComputeHash(stream);
      StringBuilder builder = new StringBuilder(bytes.Length * 2);
      for (int i = 0; i < bytes.Length; i++)
      {
        builder.Append(bytes[i].ToString("x2"));
      }
      return builder.ToString();
    }

    /// <summary>
    /// Create or update hash file(s) 
    /// Check if hashes match
    /// </summary>
    /// <param name="file"></param>
    /// <param name="fileName"></param>
    /// <param name="SHA256">Hash 1</param>
    /// <param name="hash">Hash 2</param>
    /// <param name="quiet"></param>
    /// <returns></returns>
    public static async Task CreateOrUpdateHashFile(string file, string fileName, string SHA256, string hash, bool quiet)
    {
      Directory.CreateDirectory(Path.GetDirectoryName(file));

      using FileStream fs = new FileStream(file + ".hash", FileMode.OpenOrCreate, FileAccess.ReadWrite);

      if (fs.Length != 0)
      {
        fs.SetLength(0);
      }

      LogWithoutNewLine($"Verifying SHA256 of {fileName}: ", quiet);
      Log(hash, quiet);

      if (hash.Equals(SHA256))
      {
        Log("Verified hash", quiet);
        byte[] bytes = Encoding.UTF8.GetBytes(hash);
        await fs.WriteAsync(bytes, 0, bytes.Length);
      }
      else
      {
        Utilities.Log("Hashes not match. An error occured while downloading!", false);
      }
    }

    /// <summary>
    /// Create or update hash file
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static async Task CreateOrUpdateHashFile(string file)
    {
      Directory.CreateDirectory(Path.GetDirectoryName(file));

      using FileStream fs = new FileStream(file + ".hash", FileMode.OpenOrCreate, FileAccess.ReadWrite);

      if (fs.Length != 0)
      {
        fs.SetLength(0);
      }
      string hash = GetHashSha256(file);

      byte[] bytes = Encoding.UTF8.GetBytes(hash);
      await fs.WriteAsync(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Console log information of the current process 
    /// </summary>
    /// <param name="s"></param>
    /// <param name="quiet"></param>
    public static void Log(string s, bool quiet)
    {
      if (!quiet)
      {
        Console.WriteLine(s);
      }
    }

    /// <summary>
    /// Console log information of the current process on the same line
    /// </summary>
    /// <param name="s"></param>
    /// <param name="quiet"></param>
    public static void LogWithoutNewLine(string s, bool quiet)
    {
      if (!quiet)
      {
        Console.Write(s);
      }
    }
    public static void LogError(string s, bool quiet=false)
    {
      if (s.Contains("thrown"))
      {
        return;
      }
      else if (!quiet)
      {
        Console.WriteLine(s);
      }
    }

    public static void LogErrorWithoutNewLine(string s, bool quiet=false)
    {
      if (s.Contains("thrown"))
      {
        return;
      }
      if (!quiet)
      {
        Console.Error.Write(s);
      }
    }

    /// <summary>
    /// Execute the process with accordingly parameter(s) if any
    /// </summary>
    /// <param name="inputProcess"></param>
    /// <param name="inputArgs"></param>
    public static void ExecuteProcess(string inputProcess, string inputArgs)
    {
      Process process = new Process();
      process.StartInfo = new ProcessStartInfo(inputProcess)
      {
        UseShellExecute = true
      };
      process.StartInfo.Arguments = inputArgs;
      process.Start();
    }
    /// <summary>
    /// Parse out arguments of the process from an input string delimited by comma(s)
    /// </summary>
    /// <param name="inputProcess"></param>
    /// <returns></returns>
    public static string ParseProcessArguments(ref string inputProcess)
    {
      List<string> arguments = inputProcess.Split(',').ToList();
      inputProcess = arguments[0];
      arguments.RemoveAt(0);
      for (var i = 0; i < arguments.Count; i++)
      {
        if (arguments[i].Contains(" "))
          arguments[i] = string.Format("\"" + arguments[i] + "\"");
      }
      string processArguments = string.Join(" ", arguments);
      return processArguments;

    }

    /// <summary>
    /// Check for any updates to the Melissa Updater.
    /// Displays information if there is a newer version.
    /// </summary>
    /// <returns>Whether or not it was successfully checked.</returns>
    public static async Task<bool> CheckForMelissaUpdaterUpdates(bool quiet)
    {
      string url;

      if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        url = "https://releases.melissadata.net/Metadata/Library/LINUX/NET/ANY/latest/MelissaUpdater";
      }
      else
      {
        url = "https://releases.melissadata.net/Metadata/Library/WINDOWS/NET/ANY/latest/MelissaUpdater.exe";
      }

      Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
      bool success = true;

      try
      {
        HttpResponseMessage response = await Client.GetAsync(url);

        string responseString = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
          var responseInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
          Version latestVersion = new Version(responseInfo["BuildNumber"]);

          if (currentVersion < latestVersion)
          {
            Log($"A newer version of the Melissa Updater is now available!", quiet);
            Log($"{currentVersion} -> {latestVersion}", quiet);
            Log($"Download:      {url}", quiet);
            Log($"Release Notes: https://releasenotes.melissa.com/software/melissa-updater/", quiet);
            Log($"GitHub:        https://github.com/MelissaData/MelissaUpdater", quiet);
          }
        }
        else
        {
          success = false;
        }
      } catch
      {
        success = false;
      }

      return success;
    }

    /// <summary>
    /// Displays the download progress for a file
    /// </summary>
    /// <param name="totalFileSize"></param>
    /// <param name="totalBytesDownloaded"></param>
    /// <param name="progressPercentage"></param>
    /// <param name="quiet"></param>
    public static void DownloadProgressStatus(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage, bool quiet)
    {
      string totalBytesDownloadedReadable = ByteSize.FromBytes(Convert.ToDouble(totalBytesDownloaded)).ToString("#.00");
      string totalFileSizeReadable = ByteSize.FromBytes(Convert.ToDouble(totalFileSize)).ToString("#.00");
      Utilities.LogErrorWithoutNewLine($"\r [ {totalBytesDownloadedReadable,9} / {totalFileSizeReadable,9} ] {progressPercentage,3} % complete...", quiet);
    }

    /// <summary>
    /// Displays the total file size to be updated
    /// </summary>
    /// <param name="totalFileSize"></param>
    /// <param name="quiet"></param>
    public static void DisplayTotalFileSize(long totalFileSize, bool quiet)
    {
      Utilities.Log($"Total size of update: {ByteSize.FromBytes(Convert.ToDouble(totalFileSize)).ToString("#.00")}\n", quiet);
    }

    /// <summary>
    /// Displays the total file size to be updated
    /// </summary>
    /// <param name="totalFileSize"></param>
    /// <param name="quiet"></param>
    public static void DisplayTotalFileSize(string totalFileSize, bool quiet)
    {
      Utilities.Log($"Total size of update: {ByteSize.FromBytes(Convert.ToDouble(totalFileSize)).ToString("#.00")}\n", quiet);
    }
  }
}
