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

            // Long option: --key or --key value
            if (arg.StartsWith("--"))
            {
               string key = arg.Substring(2);

               if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
               {
                  AddOptional(result.Optionals, key, args[++i]);
               }
               else
               {
                  EnsureOptional(result.Optionals, key);
               }
            }
            // Short option: -k or -k value
            else if (arg.StartsWith("-") && arg.Length > 1 && !char.IsDigit(arg[1]))
            {
               string key = arg.Substring(1);

               if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
               {
                  AddOptional(result.Optionals, key, args[++i]);
               }
               else
               {
                  EnsureOptional(result.Optionals, key);
               }
            }
            // Positional
            else
            {
               result.Positionals.Add(arg);
            }
         }

         return result;
      }

   #endregion

   #region Helpers

      // ------------------------------------------------------------
      /// <summary>
      /// Adds a value to an optional key, creating the list if needed.
      /// </summary>
      // ------------------------------------------------------------
      private void AddOptional(Dictionary<string, List<string>> optionals, string key, string value)
      {
         if (!optionals.ContainsKey(key))
         {
            optionals[key] = new List<string>();
         }

         optionals[key].Add(value);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Ensures an optional key exists (flag with no value).
      /// </summary>
      // ------------------------------------------------------------
      private void EnsureOptional(Dictionary<string, List<string>> optionals, string key)
      {
         if (!optionals.ContainsKey(key))
         {
            optionals[key] = new List<string>();
         }
      }

   #endregion

   }
}
