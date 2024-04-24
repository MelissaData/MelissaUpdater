using System;
using System.Collections.Generic;
using System.Text;

namespace MelissaUpdater.Exceptions
{
  class InvalidManifestOperatingSystemException : Exception
  {
    public string Value { get; set; }
    public InvalidManifestOperatingSystemException () { }
    public InvalidManifestOperatingSystemException (string value)
    {
      Value = value;
    }
  }
}
