using System;
using System.Collections.Generic;
using System.Text;

namespace MelissaUpdater.Exceptions
{
  class InvalidManifestNameException : Exception
  {
    public string Value { get; set; }
    public InvalidManifestNameException () { }
    public InvalidManifestNameException (string value)
    {
      Value = value;
    }
  }
}
