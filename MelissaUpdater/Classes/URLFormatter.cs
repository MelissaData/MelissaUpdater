using MelissaUpdater.Models;
using System.Web;

namespace MelissaUpdater.Classes
{
  public static class URLFormatter
  {
    public static readonly string fileHashDataScaffolding =
        "https://releases.melissadata.net/sha256/Data/{0}/{1}/?id={2}";
    public static readonly string fileHashLibraryScaffolding =
        "https://releases.melissadata.net/sha256/Library/{0}/{1}/{2}/{3}/{4}/?id={5}";
    public static readonly string fileHashInterfaceScaffolding =
        "https://releases.melissadata.net/sha256/Interface/{0}/{1}/{2}/{3}/{4}?id={5}";

    public static readonly string fileMetaDataScaffolding =
        "https://releases.melissadata.net/metadata/Data/{0}/{1}/?id={2}";
    public static readonly string fileMetaLibraryScaffolding =
        "https://releases.melissadata.net/metadata/Library/{0}/{1}/{2}/{3}/{4}/?id={5}";
    public static readonly string fileMetaInterfaceScaffolding =
        "https://releases.melissadata.net/metadata/Interface/{0}/{1}/{2}/{3}/{4}?id={5}";


    public static readonly string manifestDownloadDataScaffolding =
        "https://releases.melissadata.net/Download/Data/{0}/{1}/?id={2}";
    public static readonly string manifestDownloadLibraryScaffolding =
        "https://releases.melissadata.net/Download/Library/{0}/{1}/{2}/{3}/{4}/?id={5}";
    public static readonly string manifestDownloadInterfaceScaffolding =
        "https://releases.melissadata.net/Download/Interface/{0}/{1}/{2}/{3}/{4}?id={5}";

    private static readonly string manifestContentScaffolding =
        "https://releases.melissadata.net/Manifest/{0}/{1}/?id={2}&format=lfList";
    private static readonly string manifestHashDataScaffolding =
        "https://releases.melissadata.net/sha256/Data/{0}/{1}/?id={2}";
    private static readonly string manifestHashLibraryScaffolding =
        "https://releases.melissadata.net/sha256/Library/{0}/{1}/{2}/{3}/{4}/?id={5}";
    private static readonly string manifestHashInterfaceScaffolding =
        "https://releases.melissadata.net/sha256/Interface/{0}/{1}/{2}/{3}/{4}?id={5}";

	public static readonly string productDownloadScaffolding =
            "https://releases.melissadata.net/Download/Product/{0}/{1}?id={2}";
    public static readonly string productMetaScaffolding =
        "https://releases.melissadata.net/metadata/Product/{0}/{1}?id={2}";
    private static readonly string productHashScaffolding =
        "https://releases.melissadata.net/sha256/PRODUCT/{0}/{1}?id={2}";

	/// <summary>
	/// Get the url to retrieve all files in a manifest
	/// </summary>
	/// <param name="releaseVersion"></param>
	/// <param name="product"></param>
	/// <param name="licenseString"></param>
	/// <returns></returns>
	public static string FormatManifestContentsUrl(string releaseVersion, string product, string licenseString)
    {
    string release = string.IsNullOrWhiteSpace(releaseVersion) ? "latest" : releaseVersion;

    return string.Format(manifestContentScaffolding, release, product, HttpUtility.UrlEncode(licenseString));
    }

	/// <summary>
	/// Get the url to download a data file
	/// </summary>
	/// <param name="manifestFile"></param>
	/// <param name="licenseString"></param>
	/// <returns></returns>
	public static string FormatDataDownloadUrl(IFile manifestFile, string licenseString)
    {
      return string.Format(
          manifestDownloadDataScaffolding,
          manifestFile.Release,
          manifestFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

    /// <summary>
    /// Get the url to download a library file
    /// </summary>
    /// <param name="manifestFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatLibraryDownloadUrl(IFile manifestFile, string licenseString)
    {
      return string.Format(
          manifestDownloadLibraryScaffolding,
          manifestFile.OS,
          manifestFile.Compiler,
          manifestFile.Architecture,
          manifestFile.Release,
          manifestFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

    /// <summary>
    /// Get the url to download an interface file
    /// </summary>
    /// <param name="manifestFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatInterfaceDownloadUrl(IFile manifestFile, string licenseString)
    {
      return string.Format(
          manifestDownloadInterfaceScaffolding,
          manifestFile.OS,
          manifestFile.Compiler,
          manifestFile.Architecture,
          manifestFile.Release,
          manifestFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

    /// <summary>
    /// Get the url to download the product file
    /// </summary>
    /// <param name="productFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatProductDownloadUrl(ProductFile productFile, string licenseString)
    {
      return string.Format(
          productDownloadScaffolding,
          productFile.Release,
          productFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

    /// <summary>
    /// Get the url to get the hash of a single data file
    /// </summary>
    /// <param name="singleFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatDataUrl(SingleFile singleFile, string licenseString)
    {
      return string.Format(
          fileHashDataScaffolding,
          singleFile.Release,
          singleFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

    /// <summary>
    /// Get the url to get the hash of a data file in the manifest
    /// </summary>
    /// <param name="manifestFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatDataUrl(ManifestFile manifestFile, string licenseString)
    {
      return string.Format(
          manifestHashDataScaffolding,
          manifestFile.Release,
          manifestFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

	/// <summary>
	/// Get the url to get the hash of the product file
	/// </summary>
	/// <param name="productFile"></param>
	/// <param name="licenseString"></param>
	/// <returns></returns>
	public static string FormatProductUrl(ProductFile productFile, string licenseString)
	{
		return string.Format(
				productHashScaffolding,
				productFile.Release,
				productFile.FileName,
				HttpUtility.UrlEncode(licenseString)
		);
	}

    /// <summary>
    /// Get the url to get the metadata of the product file
    /// </summary>
    /// <param name="productFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatMetaDataUrl(ProductFile productFile, string licenseString)
    {
        return string.Format(
            productMetaScaffolding,
            productFile.Release,
            productFile.FileName,
            HttpUtility.UrlEncode(licenseString)
        );
    }

    /// <summary>
    /// Get the url to get the metadata of a single data file
    /// </summary>
    /// <param name="singleFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatMetaDataUrl(SingleFile singleFile, string licenseString)
    {
      return string.Format(
          fileMetaDataScaffolding,
          singleFile.Release,
          singleFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

    /// <summary>
    /// Get the url to get the hash of a single library file
    /// </summary>
    /// <param name="singleFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatLibraryUrl(SingleFile singleFile, string licenseString)
    {
      return string.Format(
          fileHashLibraryScaffolding,
          singleFile.OS,
          singleFile.Compiler,
          singleFile.Architecture,
          singleFile.Release,
          singleFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

    /// <summary>
    /// Get the url to get the hash of a library file in the manifest
    /// </summary>
    /// <param name="manifestFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatLibraryUrl(ManifestFile manifestFile, string licenseString)
    {
      return string.Format(
          manifestHashLibraryScaffolding,
          manifestFile.OS,
          manifestFile.Compiler,
          manifestFile.Architecture,
          manifestFile.Release,
          manifestFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

	/// <summary>
	/// Get the url to get the metadata of a single library file
	/// </summary>
	/// <param name="singleFile"></param>
	/// <param name="licenseString"></param>
	/// <returns></returns>
	public static string FormatMetaLibraryUrl(SingleFile singleFile, string licenseString)
    {
      return string.Format(
          fileMetaLibraryScaffolding,
          singleFile.OS,
          singleFile.Compiler,
          singleFile.Architecture,
          singleFile.Release,
          singleFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

    /// <summary>
    /// Get the url to get the hash of a single interface file
    /// </summary>
    /// <param name="singleFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatInterfaceUrl(SingleFile singleFile, string licenseString)
    {
      return string.Format(
          fileHashInterfaceScaffolding,
          singleFile.OS,
          singleFile.Compiler,
          singleFile.Architecture,
          singleFile.Release,
          singleFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

    /// <summary>
    /// Get the url to get the hash of an interface file in the manifest
    /// </summary>
    /// <param name="manifestFile"></param>
    /// <param name="licenseString"></param>
    /// <returns></returns>
    public static string FormatInterfaceUrl(ManifestFile manifestFile, string licenseString)
    {
      return string.Format(
          manifestHashInterfaceScaffolding,
          manifestFile.OS,
          manifestFile.Compiler,
          manifestFile.Architecture,
          manifestFile.Release,
          manifestFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

	/// <summary>
	/// Get the url to get the metadata of a single interface file
	/// </summary>
	/// <param name="singleFile"></param>
	/// <param name="licenseString"></param>
	/// <returns></returns>
	public static string FormatMetaInterfaceUrl(SingleFile singleFile, string licenseString)
    {
      return string.Format(
          fileMetaInterfaceScaffolding,
          singleFile.OS,
          singleFile.Compiler,
          singleFile.Architecture,
          singleFile.Release,
          singleFile.FileName,
          HttpUtility.UrlEncode(licenseString)
      );
    }

  }
}
