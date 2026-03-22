using System;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Schema for a single positional argument.
   /// </summary>
   // ============================================================
   [Serializable]
   public class ArgSchema
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Display name (e.g. "file", "pid", "expression").
      /// </summary>
      // ------------------------------------------------------------
      public string Name { get; set; }

      // ------------------------------------------------------------
      /// <summary>
      /// Value type: "string" or "int".
      /// </summary>
      // ------------------------------------------------------------
      public string Type { get; set; }

      // ------------------------------------------------------------
      /// <summary>
      /// Whether this argument is required.
      /// </summary>
      // ------------------------------------------------------------
      public bool Required { get; set; }

      // ------------------------------------------------------------
      /// <summary>
      /// Human-readable description for help output.
      /// </summary>
      // ------------------------------------------------------------
      public string Description { get; set; }

   #endregion

   }
}
