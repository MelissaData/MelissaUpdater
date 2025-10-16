using MelissaUpdater.Config;
using MelissaUpdater.Exceptions;
using System;
using System.IO;

namespace MelissaUpdater.Classes
{
  class FileProgramInputs
  {
    public string FileName { get; set; }
    public string ReleaseVersion { get; set; }
    public string LicenseString { get; set; }
    public string Type { get; set; }
    public string OperatingSystem { get; set; }
    public string Compiler { get; set; }
    public string Architecture { get; set; }

    public string TargetDirectory { get; set; }
    public string WorkingDirectory { get; set; }
    public string Tag { get; set; }
    public bool Force { get; set; }
    public bool DryRun { get; set; }
    public bool Quiet { get; set; }

    public string ProcessCallBack { get; set; }

    /// <summary>
    /// Set single file's attributes from commandline parameters
    /// </summary>
    /// <param name="opts"></param>
    public FileProgramInputs(Config.FileOptions opts)
    {
      SetFileName(opts.FileName);
      SetRelease(opts.ReleaseVersion);
      SetLicenseString(opts.License);
      SetType(opts.Type);
      SetOperatingSystem(opts.OperatingSystem);
      SetCompiler(opts.Compiler);
      SetArchitecture(opts.Architecture);
      SetTargetDirectory(opts.TargetDirectory);
      SetWorkingDirectory(opts.WorkingDirectory);
      SetTag(opts.Tag);
      SetForce(opts.Force);
      SetDryRun(opts.DryRun);
      SetQuiet(opts.Quiet);
      SetProcessCallBack(opts.ProcessCallBack);
      CheckForConflictFlags();
    }

    void SetFileName(string fileNameFromOpts)
    {
      string fileName = "";
      if (!string.IsNullOrWhiteSpace(fileNameFromOpts))
      {
        fileName = fileNameFromOpts;
      }
      FileName = fileName;
    }

    public void SetType(string typeFromOpts)
    {
      string typeString = "DATA";
      if (!string.IsNullOrWhiteSpace(typeFromOpts))
      {
        typeString = typeFromOpts;
      }
      try
      {
        if (typeString != "DATA" && typeString != "BINARY" && typeString != "INTERFACE")
        {
          throw new InvalidManifestTypeException(typeString);
        }
				else Type = typeString;
			}
      catch (InvalidManifestTypeException e)
      {
        Utilities.Log($"'{e.Value}' is not a valid file type", false);
        throw new InvalidArgumentException(e.Value);
      }
    }

    void SetOperatingSystem(string operatingSystemFromOpts)
    {
      string operatingSystemString = "ANY";
      if (!string.IsNullOrWhiteSpace(operatingSystemFromOpts))
      {
        operatingSystemString = operatingSystemFromOpts;
      }
      try
      {
        OperatingSystem = operatingSystemString;
      }
      catch (InvalidManifestOperatingSystemException e)
      {
        Utilities.Log($"{e.Value} is not a valid operating system", false);
        throw;
      }
    }

    void SetCompiler(string compilerFromOpts)
    {
      string compilerString = "ANY";
      if (!string.IsNullOrWhiteSpace(compilerFromOpts))
      {
        compilerString = compilerFromOpts;
      }
      try
      {
        Compiler = compilerString;
      }
      catch (InvalidManifestCompilerException e)
      {
        Utilities.Log($"{e.Value} is not a valid compiler", false);
        throw;
      }
    }

    void SetArchitecture(string architectureFromOpts)
    {
      string architectureString = "ANY";
      if (!string.IsNullOrWhiteSpace(architectureFromOpts))
      {
        architectureString = architectureFromOpts;
      }
      try
      {
        Architecture = architectureString;
      }
      catch (InvalidManifestArchitectureException e)
      {
        Utilities.Log($"{e.Value} is not a valid architecture", false);
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
        Utilities.Log("License String cannot be empty.\nCheck again or contact your sales representative for support.", false);
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
      WorkingDirectory =  "";
      if (!string.IsNullOrWhiteSpace(workingDirectoryFromOpts)
          && string.IsNullOrWhiteSpace(WorkingDirectory))
      {
        WorkingDirectory = workingDirectoryFromOpts;
      }
    }

    void SetTag(string tagFromOpts)
    {
      Tag = "";
      if (!string.IsNullOrWhiteSpace(tagFromOpts)
          && string.IsNullOrWhiteSpace(Tag))
      {
        Tag = tagFromOpts;
      }
    }

    void SetRelease(string releaseFromOpts)
    {
      ReleaseVersion =  "";
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
