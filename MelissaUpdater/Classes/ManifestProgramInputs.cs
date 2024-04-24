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
      SetProcessCallBack(opts.ProcessCallBack);

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
        Console.WriteLine($"{e.Value} is not a valid product name");
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
        Console.WriteLine("License String cannot be empty. Check again or contact your sale representative for support.");
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

    void SetProcessCallBack(string processCallBack)
    {
      if (!string.IsNullOrEmpty(processCallBack))
      {
        ProcessCallBack = processCallBack;
      }
    }

  }
}
