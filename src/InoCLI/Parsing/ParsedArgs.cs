using System;
using System.Collections;
using System.Collections.Generic;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Result of parsing CLI arguments.
   /// </summary>
   // ============================================================
   public class ParsedArgs
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// The command group (first positional token).
      /// </summary>
      // ------------------------------------------------------------
      public string Group { get; set; }

      // ------------------------------------------------------------
      /// <summary>
      /// The subcommand within the group (may be empty).
      /// </summary>
      // ------------------------------------------------------------
      public string Command { get; set; } = "";

      // ------------------------------------------------------------
      /// <summary>
      /// Positional arguments after group and command.
      /// </summary>
      // ------------------------------------------------------------
      public List<string> Positional { get; set; } = new List<string>();

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Named options (keyed by name without --).
      /// <br/> Values are string, bool (true for flags), or List&lt;object&gt;
      /// <br/> for repeated options.
      /// </summary>
      // ----------------------------------------------------------------------
      public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();

      // ------------------------------------------------------------
      /// <summary>
      /// Global options parsed before the group token.
      /// </summary>
      // ------------------------------------------------------------
      public Dictionary<string, object> GlobalOptions { get; set; } = new Dictionary<string, object>();

      // ------------------------------------------------------------
      /// <summary>
      /// Whether --help was present anywhere in the arguments.
      /// </summary>
      // ------------------------------------------------------------
      public bool HelpRequested { get; set; } = false;

   #endregion

   }
}
