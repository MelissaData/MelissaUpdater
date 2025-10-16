﻿using MelissaUpdater.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MelissaUpdater.Exceptions;

namespace MelissaUpdater.Classes
{
  class SingleFileManager
  {
    public SingleFile SingleFile { get; set; }
    public SingleFileMetaData SingleFileMetaData { get; set; }
    public FileProgramInputs Inputs { get; set; }
    private readonly HttpClient Client;
    private readonly Stopwatch sw;
    
    /// <summary>
    /// Set single file attributes from commandline parameters
    /// Calculate SHA256 and override filename with single file's metadata information for correct casing
    /// </summary>
    /// <param name="opts"></param>
    public SingleFileManager(Config.FileOptions opts)
    {
      Inputs = new FileProgramInputs(opts);
      Client = new HttpClient();
      sw = new Stopwatch();

      SingleFile = new SingleFile()
      {
        FileName = Inputs.FileName,
        Release = Inputs.ReleaseVersion,
        OS = Inputs.OperatingSystem,
        Compiler = Inputs.Compiler,
        Architecture = Inputs.Architecture,
        Type = Inputs.Type,
      };
      SingleFileMetaData = GetMetaData(SingleFile).Result;
      SingleFile.SHA256 = SingleFileMetaData.SHA256;
      SingleFile.FileName = SingleFileMetaData.FileName;
    }

    /// <summary>
    /// Check for valid filename
    /// Check for valid license and download url
    /// Check for destination, either Target Directory or Working Directory
    /// Download the file
    /// Verify the hash
    /// </summary>
    /// <returns></returns>
    public async Task Download()
    {
      bool shouldContinue = true;
      SingleFile.SHA256 = await GetHash();

      string url = SingleFile.Type.ToUpper() switch
      {
        "DATA" => URLFormatter.FormatDataDownloadUrl(SingleFile, Inputs.LicenseString),
        "LIBRARY" => URLFormatter.FormatLibraryDownloadUrl(SingleFile, Inputs.LicenseString),
        "BINARY" => URLFormatter.FormatLibraryDownloadUrl(SingleFile, Inputs.LicenseString),
        "INTERFACE" => URLFormatter.FormatInterfaceDownloadUrl(SingleFile, Inputs.LicenseString),
        _ => ""
      };

      // add tag if given
      if (!string.IsNullOrEmpty(Inputs.Tag))
      {
        url = URLFormatter.AddTagToUrl(url, Inputs.Tag);
      }

      //check if file name is valid 
      if (SingleFile.SHA256.ToLower().Contains("invalid"))
      {
        shouldContinue = false;
        Utilities.Log($"{SingleFile.FileName} for {SingleFile.Release} is an invalid file!", Inputs.Quiet);
        throw new Exception("Cannot continue downloading due to invalid file name!");
      }
      
      if (!CheckDownloadUrl(url))
      {
        shouldContinue = false;
      }
      else
      {
        Utilities.Log($"Finished checking\n", Inputs.Quiet);
      }

      if (shouldContinue)
      {

        string path = string.IsNullOrWhiteSpace(Inputs.WorkingDirectory)
            ? Inputs.TargetDirectory
            : Inputs.WorkingDirectory;

        if (!Directory.Exists(path))
        {
          Directory.CreateDirectory(path);
        }

        string filePath = Path.Combine(path, SingleFile.FilePath);
        string hash = string.Empty;

        using (var client = new HttpClientDownloadWithProgress(url, filePath))
        {
          Utilities.Log($"Downloading {SingleFile.FileName} for {SingleFile.Release}", Inputs.Quiet);
          client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) => {
            Utilities.DownloadProgressStatus(totalFileSize, totalBytesDownloaded, progressPercentage, Inputs.Quiet);
          };
          hash = await client.StartDownload();
        }
        Utilities.Log("", Inputs.Quiet);

        await Utilities.CreateOrUpdateHashFile(filePath, SingleFile.FileName, SingleFile.SHA256, hash, Inputs.Quiet);
        Utilities.Log("Finished downloading\n", Inputs.Quiet);
      }
      
    }

    /// <summary>
    /// Check if the url to download the file is valid.
    /// Throws an exception if not.
    /// </summary>
    /// <param name="url"></param>
    private bool CheckDownloadUrl(string url)
    {
      Utilities.Log($"Checking url for {SingleFile.FileName} for {SingleFile.Release}", Inputs.Quiet);
      HttpClient client = new HttpClient();
      var response = client.GetAsync($"{url}", HttpCompletionOption.ResponseHeadersRead).Result;
      if (response.StatusCode != HttpStatusCode.OK)
      {
        throw new InvalidResponseException(response);
      }
      return response.StatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    /// Get file path from filename
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public void JoinFilePath()
    {
      sw.Restart();
      SingleFile.Type = Inputs.Type;
      SingleFile.FilePath = SingleFile.FileName;
      return;
    }

    /// <summary>
    /// Determine if the file exists or is up-to-date in the Working Directory
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CheckIfExistsInWorkingDirectory()
    {
      Utilities.Log($"Checking WorkingDirectory for {SingleFile.FileName} for {SingleFile.Release}", Inputs.Quiet);

      string path = Path.Combine(Inputs.WorkingDirectory, SingleFile.FilePath);

      if (File.Exists(path))
      {
        string hash = await GetHash();
        if (File.Exists(path + ".hash"))
        {
          SingleFile.SHA256 = await File.ReadAllTextAsync(path + ".hash");
        }
        else
        {
          if (Inputs.DryRun)
          {
            SingleFile.SHA256 = Utilities.GetHashSha256(path);
          }
          else
          {
            await Utilities.CreateOrUpdateHashFile(path);
            SingleFile.SHA256 = await File.ReadAllTextAsync(path + ".hash");
          }
        }

        if (hash.Equals(SingleFile.SHA256))
        {
          Utilities.Log($"{SingleFile.FileName} for {SingleFile.Release} in WorkingDirectory is up-to-date\n", Inputs.Quiet);
          return true;
        }

        else if (!Inputs.DryRun)
        {
          File.Move(path, $"{path}_OUT_OF_DATE");
          File.Move(path + ".hash", $"{path}_OUT_OF_DATE.hash");

          try
          {
            File.Delete(path);
            File.Delete(path + ".hash");

            File.Delete($"{path}_OUT_OF_DATE");
            File.Delete($"{path}_OUT_OF_DATE.hash");
          }
          catch (Exception e)
          {
            Utilities.Log($"Unable to delete files: {e.Message}.\n", false);
          }
        }
      }
      Utilities.Log($"{SingleFile.FileName} for {SingleFile.Release} needs to be updated in Working Directory\n", Inputs.Quiet);
      return false;
    }

    /// <summary>
    /// Check if file exists and up-to-date
    /// </summary>
    /// <returns></returns>
    public async Task<bool> DetermineRequiredUpdates()
    {
      sw.Restart();

      string path = Path.Combine(Inputs.TargetDirectory, SingleFile.FilePath);

      try
      {
        if (!File.Exists(path))
        {
          Utilities.Log($"{SingleFile.FileName} for {SingleFile.Release} needs to be updated\n", Inputs.Quiet);
          return true;
        }
        if (File.Exists(path + ".hash"))
        {
          SingleFile.SHA256 = await File.ReadAllTextAsync(path + ".hash");
        }
        else
        {
          if(Inputs.DryRun)
          {
            SingleFile.SHA256 = Utilities.GetHashSha256(path);
          }
          else
          {
            await Utilities.CreateOrUpdateHashFile(path);

            SingleFile.SHA256 = await File.ReadAllTextAsync(path + ".hash");
          }
        }

        string hash = await GetHash();
        if (string.IsNullOrEmpty(hash) || string.IsNullOrWhiteSpace(hash))
          throw new Exception("Hash from releases.net is empty");
        if (SingleFile.SHA256.Equals(hash))
        {
          Utilities.Log($"{SingleFile.FileName} for {SingleFile.Release} is up-to-date\n", Inputs.Quiet );
          return false;
        }
        
      }
      catch (Exception ex) 
      {
        Utilities.Log(ex.Message, false);
      }

      Utilities.Log($"{SingleFile.FileName} for {SingleFile.Release} needs to be updated\n", Inputs.Quiet);
      return true;
    }

    /// <summary>
    /// Move the downloaded file from Working Directory to Target Directory
    /// </summary>
    public void Update()
    {
      sw.Restart();
      Utilities.Log($"Updating {SingleFile.FileName} for {SingleFile.Release}", Inputs.Quiet);

      string workingDirectory = string.IsNullOrWhiteSpace(Inputs.WorkingDirectory)
          ? Inputs.TargetDirectory
          : Inputs.WorkingDirectory;

      string from = Path.Combine(workingDirectory, SingleFile.FilePath);
      string to = Path.Combine(Inputs.TargetDirectory, SingleFile.FilePath);

      if (!Directory.Exists(Path.GetDirectoryName(to)))
      {
        Directory.CreateDirectory(Path.GetDirectoryName(to));
      }

      int attempts = 0;
      while (attempts++ < 3)
      {
        try
        {
          File.Move(from, to);
          File.Move(from + ".hash", to + ".hash");
          Utilities.Log($"Finished updating\n", Inputs.Quiet);

          break;
        }
        catch (IOException ex)
        {
          Utilities.Log(ex.Message, false);
          if (attempts == 3)
          {
            Utilities.Log("Maximum number of attempts made.", false);
            Utilities.Log("Please close the associated file and restart this program.", false);
            Utilities.Log("This program will pick up where it left off.", false);
          }
          else
          {
            Utilities.Log($"[{attempts}] Please close the file and press any key to proceed", false);
            Console.ReadKey(false);
          }
        }
      }

    }

    /// <summary>
    /// Remove the existing file and hash file to make space for a new file to download
    /// </summary>
    public void CleanUpTargetDirectory()
    {
      Utilities.Log("Cleaning up Target Directory", Inputs.Quiet);

      string path = Path.Combine(Inputs.TargetDirectory, SingleFile.FilePath);
      int attempts = 0;
      while (attempts++ <3)
      {
        try
        {
          if (File.Exists(path))
          {
            File.Delete(path);
          }
          if (File.Exists(path + ".hash"))
          {
            File.Delete(path + ".hash");
          }
        }
        catch (IOException ex)
        {
          Utilities.Log(ex.Message, false);
          if (attempts == 3)
          {
            Utilities.Log("Maximum number of attempts made.", false);
            Utilities.Log("Please close the associated file and restart this program.", false);
            Utilities.Log("This program will pick up where it left off.", false);
          }
          else
          {
            Utilities.Log($"[{attempts}] Please close the file and press any key to proceed", false);
            Console.ReadKey(false);
          }
        }
      }

      Utilities.Log("Finished cleaning up\n", Inputs.Quiet);
    }

    /// <summary>
    /// Get hash of the file
    /// </summary>
    /// <returns></returns>
    private async Task<string> GetHash()
    {
      string url = SingleFile.Type.ToUpper() switch
      {
        "DATA" => URLFormatter.FormatDataUrl(SingleFile, Inputs.LicenseString),
        "LIBRARY" => URLFormatter.FormatLibraryUrl(SingleFile, Inputs.LicenseString),
        "BINARY" => URLFormatter.FormatLibraryUrl(SingleFile, Inputs.LicenseString),
        "INTERFACE" => URLFormatter.FormatInterfaceUrl(SingleFile, Inputs.LicenseString),
        _ => ""
      };

      // add tag if given
      if (!string.IsNullOrEmpty(Inputs.Tag))
      {
        url = URLFormatter.AddTagToUrl(url, Inputs.Tag);
      }

      HttpResponseMessage response = await Client.GetAsync(url);

      return await response.Content.ReadAsStringAsync();
    }
        
    /// <summary>
    /// Get metadata information for the file
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public  async Task<SingleFileMetaData> GetMetaData( SingleFile file)
    {
      string url = file.Type.ToUpper() switch
      {
        "DATA" => URLFormatter.FormatMetaDataUrl(file, Inputs.LicenseString),
        "LIBRARY" => URLFormatter.FormatMetaLibraryUrl(file, Inputs.LicenseString),
        "BINARY" => URLFormatter.FormatMetaLibraryUrl(file, Inputs.LicenseString),
        "INTERFACE" => URLFormatter.FormatMetaInterfaceUrl(file, Inputs.LicenseString),
        _ => ""
      };

      // add tag if given
      if (!string.IsNullOrEmpty(Inputs.Tag))
      {
        url = URLFormatter.AddTagToUrl(url, Inputs.Tag);
      }

      HttpResponseMessage response = await Client.GetAsync(url);
      var responseString = await response.Content.ReadAsStringAsync();

      if (response.StatusCode == HttpStatusCode.OK)
      {
        SingleFileMetaData metadata = JsonConvert.DeserializeObject<SingleFileMetaData>(responseString);
        return metadata;
      }
      else
      {
        string invalidType = "";
        if (!String.IsNullOrEmpty(responseString))
        {
            Response responseInfo = JsonConvert.DeserializeObject<Response>(responseString);
            invalidType = responseInfo.type;
        }
        throw new InvalidArgumentException(invalidType, file.Release);
      }
      
    }

    /// <summary>
    /// Displays the total file size of the single file
    /// </summary>
    public void DisplayTotalFileSize()
    {
      Utilities.DisplayTotalFileSize(SingleFileMetaData.FileSize, Inputs.Quiet);
    }

  }
}
