using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace MelissaUpdater.Config
{
  [Verb("verify", HelpText = "Verify a file or folder path.")]

  class VerifyOptions
  {
    [Option('p', "path", HelpText = "The file or folder path that you wish to verify.")]
    public string Path { get; set; }

    [Option('q', "quiet", HelpText = "Run the program in quiet mode without console output except for errors.")]
    public bool Quiet { get; set; }

    [Option('x', "callback", HelpText = "Action command for the next script or process to run.")]
    public string ProcessCallBack { get; set; }

  }
}
