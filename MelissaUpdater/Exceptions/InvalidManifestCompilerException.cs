using System;
using System.Collections.Generic;
using System.Text;

namespace MelissaUpdater.Exceptions
{
  class InvalidManifestCompilerException : Exception
  {
    public string Value { get; set; }
    public InvalidManifestCompilerException () { }
    public InvalidManifestCompilerException (string value)
    {
      Value = value;
    }
  }
}
