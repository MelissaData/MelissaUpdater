using System;
using System.Collections.Generic;
using System.Text;

namespace MelissaUpdater.Models
{
  class ManifestMapRow
  {
    public string FileName { get; set; }
    public string Type { get; set; }
    public string OS { get; set; }
    public string Compiler { get; set; }
    public string Architecture { get; set; }

    public string FilePath { get; set; }
  }
}
