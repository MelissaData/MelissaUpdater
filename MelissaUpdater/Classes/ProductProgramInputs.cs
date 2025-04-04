using MelissaUpdater.Config;
using MelissaUpdater.Exceptions;
using System;
using System.IO;

namespace MelissaUpdater.Classes
{
  class ProductProgramInputs
  {
    public string Product { get; set; }
    public string ReleaseVersion { get; set; }
    public string LicenseString { get; set; }
    public string TargetDirectory { get; set; }
    public string WorkingDirectory { get; set; }
    public bool Force { get; set; }
    public bool DryRun { get; set; }
    public bool Quiet { get; set; }
    public string ProcessCallBack { get; set; }

    /// <summary>
    /// Set Product attributes from commandline parameters
    /// </summary>
    /// <param name="opts"></param>
    public ProductProgramInputs(ProductOptions opts)
    {
      SetProduct(opts.Product);
      SetRelease(opts.ReleaseVersion);
      SetLicenseString(opts.License);
      SetTargetDirectory(opts.TargetDirectory);
      SetWorkingDirectory(opts.WorkingDirectory);
      SetForce(opts.Force);
      SetDryRun(opts.DryRun);
	  SetQuiet(opts.Quiet);
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
	  
      if (conflict)
      {
        Utilities.Log("\nUnable to start program, please check above for details.", false);
        Environment.Exit(1);
        return;
      }
    }

  }
}
