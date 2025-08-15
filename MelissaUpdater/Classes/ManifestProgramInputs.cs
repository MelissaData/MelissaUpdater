using MelissaUpdater.Config;
using MelissaUpdater.Exceptions;
using System;
using System.IO;

namespace MelissaUpdater.Classes
{
  class ManifestProgramInputs
  {
    public string Product { get; set; }
    public string ReleaseVersion { get; set; }
    public string LicenseString { get; set; }
    public string TargetDirectory { get; set; }
    public string WorkingDirectory { get; set; }
    public bool Index { get; set; }
    public bool Force { get; set; }
    public bool DryRun { get; set; }
    public bool Quiet { get; set; }
    public string Map { get; set; }
    public bool GenerateMap { get; set; }
    public string GenerateMapPath { get; set; }

    public string ProcessCallBack { get; set; }

    /// <summary>
    /// Set Manifest attributes from commandline parameters
    /// </summary>
    /// <param name="opts"></param>
    public ManifestProgramInputs(ManifestOptions opts)
    {
      SetProduct(opts.Product);
      SetRelease(opts.ReleaseVersion);
      SetLicenseString(opts.License);
      SetTargetDirectory(opts.TargetDirectory);
      SetWorkingDirectory(opts.WorkingDirectory);
      SetForce(opts.Force);
      SetDryRun(opts.DryRun);
			SetQuiet(opts.Quiet);
      SetIndex(opts.Index);
      SetMap(opts.Map);
      SetGenerateMap(opts.GenerateMap);
      SetGenerateMapPath(opts.GenerateMap);
      SetProcessCallBack(opts.ProcessCallBack);
      CheckForConflictFlags();
    }

    void SetProduct(string productFromOpts)
    {
      string product = "";
      if (!string.IsNullOrWhiteSpace(productFromOpts))
      {
        product = productFromOpts;
      }
      try
      {
        Product = product;
      }
      catch (InvalidManifestNameException e)
      {
        Utilities.Log($"{e.Value} is not a valid product name", false);
        throw;
      }
    }
    
    void SetLicenseString(string licenseFromOpts)
    {
      if (!string.IsNullOrWhiteSpace(licenseFromOpts))
      {
        LicenseString = licenseFromOpts;
      }
      else  //check environment variable
      {
        LicenseString = Environment.GetEnvironmentVariable("MD_LICENSE");
      }
      if (string.IsNullOrWhiteSpace(LicenseString))
      {
        Utilities.Log("License String cannot be empty. Check again or contact your sale representative for support.", false);
        throw new Exception();
      }
    }

    void SetTargetDirectory(string targetDirectoryFromOpts)
    {
      if (!string.IsNullOrWhiteSpace(targetDirectoryFromOpts))
      {
        TargetDirectory = targetDirectoryFromOpts;
      }
      else
      {
        TargetDirectory = Directory.GetCurrentDirectory();
      }
    }

    void SetWorkingDirectory(string workingDirectoryFromOpts)
    {
      WorkingDirectory = "";
      if (!string.IsNullOrWhiteSpace(workingDirectoryFromOpts))
      {
        WorkingDirectory = workingDirectoryFromOpts;
      }
    }

    void SetRelease(string releaseFromOpts)
    {
      ReleaseVersion = "";
      if (!string.IsNullOrWhiteSpace(releaseFromOpts))
      {
        ReleaseVersion = releaseFromOpts;
      }
      // If not provided, fetch newest release from api
      if (string.IsNullOrWhiteSpace(ReleaseVersion))
      {
        ReleaseVersion = "latest";
      }
    }

    void SetForce(bool force)
    {
      Force = force;
    }

    void SetDryRun(bool dryrun)
    {
      DryRun = dryrun;
		}

		void SetQuiet(bool quiet)
    {
      Quiet = quiet;
    }

    void SetIndex(bool index)
    {
      Index = index;
		}

    void SetMap(string mapFromOpts)
    {
      Map = "";
      if (!string.IsNullOrWhiteSpace(mapFromOpts))
      {
        Map = mapFromOpts;
      }
    }
    void SetGenerateMap(string generateMap)
    {
      if (generateMap == null)
      {
        GenerateMap = false;
      }
      else
      {
        GenerateMap = true;
      }
    }
    void SetGenerateMapPath(string generateMapPath)
    {
      GenerateMapPath = "";
      if (!string.IsNullOrWhiteSpace(generateMapPath))
      {
        GenerateMapPath = generateMapPath;
      }

      // Append default filename if filename not specified
      if (!Path.HasExtension(GenerateMapPath))
      {
        GenerateMapPath = Path.Combine(GenerateMapPath, $"{Product}.map");
      }
    }

    void SetProcessCallBack(string processCallBack)
    {
      if (!string.IsNullOrEmpty(processCallBack))
      {
        ProcessCallBack = processCallBack;
      }
    }
    void CheckForConflictFlags()
    {
      bool conflict = false;
      // Check for conflict flags
      if (DryRun && Force)
      {
        Utilities.Log("Force Mode and Dry Run Mode cannot be chosen at the same time. Please try again.", false);
        conflict = true;
      }
      else if (Quiet && Index)
      {
        Utilities.Log("Quiet Mode and Index Mode cannot be chosen at the same time. Please try again.", false);
        conflict = true;
      }

			if (conflict)
      {
        Utilities.Log("\nUnable to start program, please check above for details.", false);
        Environment.Exit(1);
        return;
      }
    }

  }
}
