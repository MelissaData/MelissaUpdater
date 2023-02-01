using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MelissaUpdater.Models;

namespace MelissaUpdater.Classes
{
  class Utilities
  {
    private static readonly SHA256 Sha256 = SHA256.Create();

    /// <summary>
    /// Calculate hash
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string GetHashSha256(string filename)
    {
      using FileStream stream = File.OpenRead(filename);
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
    /// <param name="SHA256"></param>
    /// <param name="quiet"></param>
    /// <returns></returns>
    public static async Task CreateOrUpdateHashFile(string file, string fileName, string SHA256, bool quiet)
    {
      Directory.CreateDirectory(Path.GetDirectoryName(file));

      using FileStream fs = new FileStream(file + ".hash", FileMode.OpenOrCreate, FileAccess.ReadWrite);

      if (fs.Length != 0)
      {
        fs.SetLength(0);
      }
      //calculating SHA256 hash of <file name>: 
      LogWithoutNewLine($"Calculating SHA256 of {fileName}: ", quiet);
      string hash = GetHashSha256(file);
      Log(hash, quiet);

      if (hash.Equals(SHA256))
      {
        Log("Verified hash", quiet);
        byte[] bytes = Encoding.UTF8.GetBytes(hash);
        await fs.WriteAsync(bytes, 0, bytes.Length);
      }
      else
      {
        Console.WriteLine("Hashes not match. An error occured while downloading!");
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

    public static void LogError(string s, bool quiet)
    {
      if (!quiet)
      {
        Console.Error.WriteLine(s);
      }
    }

    public static void LogErrorWithoutNewLine(string s, bool quiet)
    {
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

  }
}
