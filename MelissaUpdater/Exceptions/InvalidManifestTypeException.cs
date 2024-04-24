using System;
using System.Collections.Generic;
using System.Text;

namespace MelissaUpdater.Exceptions
{
  class InvalidManifestTypeException : Exception
{
    public string Value { get; set; }
    public InvalidManifestTypeException () { }
    public InvalidManifestTypeException (string value)
    {
      Value = value;
    }
  }
}
