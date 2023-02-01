using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MelissaUpdater.Classes
{
  class VerifyManager
  {
    public VerifyProgramInputs Inputs {get; set;}
    private readonly Stopwatch sw;
    bool validHashFile = true;

    /// <summary>
    /// Set Verify manager attributes from commandline parameters
    /// </summary>
    /// <param name="opts"></param>
    public VerifyManager(Config.VerifyOptions opts)
    {
      Inputs = new VerifyProgramInputs(opts);
      sw = new Stopwatch();
    }

    /// <summary>
    /// Check and compare the hashes for a single file or for all files in a directory
    /// </summary>
    /// <returns></returns>
    public int CheckHash()
    {
      // get the file attributes for file or directory
      FileAttributes attr = File.GetAttributes(Inputs.Path);
      int countFails = 0;

      // check directory 
      if (attr.HasFlag(FileAttributes.Directory))
      {
        Utilities.Log($"Verifying files in a directory: {Inputs.Path}", Inputs.Quiet);
        string[] fileEntries = Directory.GetFiles(Inputs.Path);
        foreach (string fileName in fileEntries)
        {
          if (fileName.Contains("hash"))
            continue;
          
          string hash1 = CalculateHash(fileName).Trim();
          string hash2 = GetOriginalHashAsync(fileName).Result;
          if (string.Equals(hash1, hash2))
          {
            Utilities.Log($"{Path.GetFileName(fileName)}: OK", Inputs.Quiet);
          }
          else if (!validHashFile)
          {
            Console.WriteLine($"{Path.GetFileName(fileName)}: FAILED MISSING HASH FILE");
            countFails++;
          }
          else
          {
            Console.WriteLine($"{Path.GetFileName(fileName)}: FAILED");
            countFails++;
          }
        }
      }

      // check a file
      else
      {
        Utilities.Log($"Verifying a file: {Inputs.Path}", Inputs.Quiet);
        string hash1 = CalculateHash(Inputs.Path).Trim();
        string hash2 = GetOriginalHashAsync(Inputs.Path).Result;
        if (string.Equals(hash1, hash2))
        {
          Utilities.Log($"{Path.GetFileName(Inputs.Path)}: OK", Inputs.Quiet);
        }
        else if (!validHashFile)
        {
          Console.WriteLine($"{Path.GetFileName(Inputs.Path)}: FAILED MISSING HASH FILE");
          countFails++;
        }
        else
        {
          Console.WriteLine($"{Path.GetFileName(Inputs.Path)}: FAILED");
          countFails++;
        }
      }

      if (countFails > 0)
      {
        Console.WriteLine($"Melissa Updater: WARNING: {countFails} computed checksum did NOT match.", Inputs.Quiet);
      }

      return countFails;
    }

    /// <summary>
    /// Calculate hash for a file
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private string CalculateHash(string file) 
    {
      string hash = Utilities.GetHashSha256(file);
      return hash;
    }

    /// <summary>
    /// Get the existing hash from the .hash file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<string> GetOriginalHashAsync(string path)
    {
      string hash = "";
      try
      {
        validHashFile = true;
        hash = await File.ReadAllTextAsync(path + ".hash");
      }
      catch (Exception e)
      {
        validHashFile = false;
      }
      return hash;
    }
  }
}
