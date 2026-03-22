using System;
using System.Text.Json;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Parsed response from a CLI server: {success, result/error}.
   /// </summary>
   // ============================================================
   public class CliResponse
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Whether the server reported success.
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

   #endregion

   #region Factory

      // ------------------------------------------------------------
      /// <summary>
      /// Parses a JSON response string.
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

      // ------------------------------------------------------------
      /// <summary>
      /// Creates a synthetic success response (e.g. for lost frames).
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
