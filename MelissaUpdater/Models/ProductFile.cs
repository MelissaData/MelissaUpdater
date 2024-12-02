using System;
using System.Collections.Generic;
using System.Text;

namespace MelissaUpdater.Models
{
  public class ProductFile : IFile
  {
    public string Name { get; set; }
    public string FileName { get; set; }
    public string Link { get; set; }
    public string Type { get; set; }

    public string OS { get; set; }
    public string Compiler { get; set; }
    public string Architecture { get; set; }

    public string FilePath { get; set; }
    public string Release { get; set; }
    public string SHA256 { get; set; }
  }

  public class ProductFileMetaData
  {
    public string FileName { get; set; }
    public string Release { get; set; }
    public string BuildDate { get; set; }
    public string FileSize { get; set; }
    public string MD5 { get; set; }
    public string SHA256 { get; set; }

  }
}
