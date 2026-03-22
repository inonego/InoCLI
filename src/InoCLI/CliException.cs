using System;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Exception thrown for CLI-level errors (validation, parsing).
   /// </summary>
   // ============================================================
   public class CliException : Exception
   {

   #region Constructors

      public CliException(string message) : base(message) {}

      public CliException(string message, Exception inner) : base(message, inner) {}

   #endregion

   }
}
