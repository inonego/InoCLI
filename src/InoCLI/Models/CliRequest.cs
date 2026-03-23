using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// JSON request: {positionals[], optionals{}}.
   /// </summary>
   // ============================================================
   public class CliRequest
   {

   #region Fields

      public List<string>                       Positionals { get; set; } = new List<string>();
      public Dictionary<string, List<string>>   Optionals     { get; set; } = new Dictionary<string, List<string>>();

   #endregion

   #region Factory

      // ------------------------------------------------------------
      /// <summary>
      /// Creates a CliRequest from parsed arguments.
      /// </summary>
      // ------------------------------------------------------------
      public static CliRequest FromParsedArgs(ParsedArgs parsed)
      {
         return new CliRequest
         {
            Positionals = parsed.Positionals,
            Optionals     = parsed.Optionals
         };
      }

   #endregion

   #region Serialization

      // ------------------------------------------------------------
      /// <summary>
      /// Serializes this request to a JSON string.
      /// </summary>
      // ------------------------------------------------------------
      public string ToJson()
      {
         var dict = new Dictionary<string, object>();

         if (Positionals.Count > 0)
         {
            dict["positionals"] = Positionals;
         }

         if (Optionals.Count > 0)
         {
            dict["optionals"] = Optionals;
         }

         return JsonSerializer.Serialize(dict);
      }

   #endregion

   }
}
