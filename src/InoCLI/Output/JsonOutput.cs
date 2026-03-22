using System;
using System.Text.Json;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Writes JSON to stdout with optional pretty-printing.
   /// </summary>
   // ============================================================
   public static class JsonOutput
   {

   #region Write

      // ------------------------------------------------------------
      /// <summary>
      /// Writes JSON to stdout, optionally pretty-printed.
      /// </summary>
      // ------------------------------------------------------------
      public static void Write(string json, bool pretty = false)
      {
         if (pretty)
         {
            json = Prettify(json);
         }

         Console.WriteLine(json);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Re-formats a JSON string with indentation.
      /// </summary>
      // ------------------------------------------------------------
      public static string Prettify(string json)
      {
         var doc = JsonDocument.Parse(json);

         return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
      }

   #endregion

   }
}
