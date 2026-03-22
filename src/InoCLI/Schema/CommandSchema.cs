using System;
using System.Collections;
using System.Collections.Generic;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Schema for a single command within a group.
   /// </summary>
   // ============================================================
   [Serializable]
   public class CommandSchema
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Human-readable description for help output.
      /// </summary>
      // ------------------------------------------------------------
      public string Description { get; set; }

      // ------------------------------------------------------------
      /// <summary>
      /// Ordered positional arguments this command accepts.
      /// </summary>
      // ------------------------------------------------------------
      public List<ArgSchema> Args { get; set; } = new List<ArgSchema>();

      // ------------------------------------------------------------
      /// <summary>
      /// Named options this command accepts (keyed by option name without --).
      /// </summary>
      // ------------------------------------------------------------
      public Dictionary<string, OptionSchema> Options { get; set; } = new Dictionary<string, OptionSchema>();

   #endregion

   }
}
