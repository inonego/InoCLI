using System;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Schema for a single named option (e.g. --count, --pretty).
   /// </summary>
   // ============================================================
   [Serializable]
   public class OptionSchema
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Value type: "string", "int", or "bool".
      /// </summary>
      // ------------------------------------------------------------
      public string Type { get; set; }

      // ------------------------------------------------------------
      /// <summary>
      /// Whether this option can be repeated (e.g. --using A --using B).
      /// </summary>
      // ------------------------------------------------------------
      public bool Repeated { get; set; }

      // ------------------------------------------------------------
      /// <summary>
      /// Human-readable description for help output.
      /// </summary>
      // ------------------------------------------------------------
      public string Description { get; set; }

   #endregion

   }
}
