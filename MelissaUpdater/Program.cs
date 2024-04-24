using CommandLine;
using MelissaUpdater.Classes;
using MelissaUpdater.Config;
using MelissaUpdater.Exceptions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

namespace MelissaUpdater
{
  class Program
  {
    static void Main(string[] args)
    {
      Parser.Default.ParseArguments<FileOptions, ManifestOptions, VerifyOptions>(args)
          .WithParsed<FileOptions>(opts => RunFileOptions(opts))
          .WithParsed<ManifestOptions>(opts => RunManifestOptions(opts))
          .WithParsed<VerifyOptions>(opts => RunVerifyOptions(opts));
      
      // Download a manifest of files
      static void RunManifestOptions(ManifestOptions opts)
      {
        ManifestManager manager;
        Stopwatch sw = Stopwatch.StartNew();

        Utilities.Log($"\nStarting Program @[{DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt")}]\n", opts.Quiet);

        try
        {
          manager = new ManifestManager(opts);
        }
        catch (Exception ex)
        {
          Utilities.LogError($"\n{ex.ToString()}", opts.Quiet);
          Utilities.Log("\nUnable to start program, please check above for details.", opts.Quiet);
          Environment.Exit(1); 
          return;
        }
        try
        {                                                                                    
          // Get manifest files for product
          manager.GetManifestContents().Wait();

          // Check if Map directory is valid and append the filepaths from map files, otherwise use the current filepath
          if (!string.IsNullOrEmpty(manager.Inputs.Map))
          {
            manager.JoinFilePaths(manager.Inputs.Map).Wait();
          }
          else
          {
            manager.JoinFilePathsNoMapFile().Wait();
          }

          // Check for Working Directory to download to a staging directory before moving to the Target Directory
          if (!string.IsNullOrWhiteSpace(manager.Inputs.WorkingDirectory))
          {
            if (manager.DetermineRequiredUpdates().Result)
            {
              if (manager.CheckIfExistsInWorkingDirectory().Result)
              {
                manager.CleanUpTargetDirectory();
                manager.Update();
              }
              else
              {
                manager.Download().Wait();
                manager.CleanUpTargetDirectory();
                manager.Update();
              }
            }
          }
          else
          {
            // Check if Force download is enabled, and download the accordingly version, if not, download the latest version
            if (manager.Inputs.Force)
            {
              manager.Download().Wait();
            }
            // Check if Dry Run is enable, and complete every steps without actually downloading
            if (manager.Inputs.DryRun)
            {
              manager.DetermineRequiredUpdates().Wait();
              manager.ListManifestFiles();
            }
            // Check if Index is enable, display files list without downloading
            if (manager.Inputs.Index)
            {
              Utilities.Log($"Available file(s) in '{manager.Inputs.Product}' manifest:", manager.Inputs.Quiet);
              manager.ListManifestFiles();
            }

            if (!manager.Inputs.DryRun && manager.DetermineRequiredUpdates().Result && !manager.Inputs.Index)
            {
              manager.Download().Wait();
            }
          }

          if (!string.IsNullOrEmpty(manager.Inputs.ProcessCallBack))
          {
            string inputProcess = manager.Inputs.ProcessCallBack;
            string inputArgs = "";
            if (inputProcess.Contains(","))
              inputArgs = Utilities.ParseProcessArguments(ref inputProcess);
            Utilities.ExecuteProcess(inputProcess, inputArgs);
          }

        }
        catch (Exception ex)
        {
          if (ex is AggregateException && ex.InnerException is InvalidArgumentException)
          {
	          Utilities.LogError(ex.InnerException.Message, opts.Quiet);
          }
          else
          {
	          Utilities.Log($"\n{ex.ToString()}", opts.Quiet);
          }
          Utilities.Log("\nUnable to start program, please check above for details.", opts.Quiet);
          Environment.Exit(1);
          return;
        }

        Utilities.Log($"Ending Program @[{DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt")}]\n", opts.Quiet);

        Environment.Exit(0);
      }

      // Download a single file
      static void RunFileOptions(FileOptions opts)
      {
        SingleFileManager manager;

        Stopwatch sw = Stopwatch.StartNew();
        Utilities.Log($"\nStarting Program @[{DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt")}]\n", opts.Quiet);

        try
        {
          manager = new SingleFileManager(opts);
        }
        catch (Exception ex)
        {
          if(ex is AggregateException && ex.InnerException is InvalidArgumentException)
          {
            Utilities.LogError(ex.InnerException.Message, opts.Quiet);
          }
          else
          {
            if (ex.InnerException != null)
            {
							Utilities.LogError($"\n{ex.InnerException.Message}", opts.Quiet);
						}
            else
            {
							Utilities.LogError($"\n{ex.Message}", opts.Quiet);
						}
          }
          Utilities.Log("\nUnable to start program, please check above for details.", opts.Quiet);
          Environment.Exit(1);
          return;
        }

        try
        {
          manager.JoinFilePath();
          if (!string.IsNullOrWhiteSpace(manager.Inputs.WorkingDirectory))
          {
            if (manager.DetermineRequiredUpdates().Result)
            {
              if (manager.CheckIfExistsInWorkingDirectory().Result)
              {
								manager.CleanUpTargetDirectory();
								manager.Update();           
              }
              else
              {
                manager.Download().Wait();
								if (manager.Inputs.WorkingDirectory != manager.Inputs.TargetDirectory)
								{
									manager.CleanUpTargetDirectory();
									manager.Update();
								}
							}
            }
          }
          else
          {
            if (manager.Inputs.Force)
            {
              manager.Download().Wait();
            }
            if (manager.Inputs.DryRun)
            {
              manager.DetermineRequiredUpdates().Wait();
            }
            else if (!manager.Inputs.DryRun && manager.DetermineRequiredUpdates().Result)
            {
              manager.Download().Wait();
            }
          }

          if (!string.IsNullOrEmpty(manager.Inputs.ProcessCallBack))
          {
            string inputProcess = manager.Inputs.ProcessCallBack;
            string inputArgs = "";
            if (inputProcess.Contains(","))
              inputArgs = Utilities.ParseProcessArguments(ref inputProcess);
            Utilities.ExecuteProcess(inputProcess, inputArgs);
          }
        }
        catch (Exception ex)
        {
					if (ex is AggregateException && ex.InnerException is InvalidArgumentException)
					{
						Utilities.LogError(ex.InnerException.Message, opts.Quiet);
					}
					else
					{
						Utilities.LogError($"\n{ex.ToString()}", opts.Quiet);
					}
					Utilities.Log("\nUnable to start program, please check above for details.", opts.Quiet);
					Environment.Exit(1);
					return;
				}

        Utilities.Log($"Ending Program @[{DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt")}]\n", opts.Quiet);

        Environment.Exit(0);
      }

      // Verify hash(es) of a file or a folder path
      static void RunVerifyOptions(VerifyOptions opts)
      {
        VerifyManager manager;

        Stopwatch sw = Stopwatch.StartNew();
        Utilities.Log($"\nStarting Program @[{DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt")}]\n", opts.Quiet);
        int exit = 0;
        try
        {
          manager = new VerifyManager(opts);
        }
        catch (Exception ex)
        {
          Utilities.LogError($"{ex.ToString()}", opts.Quiet);
          Utilities.Log("\nUnable to start program, please check above for details.", opts.Quiet);
          Environment.Exit(1);
          return;
        }

        try
        {
          exit = manager.CheckHash();

          if (!string.IsNullOrEmpty(manager.Inputs.ProcessCallBack))
          {
            string inputProcess = manager.Inputs.ProcessCallBack;
            string inputArgs = "";
            if (inputProcess.Contains(","))
              inputArgs = Utilities.ParseProcessArguments(ref inputProcess);
            Utilities.ExecuteProcess(inputProcess, inputArgs);
          }
        }
        catch (Exception ex)
        {
          Utilities.LogError($"{ex.Message}", opts.Quiet);
          Environment.Exit(1);
        }

        Utilities.Log($"\nEnding Program @[{DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt")}]\n", opts.Quiet);
        if (exit > 0)
          Environment.Exit(1);
        Environment.Exit(0);
      }
    }
  }
}
