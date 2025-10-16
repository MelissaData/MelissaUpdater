﻿using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace MelissaUpdater.Config
{
  [Verb("product", HelpText = "Download a ZIP file of a specific product.")]
  class ProductOptions
  {
    [Option('d', "dry_run", HelpText = "Simulate the process without modifying any files.")]
    public bool DryRun { get; set; }

    [Option('f', "force", HelpText = "Force the download and overwrite existing file(s).")]
    public bool Force { get; set; }

    [Option('l', "license", HelpText = "The valid Melissa license string for the product you wish to download.")]
    public string License { get; set; }

    [Option('p', "product", HelpText = "The product name to be downloaded.")]
    public string Product { get; set; }

    [Option('q', "quiet", HelpText = "Run the program in quiet mode without console output except for errors.")]
    public bool Quiet { get; set; }

    [Option('r', "release_version", HelpText = "The release version (YYYY.MM, YYYY.Q#, LATEST) for the product you wish to download (e.g. \"2023.01\" or \"2023.Q1\" or \"LATEST\").")]
    public string ReleaseVersion { get; set; }

    [Option('t', "target_directory", HelpText = "The target directory where to place the downloaded file(s). If not specified, the default is the current directory.")]
    public string TargetDirectory { get; set; }

    [Option('w', "working_directory", HelpText = "The working directory where to temporarily stage downloaded file(s) before moving into the target directory.")]
    public string WorkingDirectory { get; set; }

    [Option('x', "callback", HelpText = "Action command for the next script or process to run.")]
    public string ProcessCallBack { get; set; }

    [Option("tag", HelpText = "Select the product with a specific tag.")]
    public string Tag { get; set; }
  }
}

