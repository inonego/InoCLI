using System;
using System.Collections;
using System.Collections.Generic;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Parses CLI arguments into Positionals and Optionals.
   /// Supports both -short and --long optional formats.
   /// </summary>
   // ============================================================
   public class ArgParser
   {

   #region Parse

      // ------------------------------------------------------------
      /// <summary>
      /// Parses the argument array into a ParsedArgs object.
      /// </summary>
      // ------------------------------------------------------------
      public ParsedArgs Parse(string[] args)
      {
         var result = new ParsedArgs();

         for (int i = 0; i < args.Length; i++)
         {
            string arg = args[i];

            if (TryParseOptional(arg, out string key))
            {
               ParseOptionalValue(result.Optionals, key, args, ref i);
            }
            else
            {
               var positional = ResolvePositional(arg);

               result.Positionals.Add(positional);
            }
         }

         return result;
      }

   #endregion

   #region Helpers

      // ---------------------------------------------------------------------------------
      /// <summary>
      /// Extracts key from -short or --long format. Returns false if not an optional.
      /// </summary>
      // ---------------------------------------------------------------------------------
      private bool TryParseOptional(string arg, out string key)
      {
         if (arg.StartsWith("--"))
         {
            key = arg.Substring(2);
            return true;
         }

         if (arg.StartsWith("-") && arg.Length > 1 && !char.IsDigit(arg[1]))
         {
            key = arg.Substring(1);
            return true;
         }

         key = null;

         return false;
      }

      // -------------------------------------------------------------------
      /// <summary>
      /// Reads the optional value if present, otherwise marks as flag.
      /// </summary>
      // -------------------------------------------------------------------
      private void ParseOptionalValue(Dictionary<string, List<string>> optionals, string key, string[] args, ref int i)
      {
         if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
         {
            if (!optionals.ContainsKey(key))
            {
               optionals[key] = new List<string>();
            }

            optionals[key].Add(args[++i]);
         }
         else
         {
            if (!optionals.ContainsKey(key))
            {
               optionals[key] = new List<string>();
            }
         }
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Resolves a positional value. Substitutes "-" with stdin.
      /// </summary>
      // ------------------------------------------------------------
      private string ResolvePositional(string arg)
      {
         if (arg == "-" && Console.IsInputRedirected)
         {
            return Console.In.ReadToEnd().Trim();
         }

         return arg;
      }

   #endregion

   }
}
