using System;
using System.Collections.Generic;
using System.Text;

namespace MelissaUpdater.Classes
{
  class VerifyProgramInputs
  {
    public string Path { get; set; }
    public bool Quiet { get; set; }
    public string ProcessCallBack { get; set; }

    /// <summary>
    /// Set Verify inputs from the commandlind parameters
    /// </summary>
    /// <param name="opts"></param>
    public VerifyProgramInputs(Config.VerifyOptions opts)
    {
      SetPath(opts.Path);
      SetQuiet(opts.Quiet);
      SetProcessCallBack(opts.ProcessCallBack);
    }

    void SetPath(string pathFromOpts)
    {
      if(!string.IsNullOrEmpty(pathFromOpts))
      {
        Path = pathFromOpts;
      }
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

  }
}
