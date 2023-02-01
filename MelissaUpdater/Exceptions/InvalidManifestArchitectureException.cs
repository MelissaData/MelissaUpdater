using System;
using System.Collections.Generic;
using System.Text;

namespace MelissaUpdater.Exceptions
{
    class InvalidManifestArchitectureException : Exception
    {
        public string Value { get; set; }
        public InvalidManifestArchitectureException () { }
        public InvalidManifestArchitectureException (string value)
        {
            Value = value;
        }
    }
}
