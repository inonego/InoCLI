using System;
using System.Collections;
using System.Collections.Generic;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Schema for a command group (e.g. "flow", "break").
   /// </summary>
   // ============================================================
   [Serializable]
   public class GroupSchema
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
      /// Commands in this group, keyed by command name.
      /// </summary>
      // ------------------------------------------------------------
      public Dictionary<string, CommandSchema> Commands { get; set; } = new Dictionary<string, CommandSchema>();

   #endregion

   }
}
