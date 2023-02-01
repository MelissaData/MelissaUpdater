using MelissaUpdater.Config;
using MelissaUpdater.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
    /// Get hash for a file in manifest
    /// </summary>
    /// <param name="index"></param>
    /// <param name="manifestFile"></param>
    /// <returns></returns>
    private async Task<(int, string)> GetHash(int index, ManifestFile manifestFile)
    {
      string url = manifestFile.Type.ToUpper() switch
      {
        "DATA" => URLFormatter.FormatDataUrl(manifestFile, Inputs.LicenseString),
        "LIBRARY" => URLFormatter.FormatLibraryUrl(manifestFile, Inputs.LicenseString),
        "INTERFACE" => URLFormatter.FormatInterfaceUrl(manifestFile, Inputs.LicenseString),
        _ => ""
      };

      HttpResponseMessage response = await Client.GetAsync(url);

      string hash = await response.Content.ReadAsStringAsync();

      return (index, hash);
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

      string text = await response.Content.ReadAsStringAsync();

      string[] urls = text.Split('\n');

      List<Task<(int, string)>> hashPromises = new List<Task<(int, string)>>();

      for (int i = 0; i < urls.Length; i++)
      {
        ManifestFile manifestFile = ParseManifestUrlSegment(urls[i]);

        manifestFiles.Add(manifestFile);

        int index = i;
        hashPromises.Add(Task.Run(async () => await GetHash(index, manifestFile)));
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
      sw.Restart();
      Utilities.Log($"Determining what to update for {Inputs.Product} for {Inputs.ReleaseVersion}", Inputs.Quiet);

      for (int i = ManifestFiles.Count - 1; i >= 0; i--)
      {
        string path = Path.Combine(Inputs.TargetDirectory, ManifestFiles[i].FilePath);
        try
        {
          string hash;
          if (File.Exists(path + ".hash"))
          {
            hash = await File.ReadAllTextAsync(path + ".hash");
          }
          else
          {
            await Utilities.CreateOrUpdateHashFile(path);
            hash = await File.ReadAllTextAsync(path + ".hash");
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
          Console.WriteLine(ex.Message);
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
              await Utilities.CreateOrUpdateHashFile(path);
              hash = await File.ReadAllTextAsync(path + ".hash");
            }

            if (hash.Equals(ManifestFiles[i].SHA256))
            {
              ManifestFiles.RemoveAt(i);
              count--;
            }
            else
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
                Console.WriteLine($"Unable to delete files: {e.Message}.\n");
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
        string basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        mapPath = Path.Combine(basePath, mapFilePath);
      }
      else      // resticted path
      {
        mapPath = mapFilePath;
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
              Type = columns[1],
              OS = columns[2],
              Compiler = columns[3],
              Architecture = columns[4],
              FilePath = columns[5]
            }
        );
      }

      List<ManifestFile> joinedManifests;

      if (ManifestFiles.Count > 0 && ManifestFiles[0].Type.ToUpper().Equals("DATA"))
      {
        joinedManifests = ManifestFiles.GroupJoin(
          manifestMapRows, file => file.FileName, maprow => maprow.FileName,
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
            FilePath = maprow == null ? file.manifestFile.FilePath : maprow.FilePath
          }
        ).ToList();

      }
      else // Manifest group join for BINARY | INTERFACE
      {
        joinedManifests = ManifestFiles.GroupJoin(
            manifestMapRows, file => new { file.FileName, file.Type, file.OS, file.Compiler, file.Architecture }, 
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
              Type = maprow.Type,
              OS = maprow.OS,
              Compiler = maprow.Compiler,
              Architecture = maprow.Architecture,
              Release = file.manifestFile.Release,
              SHA256 = file.manifestFile.SHA256,
              FilePath = maprow == null ? "" : maprow.FilePath
            }
          ).ToList();
      }
      Utilities.Log("Finished gathering filepath data \n", Inputs.Quiet);

      ManifestFiles = joinedManifests;
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

      if (!Directory.Exists(Path.GetDirectoryName(path)))
      {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
      }

      using (var client = new HttpClientDownloadWithProgress(manifestFile.Link, path))
      {
        string progress = $"{count}/{total}";
        Utilities.Log($"Downloaded [{progress,7}] | Working on {manifestFile.FileName}", Inputs.Quiet);
        client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) => {
          DownloadProgressStatus(progressPercentage);
        };
        await client.StartDownload();
      }

      Utilities.Log("", Inputs.Quiet);
      await Utilities.CreateOrUpdateHashFile(path, manifestFile.FileName, manifestFile.SHA256, Inputs.Quiet);
    }

    /// <summary>
    /// Displays the operation identifier, and the transfer progress.
    /// </summary>
    /// <param name="progressPercentage"></param>
    private void DownloadProgressStatus(double? progressPercentage)
    {
      Utilities.LogErrorWithoutNewLine($"\r {progressPercentage,3} % complete...", Inputs.Quiet);
    }

    /// <summary>
    /// Move downloaded file(s) from the Working Directory to the Target Directory
    /// </summary>
    public void Update()
    {
      sw.Restart();
      Utilities.Log($"Updating {ManifestFilesToUpdate.Count} file(s) for {Inputs.Product} for {Inputs.ReleaseVersion}", Inputs.Quiet);

      string workingDirectory = string.IsNullOrWhiteSpace(Inputs.WorkingDirectory)
          ? Inputs.TargetDirectory
          : Inputs.WorkingDirectory;

      ManifestFilesToUpdate.ForEach(manifestFile =>
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
            Console.WriteLine(ex.Message);
            if (attempts == 3)
            {
              Console.WriteLine("Maximum number of attempts made.");
              Console.WriteLine("Please close the associated file and restart this program.");
              Console.WriteLine("This program will pick up where it left off.");
            }
            else
            {
              Console.WriteLine($"[{attempts}] Please close the file and press any key to proceed");
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

      ManifestFilesToUpdate.ForEach(manifestFile =>
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
            Console.WriteLine(ex.Message);
            if (attempts == 3)
            {
              Console.WriteLine("Maximum number of attempts made.");
              Console.WriteLine("Please close the associated file and restart this program.");
              Console.WriteLine("This program will pick up where it left off.");
            }
            else
            {
              Console.WriteLine($"[{attempts}] Please close the file and press any key to proceed");
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

      manifestFile.Type = uri.Segments[2].Trim(charsToTrim);
      if (!manifestFile.Type.ToLower().Equals("data"))
      {
        manifestFile.OS = uri.Segments[3].Trim(charsToTrim);
        manifestFile.Compiler = uri.Segments[4].Trim(charsToTrim);
        manifestFile.Architecture = uri.Segments[5].Trim(charsToTrim);
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
      for (int i = ManifestFiles.Count - 1; i >= 0; i--)
      {
        Utilities.Log(ManifestFiles[i].FileName, Inputs.Quiet);
      }
    }

  }
}
