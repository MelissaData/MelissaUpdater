using System;
using System.Collections.Generic;
using System.Text;

namespace MelissaUpdater.Exceptions
{
  class InvalidArgumentException : Exception
  {
    public string Type { get; set; }
    public string Value { get; set; }
    public InvalidArgumentException(string type, string release = "") : base(GetExceptionMessage(type, release))
    {
      Type = type;
      Value = release;
    }

    /// <summary>
    /// Gets the correct error message for each case.
    /// </summary>
    /// <param name="type">Type of error (e.g. "invalidCompiler")</param>
    /// <param name="value">Additional value to describe the error, if applicable, such as release date or map file path.</param>
    /// <returns></returns>
    private static string GetExceptionMessage(string type, string value)
    {
      string message = "";

      switch (type)
      {
        case "invalidArchitecture":
          message = "Architecture type is missing or invalid.\nCheck again or contact tech support at https://www.melissa.com/company/support.";
          break;
        case "invalidFileName":
          message = "File attributes are missing or invalid.\nCheck the File Name, File Type, OS, and Architecture again or contact tech support at https://www.melissa.com/company/support.";
          break;
        case "invalidLanguage":
          message = "Compiler type is missing or invalid.\nCheck again or contact tech support at https://www.melissa.com/company/support.";
          break;
        case "invalidLicense":
          message = "License String is invalid.\nCheck again or contact your sale representative for support.";
          break;
        case "expiredLicense":
          message = "License String is expired.\nCheck again or contact your sale representative for support.";
          break;
        case "invalidManifest":
          message = "Manifest Name is invalid.\nCheck again or visit https://releasenotes.melissa.com/ for more info.";
          break;
				case "invalidMap":
				  message = $"Map file \"{value}\" is invalid.\nCheck again or contact tech support at https://www.melissa.com/company/support.";
				  break;
				case "invalidMapMissing":
					message = $"Map file \"{value}\" doesn't exist.\nCheck again or contact tech support at https://www.melissa.com/company/support.";
					break;
				case "invalidOS":
          message = "OS type is missing or invalid.\nCheck again or contact tech support at https://www.melissa.com/company/support.";
          break;
        case "invalidProduct":
          message = "License String is invalid or Customer not authorized for product.\nCheck again or contact your sale representative for support.";
          break;
        case "invalidRelease":
          if(String.IsNullOrEmpty(value))
          {
              message = "Release Version is missing.\nCheck again or visit https://releasenotes.melissa.com/ for more info.";
          }
          else
          {
              message = $"Release Version \"{value}\" is invalid.\nCheck again or visit https://releasenotes.melissa.com/ for more info.";
          }
          break;
        default:
          message = "There is a missing or invalid argument.\nCheck again or contact tech support at https://www.melissa.com/company/support.";
          break;
      }
      return message;
    }
  }
}
