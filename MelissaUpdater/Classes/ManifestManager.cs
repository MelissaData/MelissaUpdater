using MelissaUpdater.Config;
using MelissaUpdater.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using MelissaUpdater.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace MelissaUpdater.Classes
{
  class ManifestManager
  {
    public List<ManifestFile> ManifestFiles { get; set; }
    public List<ManifestFile> ManifestFilesToUpdate { get; set; }

    public ManifestProgramInputs Inputs { get; set; }
    private readonly HttpClient Client;
    private readonly Stopwatch sw;

    /// <summary>
    /// Set Manifest Manager attributes from commandling parameters
    /// </summary>
    /// <param name="opts"></param>
    public ManifestManager(ManifestOptions opts)
    {
      Inputs = new ManifestProgramInputs(opts);
      Client = new HttpClient();
      sw = new Stopwatch();
    }

    /// <summary>
    /// Get hash and file size for a file in manifest
    /// </summary>
    /// <param name="index"></param>
    /// <param name="manifestFile"></param>
    /// <returns></returns>
    private async Task<(int, string, string)> GetHashAndFileSize(int index, ManifestFile manifestFile)
    {
      string url = manifestFile.Type.ToUpper() switch
      {
        "DATA" => URLFormatter.FormatDataUrl(manifestFile, Inputs.LicenseString),
        "LIBRARY" => URLFormatter.FormatLibraryUrl(manifestFile, Inputs.LicenseString),
        "INTERFACE" => URLFormatter.FormatInterfaceUrl(manifestFile, Inputs.LicenseString),
        _ => ""
      };

      HttpResponseMessage response = await Client.GetAsync(url);

      string responseString = await response.Content.ReadAsStringAsync();

      var responseInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
      string hash = responseInfo["SHA256"];
      string fileSize = responseInfo["FileSize"];

      return (index, hash, fileSize);
    }

    /// <summary>
    /// Get all files in the manifest
    /// </summary>
    /// <returns></returns>
    public async Task GetManifestContents()
    {
      sw.Restart();

      Utilities.Log($"Fetching manifest data for {Inputs.Product} for {Inputs.ReleaseVersion}", Inputs.Quiet);

      List<ManifestFile> manifestFiles = new List<ManifestFile>();

      string url = URLFormatter.FormatManifestContentsUrl(Inputs.ReleaseVersion, Inputs.Product, Inputs.LicenseString);

      HttpResponseMessage response = await Client.GetAsync(url);

      string responseString = await response.Content.ReadAsStringAsync();

	    if (response.StatusCode != HttpStatusCode.OK)
	    {
          string invalidType = "";
          if (!String.IsNullOrEmpty(responseString))
          {
              Response responseInfo = JsonConvert.DeserializeObject<Response>(responseString);
              invalidType = responseInfo.type;
          }       
	   	  throw new InvalidArgumentException(invalidType, Inputs.ReleaseVersion);
	    }

	    string[] urls = responseString.Split('\n');

      List<Task<(int, string)>> hashPromises = new List<Task<(int, string)>>();

      for (int i = 0; i < urls.Length; i++)
      {
        ManifestFile manifestFile = ParseManifestUrlSegment(urls[i]);

        int index = i;
        hashPromises.Add(Task.Run(async () => {
            var hashAndFileSize = await GetHashAndFileSize(index, manifestFile);
            manifestFile.FileSize = hashAndFileSize.Item3;
            return (hashAndFileSize.Item1, hashAndFileSize.Item2);
          }));

        manifestFiles.Add(manifestFile);
      }

      Task<(int, string)[]> getHashes = Task.WhenAll(hashPromises);

      getHashes.Wait();

      if (getHashes.Status == TaskStatus.RanToCompletion)
      {
        foreach ((int index, string hash) in getHashes.Result)
        {
          manifestFiles[index].SHA256 = hash;
        }
      }

      Utilities.Log("Finished fetching manifest data \n", Inputs.Quiet);

      ManifestFiles = manifestFiles;
    }

    /// <summary>
    /// Check if any file in the manifest needs to be updated
    /// </summary>
    /// <returns></returns>
    public async Task<bool> DetermineRequiredUpdates()
    {
      if (Inputs.Index)
      {
        return false;
      }
      sw.Restart();
      Utilities.Log($"Determining what to update for {Inputs.Product} for {Inputs.ReleaseVersion}", Inputs.Quiet);

      for (int i = ManifestFiles.Count - 1; i >= 0; i--)
      {
        string path = Path.Combine(Inputs.TargetDirectory, ManifestFiles[i].FilePath);
        try
        {
          string hash;
          if (!File.Exists(path))
          {
            continue;
          }
          if (File.Exists(path + ".hash"))
          {
            hash = await File.ReadAllTextAsync(path + ".hash");
          }
          else
          {
            if (Inputs.DryRun)
            {
              hash = Utilities.GetHashSha256(path);
            }
            else
            {
              await Utilities.CreateOrUpdateHashFile(path);
              hash = await File.ReadAllTextAsync(path + ".hash");
            }
          }

          if (string.IsNullOrEmpty(hash) || string.IsNullOrWhiteSpace(hash))
            throw new Exception("Hash from releases.net is empty");

          if (hash.Equals(ManifestFiles[i].SHA256) && File.Exists(path))
          {
            ManifestFiles.RemoveAt(i);
          }
        }
        catch (Exception ex) 
        {
          Utilities.Log(ex.Message, false);
        }
      }

      Utilities.Log($"Determined {ManifestFiles.Count} file(s) to be updated\n", Inputs.Quiet);

      return ManifestFiles.Count > 0;
    }

    /// <summary>
    /// Check if files exist and are up-to-date in the Working Directory
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CheckIfExistsInWorkingDirectory()
    {
      sw.Restart();
      ManifestFilesToUpdate = new List<ManifestFile>( ManifestFiles);

      Utilities.Log($"Checking Working Directory for { Inputs.Product} for {Inputs.ReleaseVersion}", Inputs.Quiet);

      int count = ManifestFiles.Count;

      for (int i = ManifestFiles.Count - 1; i >= 0; i--)
      {
        string path = Path.Combine(Inputs.WorkingDirectory, ManifestFiles[i].FilePath);
        try
        {
          if (File.Exists(path))
          {
            string hash;
            if (File.Exists(path + ".hash"))
            {
              hash = await File.ReadAllTextAsync(path + ".hash");
            }
            else
            {
              if(Inputs.DryRun)
              {
                hash = Utilities.GetHashSha256(path);
              }
              else
              {
                await Utilities.CreateOrUpdateHashFile(path);
                hash = await File.ReadAllTextAsync(path + ".hash");
              }
            }

            if (hash.Equals(ManifestFiles[i].SHA256))
            {
              ManifestFiles.RemoveAt(i);
              count--;
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
          
        }
        catch (Exception) { }
      }

      Utilities.Log($"Determined {count} file(s) to be updated for {Inputs.Product} for {Inputs.ReleaseVersion} in working directory\n", Inputs.Quiet);

      return ManifestFiles.Count == 0;
    }

    /// <summary>
    /// Without a map file, set file path as file name 
    /// </summary>
    public async Task JoinFilePathsNoMapFile()
    {
      sw.Restart();
      Utilities.Log($"Gathering filepath data for {Inputs.Product} for {Inputs.ReleaseVersion}", Inputs.Quiet);
      foreach (ManifestFile manifestFile in ManifestFiles)
      {
        manifestFile.FilePath = manifestFile.FileName;
      }
      Utilities.Log("Finished gathering filepath data\n", Inputs.Quiet);
      return ;
    }

    /// <summary>
    /// With a map file, set file path following the map file patterns
    /// </summary>
    /// <param name="mapFilePath"></param>
    /// <returns></returns>
    public async Task JoinFilePaths(string mapFilePath)
    {
      sw.Restart();
      Utilities.Log($"Gathering filepath data for {Inputs.Product} for {Inputs.ReleaseVersion}", Inputs.Quiet);
      string mapPath = "";
      // find path for specified .map file
      if (mapFilePath.StartsWith("."))    // relative path
      {
        string basePath = Path.GetDirectoryName(AppContext.BaseDirectory);
        mapPath = Path.GetFullPath(Path.Combine(basePath, mapFilePath));
      }
      else      // resticted path
      {
        mapPath = mapFilePath;
      }

      if (!File.Exists(mapPath))
      {
        throw new InvalidArgumentException("invalidMapMissing", mapPath);
      }
      string[] content = await File.ReadAllLinesAsync(mapPath);

      List<ManifestMapRow> manifestMapRows = new List<ManifestMapRow>();

      for (int i = 0; i < content.Length; i++)
      {
        string[] columns = content[i].Split('|');
        manifestMapRows.Add(
            new ManifestMapRow
            {
              FileName = columns[0],
              Type = columns[1].ToUpper() == "BINARY" ? "LIBRARY" : columns[1].ToUpper(),
              OS = columns[2].ToUpper(),
              Compiler = columns[3].ToUpper(),
              Architecture = columns[4].ToUpper(),
              FilePath = columns[5]
            }
        );
      }

      List<ManifestFile> joinedManifests;

      joinedManifests = ManifestFiles.GroupJoin(
            manifestMapRows, 
            file => new { file.FileName, file.Type, file.OS, file.Compiler, file.Architecture },
            maprow => new { maprow.FileName, maprow.Type, maprow.OS, maprow.Compiler, maprow.Architecture },
            (file, maprow) => new { manifestFile = file, mapRow = maprow }
          )
          .SelectMany(
            file => file.mapRow.DefaultIfEmpty(),
            (file, maprow) => new ManifestFile
            {
              Name = file.manifestFile.Name,
              FileName = file.manifestFile.FileName,
              Link = file.manifestFile.Link,
              Type = file.manifestFile.Type,
              OS = file.manifestFile.OS,
              Compiler = file.manifestFile.Compiler,
              Architecture = file.manifestFile.Architecture,
              Release = file.manifestFile.Release,
              SHA256 = file.manifestFile.SHA256,
              FileSize = file.manifestFile.FileSize,
              FilePath = maprow == null ? file.manifestFile.FilePath : maprow.FilePath
            }
          ).ToList();

      ManifestFiles = joinedManifests;

      // Display warning if any files were not on the map
      // and set the default file path to the file name
      List<ManifestFile> missingMapFiles = new List<ManifestFile>();
      for (int i = 0;  i < ManifestFiles.Count; i++)
      {
        if (ManifestFiles[i].FilePath == null)
        {
          missingMapFiles.Add(ManifestFiles[i]);
          ManifestFiles[i].FilePath = ManifestFiles[i].FileName;
        }
      }
      if ( missingMapFiles.Count > 0 )
      {
        Utilities.Log("!Warning! The following files were not on the given map:\n", Inputs.Quiet);
        DisplayManifestFilesTable(missingMapFiles);
        Utilities.Log("Please add them to the map before proceeding.\n", Inputs.Quiet);
        throw new InvalidManifestMapException("Incomplete map");
      }
      Utilities.Log("Finished gathering filepath data \n", Inputs.Quiet);
    }

    /// <summary>
    /// Download all files needed in the manifest
    /// </summary>
    /// <returns></returns>
    public async Task Download()
    {
      sw.Restart();

      Utilities.Log($"Downloading { ManifestFiles.Count} file(s) for {Inputs.Product} for {Inputs.ReleaseVersion}", Inputs.Quiet);

      string destinationBase =
          string.IsNullOrWhiteSpace(Inputs.WorkingDirectory)
              ? Inputs.TargetDirectory
              : Inputs.WorkingDirectory;

      int count = 0;
      foreach (ManifestFile manifestFile in ManifestFiles)
      {
        await DownloadFile(manifestFile, destinationBase, ++count, ManifestFiles.Count);
      }

      Utilities.Log("Finished downloading \n", Inputs.Quiet);
    }

    /// <summary>
    /// Download a file in the manifest
    /// </summary>
    /// <param name="manifestFile"></param>
    /// <param name="destinationBase"></param>
    /// <param name="count"></param>
    /// <param name="total"></param>
    /// <returns></returns>
    public async Task DownloadFile(ManifestFile manifestFile, string destinationBase, int count, int total)
    {

      string path = Path.Combine(destinationBase, manifestFile.FilePath);
      string hash = string.Empty;

      if (!Directory.Exists(Path.GetDirectoryName(path)))
      {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
      }

      using (var client = new HttpClientDownloadWithProgress(manifestFile.Link, path))
      {
        string progress = $"{count}/{total}";
        Utilities.Log($"Downloaded [{progress,7}] | Working on {manifestFile.FileName}", Inputs.Quiet);
        client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) => {
          Utilities.DownloadProgressStatus(totalFileSize, totalBytesDownloaded, progressPercentage, Inputs.Quiet);
        };
        hash = await client.StartDownload();
      }

      Utilities.Log("", Inputs.Quiet);
      await Utilities.CreateOrUpdateHashFile(path, manifestFile.FileName, manifestFile.SHA256, hash, Inputs.Quiet);
    }

    /// <summary>
    /// Move downloaded file(s) from the Working Directory to the Target Directory
    /// </summary>
    public void Update()
    {
      sw.Restart();

      // Update all files if Force mode
      List<ManifestFile> ManifestFilesToUpdateInTargetDir = Inputs.Force
        ? ManifestFiles
        : ManifestFilesToUpdate;

      Utilities.Log($"Updating {ManifestFilesToUpdateInTargetDir.Count} file(s) for {Inputs.Product} for {Inputs.ReleaseVersion}", Inputs.Quiet);

      string workingDirectory = string.IsNullOrWhiteSpace(Inputs.WorkingDirectory)
          ? Inputs.TargetDirectory
          : Inputs.WorkingDirectory;

      ManifestFilesToUpdateInTargetDir.ForEach(manifestFile =>
      {
        string from = Path.Combine(workingDirectory, manifestFile.FilePath);
        string to = Path.Combine(Inputs.TargetDirectory, manifestFile.FilePath);


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
      });

      Utilities.Log($"Finished updating \n", Inputs.Quiet);
    }

    /// <summary>
    /// Remove the file and hash file in the directory
    /// </summary>
    public void CleanUpTargetDirectory()
    {
      Utilities.Log("Cleaning up target directory", Inputs.Quiet);

      // Clean up all files if Force mode
      List<ManifestFile> ManifestFilesToClean = Inputs.Force
        ? ManifestFiles
        : ManifestFilesToUpdate;

      ManifestFilesToClean.ForEach(manifestFile =>
      {
        string path = Path.Combine(Inputs.TargetDirectory, manifestFile.FilePath);
        int attempts = 0;
        while (attempts++ < 3)
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
      });

      Utilities.Log("Finished cleaning up\n", Inputs.Quiet);

    }

    /// <summary>
    /// Parse download link for a file in the manifest
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private ManifestFile ParseManifestUrlSegment(string url)
    {
      ManifestFile manifestFile = new ManifestFile();

      Uri uri = new Uri(url);
      char[] charsToTrim = { '/', ' ' };

      manifestFile.Type = uri.Segments[2].Trim(charsToTrim).ToUpper();
      if (!manifestFile.Type.Equals("DATA"))
      {
        manifestFile.OS = uri.Segments[3].Trim(charsToTrim).ToUpper();
        manifestFile.Compiler = uri.Segments[4].Trim(charsToTrim).ToUpper();
        manifestFile.Architecture = uri.Segments[5].Trim(charsToTrim).ToUpper();
        manifestFile.Release = uri.Segments[6].Trim(charsToTrim);
        manifestFile.FileName = uri.Segments[7].Trim(charsToTrim);
      }
      else
      {
        manifestFile.OS = "ANY";
        manifestFile.Compiler = "ANY";
        manifestFile.Architecture = "ANY";
        manifestFile.Release = uri.Segments[3].Trim(charsToTrim);
        manifestFile.FileName = uri.Segments[4].Trim(charsToTrim);
      }

      manifestFile.Link = url;

      manifestFile.Name = Inputs.Product;

      return manifestFile;
    }

    /// <summary>
    /// List all files in the manifest
    /// </summary>
    public void ListManifestFiles()
    {
      DisplayManifestFilesTable(ManifestFiles);
    }

    /// <summary>
    /// Displays the total combined size of all files within the manifest.
    /// </summary>
    public void DisplayTotalFileSize()
    {
      long totalDownloadSize = 0;
      for (int i = 0; i < ManifestFiles.Count; i++)
      {
        totalDownloadSize += Convert.ToInt64(ManifestFiles[i].FileSize);
      }
      Utilities.DisplayTotalFileSize(totalDownloadSize, Inputs.Quiet);
    }

    /// <summary>
    /// Displays manifest files in a table format
    /// </summary>
    /// <param name="manifestFiles"></param>
    private void DisplayManifestFilesTable(List<ManifestFile> manifestFiles)
    {
      Utilities.Log($"\n{"Filename",20} | {"Type",10} | {"OS",10} | {"Compiler",10} | {"Architecture",12}", Inputs.Quiet);
      Utilities.Log(new string('-', 74), Inputs.Quiet);
      foreach (var mapFile in manifestFiles)
      {
        Utilities.Log($"{mapFile.FileName,20} | {(mapFile.Type == "LIBRARY" ? "BINARY" : mapFile.Type),10} | {mapFile.OS,10} | {mapFile.Compiler,10} | {mapFile.Architecture,12}", Inputs.Quiet);
      }
      Utilities.Log($"\n(Total of {manifestFiles.Count} files)\n", Inputs.Quiet);
    }

  }
}
