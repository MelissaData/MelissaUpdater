using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using MelissaUpdater.Models;


namespace MelissaUpdater.Exceptions
{
  class InvalidResponseException : Exception
  {
    public InvalidResponseException(HttpResponseMessage response) : base(GetExceptionMessage(response))
    {
    }

    /// <summary>
    /// Gets the correct error message for each case.
    /// </summary>
    /// <param name="response">The response from Melissa Releases API</param>
    /// <returns></returns>
    private static string GetExceptionMessage(HttpResponseMessage response)
    {
      string message = String.Empty;

      HttpStatusCode statusCode = response.StatusCode;
      string description = response.ReasonPhrase;

      switch (statusCode)
      {
        case HttpStatusCode.NotFound:
          message = "File or directory is no longer available or not found.\nCheck your arguments again or contact tech support at https://www.melissa.com/company/support";
          break;
        case HttpStatusCode.Unauthorized:
          try
          {
            var responseString = response.Content.ReadAsStringAsync();
            Response responseInfo = JsonConvert.DeserializeObject<Response>(responseString.Result);
            if (responseInfo.type == "invalidLicense")
            {
              message = "License string is invalid.\nCheck again or contact your sale representative for support.";
            }
            else if (responseInfo.type == "expiredLicense")
            {
              message = "License string is expired.\nCheck again or contact your sale representative for support.";
            }
            else if (responseInfo.type == "invalidProduct")
            {
              message = "License string is invalid for this product.\nCheck again or contact your sale representative for support.";
            }
            else
            {
              message = "License string is invalid.\nCheck again or contact your sale representative for support.";
            }
          }
          catch
          {
            message = "License string is invalid.\nCheck again or contact your sale representative for support.";
          }
          break;
        default:
          message = $"There was an invalid response from the Melissa Releases API:\nStatusCode: {statusCode}, ReasonPhrase: {description}\nCheck your arguments again or contact tech support at https://www.melissa.com/company/support";
          break;
      }
      return message;
    }
  }
}
