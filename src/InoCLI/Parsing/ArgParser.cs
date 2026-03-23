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

   #region Fields

      private string stdinCache = null;
      private bool   stdinRead  = false;

   #endregion

   #region Parse

      // ------------------------------------------------------------
      /// <summary>
      /// Parses the argument array into a ParsedArgs object.
      /// </summary>
      // ------------------------------------------------------------
      public ParsedArgs Parse(string[] args)
      {
         var result = new ParsedArgs();

         // Pre-read stdin if redirected
         if (Console.IsInputRedirected)
         {
            stdinCache = Console.In.ReadToEnd().Trim();
            stdinRead  = true;
         }

         for (int i = 0; i < args.Length; i++)
         {
            string arg = args[i];

            if (TryParseOptional(arg, out string key))
            {
               ParseOptionalValue(result.Optionals, key, args, ref i);
            }
            else
            {
               var positional = ResolveValue(arg);

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

            if (string.IsNullOrEmpty(key))
            {
               key = null;

               throw new ArgumentException("Invalid option: '--' (empty key)");
            }

            return true;
         }

         if (arg.StartsWith("-"))
         {
            key = arg.Substring(1);

            if (string.IsNullOrEmpty(key) || char.IsDigit(key[0]))
            {
               key = null;

               return false;
            }

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
         if (!optionals.ContainsKey(key))
         {
            optionals[key] = new List<string>();
         }

         if (i + 1 < args.Length && !TryParseOptional(args[i + 1], out _))
         {
            optionals[key].Add(ResolveValue(args[++i]));
         }
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Resolves a value. Substitutes "-" with cached stdin.
      /// </summary>
      // ------------------------------------------------------------
      private string ResolveValue(string arg)
      {
         if (arg == "-" && stdinRead)
         {
            return stdinCache;
         }

         return arg;
      }

   #endregion

   }
}
