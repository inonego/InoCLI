using System;
using System.Reflection;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Metadata for a discovered CLI command.
   /// </summary>
   // ============================================================
   public class CommandInfo
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Command path segments (e.g. ["scene", "load"]).
      /// </summary>
      // ------------------------------------------------------------
      public string[] Path { get; }

      // ------------------------------------------------------------
      /// <summary>
      /// Command description from the attribute.
      /// </summary>
      // ------------------------------------------------------------
      public string Description { get; }

      // ------------------------------------------------------------
      /// <summary>
      /// The method that implements this command.
      /// </summary>
      // ------------------------------------------------------------
      public MethodInfo Method { get; }

      // ------------------------------------------------------------
      /// <summary>
      /// Dot-joined path key (e.g. "scene.load").
      /// </summary>
      // ------------------------------------------------------------
      public string Key { get; }

   #endregion

   #region Constructors

      public CommandInfo(string[] path, string description, MethodInfo method)
      {
         Path        = path ?? Array.Empty<string>();
         Description = description ?? "";
         Method      = method;
         Key         = string.Join(".", path);
      }

   #endregion

   }
}
