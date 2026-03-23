using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Replaces "-" in positionals arguments with stdin content.
   /// POSIX convention for piped input.
   /// </summary>
   // ============================================================
   public static class StdinReader
   {

   #region Public

      // ------------------------------------------------------------
      /// <summary>
      /// Returns true if the argument list contains a "-" token.
      /// </summary>
      // ------------------------------------------------------------
      public static bool HasStdinArg(IEnumerable<string> args)
      {
         return args.Contains("-");
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Replaces the first "-" in the list with stdin content.
      /// <br/> Returns null on success, or an error message on failure.
      /// </summary>
      // ----------------------------------------------------------------------
      public static string ReadAll(List<string> positionals)
      {
         int dashCount = positionals.Count(a => a == "-");

         if (dashCount > 1)
         {
            return "Only one '-' (stdin) argument allowed.";
         }

         int dashIndex = positionals.IndexOf("-");

         if (dashIndex < 0)
         {
            return null;
         }

         if (!Console.IsInputRedirected)
         {
            return "'-' requires piped input.";
         }

         string stdin = Console.In.ReadToEnd().Trim();

         if (string.IsNullOrEmpty(stdin))
         {
            return "No input from stdin.";
         }

         positionals[dashIndex] = stdin;
         
         return null;
      }

   #endregion

   }
}
