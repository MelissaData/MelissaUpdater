using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MelissaUpdater.Exceptions
{
  class InvalidResponseException : Exception
  {
    public HttpStatusCode StatusCode { get; set; }
    public string Description { get; set; }
    public InvalidResponseException(HttpStatusCode statusCode, string description = "") : base(GetExceptionMessage(statusCode, description))
    {
        StatusCode = statusCode;
        Description = description;
    }

    /// <summary>
    /// Gets the correct error message for each case.
    /// </summary>
    /// <param name="statusCode">The HTTP Status Code</param>
    /// <param name="description">The description of the response</param>
    /// <returns></returns>
    private static string GetExceptionMessage(HttpStatusCode statusCode, string description)
    {
      string message = String.Empty;

      switch (statusCode)
      {
        case HttpStatusCode.NotFound:
          message += "File or directory is no longer available or not found.\nCheck your arguments again or contact tech support at https://www.melissa.com/company/support";
          break;
        case HttpStatusCode.Unauthorized:
          message += "License string is invalid.\nCheck again or contact your sale representative for support.";
          break;
        default:
          message += $"There was an invalid response from the Melissa Releases API:\nStatusCode: {statusCode}, ReasonPhrase: {description}\nCheck your arguments again or contact tech support at https://www.melissa.com/company/support";
          break;
      }
      return message;
    }
  }
}
