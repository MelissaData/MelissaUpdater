using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace MelissaUpdater.Config
{
  [Verb("file", HelpText = "Download a single file.")]
  class FileOptions
  {
    [Option('a', "architecture", HelpText = "The specific architecture for the binary file (64BIT, 32BIT, ANY).")]
    public string Architecture { get; set; }

    [Option('c', "compiler", HelpText = "The specific compiler for the binary file (ACC3, ANY, C, COM, DLL, GCC48, GCC46, GCC48, GCC83, JAVA, MSSQL, NET, PERL, PHP, PHP7, PLSQL, PYTHON, RUBY, SSIS2005, SSIS2008, SSIS2012, SSIS2014, SSIS2016, SSIS2017, SSIS2019, WS12, WS6, XLC12, XLC6).")]
    public string Compiler { get; set; }

    [Option('d', "dry_run", HelpText = "Simulate the process without modifying any files.")]
    public bool DryRun { get; set; }

    [Option('f', "force", HelpText = "Force the download and overwrite existing file(s).")]
    public bool Force { get; set; }

    [Option('l', "license", HelpText = "The valid Melissa license string for the product you wish to download.")]
    public string License { get; set; }

    [Option('n', "filename", HelpText = "The filename to download.")]
    public string FileName { get; set; }

    [Option('o', "os", HelpText = "The specific operating system for the binary file (AIX, ANY, HPUX_IT, HPUX_PA, LINUX, SOLARIS, WINDOWS, ZLINUX).")]
    public string OperatingSystem { get; set; }
    
    [Option('q', "quiet", HelpText = "Run the program in quiet mode without console output except for errors.")]
    public bool Quiet { get; set; }

    [Option('r', "release_version", HelpText = "The release version (YYYY.MM, YYYY.Q#, LATEST) for the product you wish to download (e.g. \"2023.01\" or \"2023.Q1\" or \"LATEST\").")]
    public string ReleaseVersion { get; set; }

    [Option('t', "target_directory", HelpText = "The target directory where to place the downloaded file(s). If not specified, the default is the current directory.")]
    public string TargetDirectory { get; set; }

    [Option('w', "working_directory", HelpText = "The working directory where to temporarily stage downloaded file(s) before moving into the target directory. ")]
    public string WorkingDirectory { get; set; }

    [Option('y', "type", HelpText = "The specific file type to be downloaded (BINARY, DATA, INTERFACE).")]
    public string Type { get; set; }

    [Option('x', "callback", HelpText = "Action command for the next script or process to run.")]
    public string ProcessCallBack { get; set; }


  }
}

