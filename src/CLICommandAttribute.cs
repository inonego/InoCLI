using System;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Marks a static method as a CLI command.
   /// Path defines the command route (e.g. "scene", "load").
   /// </summary>
   // ============================================================
   [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
   public class CLICommandAttribute : Attribute
   {

   #region Fields

      private string _description;

      // ------------------------------------------------------------
      /// <summary>
      /// Command path segments (e.g. ["scene", "load"]).
      /// </summary>
      // ------------------------------------------------------------
      public string[] Path { get; }

      // ------------------------------------------------------------
      /// <summary>
      /// Command description (read access).
      /// </summary>
      // ------------------------------------------------------------
      public string Description => _description;

      // ------------------------------------------------------------
      /// <summary>
      /// Command description (attribute parameter).
      /// </summary>
      // ------------------------------------------------------------
      public string description { get => _description; set => _description = value; }

   #endregion

   #region Constructors

      public CLICommandAttribute(params string[] path)
      {
         Path = path ?? Array.Empty<string>();
      }

   #endregion

   }
}
