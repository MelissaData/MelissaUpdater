using System;
using System.Collections.Generic;
using System.Text;

namespace MelissaUpdater.Exceptions
{
  class InvalidManifestMapException : Exception
  {
    public string Value { get; set; }
    public InvalidManifestMapException() { }
    public InvalidManifestMapException(string value)
    {
      Value = value;
    }
  }
}
