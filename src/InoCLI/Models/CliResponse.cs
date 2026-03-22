using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// CLI response: parse server JSON or build client-side responses.
   /// </summary>
   // ============================================================
   public class CliResponse
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Whether the response indicates success.
      /// </summary>
      // ------------------------------------------------------------
      public bool Success { get; set; }

      // ------------------------------------------------------------
      /// <summary>
      /// The raw JSON string of the response.
      /// </summary>
      // ------------------------------------------------------------
      public string RawJson { get; set; }

      // ------------------------------------------------------------
      /// <summary>
      /// Process exit code derived from the success field.
      /// </summary>
      // ------------------------------------------------------------
      public int ExitCode => Success ? 0 : 1;

      private static readonly JsonSerializerOptions PrettyOptions = new JsonSerializerOptions
      {
         WriteIndented = true
      };

   #endregion

   #region Parse

      // ------------------------------------------------------------
      /// <summary>
      /// Parses a JSON response string from a server.
      /// </summary>
      // ------------------------------------------------------------
      public static CliResponse Parse(string json)
      {
         var response = new CliResponse { RawJson = json };

         try
         {
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("success", out var success))
            {
               response.Success = success.GetBoolean();
            }
         }
         catch
         {
            response.Success = false;
         }

         return response;
      }

   #endregion

   #region Builders

      // ------------------------------------------------------------
      /// <summary>
      /// Creates a success response with a message.
      /// </summary>
      // ------------------------------------------------------------
      public static CliResponse Ok(string message)
      {
         var dict = new Dictionary<string, object>
         {
            ["success"] = true,
            ["message"] = message
         };

         return new CliResponse
         {
            Success = true,
            RawJson = JsonSerializer.Serialize(dict)
         };
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Creates a success response with a key-value result.
      /// </summary>
      // ------------------------------------------------------------
      public static CliResponse Result(string key, object value)
      {
         var dict = new Dictionary<string, object>
         {
            ["success"] = true,
            [key]       = value
         };

         return new CliResponse
         {
            Success = true,
            RawJson = JsonSerializer.Serialize(dict, PrettyOptions)
         };
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Creates a success response from a dictionary.
      /// <br/> Ensures "success" is the first key in the output.
      /// </summary>
      // ----------------------------------------------------------------------
      public static CliResponse From(Dictionary<string, object> data)
      {
         var ordered = new Dictionary<string, object> { ["success"] = true };

         foreach (var kv in data)
         {
            if (kv.Key != "success")
            {
               ordered[kv.Key] = kv.Value;
            }
         }

         return new CliResponse
         {
            Success = true,
            RawJson = JsonSerializer.Serialize(ordered)
         };
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Creates an error response with code and message.
      /// </summary>
      // ------------------------------------------------------------
      public static CliResponse Error(string code, string message)
      {
         return Error(code, message, null);
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Creates an error response with code, message, and extra data.
      /// <br/> Extra fields are merged into the error object.
      /// </summary>
      // ----------------------------------------------------------------------
      public static CliResponse Error(string code, string message, Dictionary<string, object> data)
      {
         var error = new Dictionary<string, object>
         {
            ["code"]    = code,
            ["message"] = message
         };

         if (data != null)
         {
            foreach (var kv in data)
            {
               error[kv.Key] = kv.Value;
            }
         }

         var dict = new Dictionary<string, object>
         {
            ["success"] = false,
            ["error"]   = error
         };

         return new CliResponse
         {
            Success = false,
            RawJson = JsonSerializer.Serialize(dict)
         };
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Creates a synthetic success response (for lost frames).
      /// </summary>
      // ------------------------------------------------------------
      public static CliResponse SyntheticSuccess()
      {
         return new CliResponse
         {
            Success = true,
            RawJson = "{\"success\":true,\"result\":null}"
         };
      }

   #endregion

   }
}
