using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// JSON-RPC style request: {group, command, args[], options{}}.
   /// </summary>
   // ============================================================
   public class CliRequest
   {

   #region Fields

      public string                    Group      { get; set; }
      public string                    Command    { get; set; }
      public List<string>              Args       { get; set; } = new List<string>();
      public Dictionary<string, object> Options   { get; set; } = new Dictionary<string, object>();

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
            Group   = parsed.Group,
            Command = parsed.Command,
            Args    = parsed.Positional,
            Options = parsed.Options
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
         var dict = new Dictionary<string, object>
         {
            ["group"] = Group
         };

         if (!string.IsNullOrEmpty(Command))
         {
            dict["command"] = Command;
         }

         if (Args.Count > 0)
         {
            dict["args"] = Args;
         }

         if (Options.Count > 0)
         {
            dict["options"] = Options;
         }

         return JsonSerializer.Serialize(dict);
      }

   #endregion

   }
}
