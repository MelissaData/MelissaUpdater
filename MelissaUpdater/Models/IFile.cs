
namespace MelissaUpdater.Models
{
  public interface IFile
  {
    public string FileName { get; set; }
    public string Release { get; set; }

    public string Type { get; set; }
    public string OS { get; set; }
    public string Compiler { get; set; }
    public string Architecture { get; set; }

    public string FilePath { get; set; }
    public string SHA256 { get; set; }
  }
}
