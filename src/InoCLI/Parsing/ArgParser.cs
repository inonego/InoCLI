using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Parses CLI arguments into a structured ParsedArgs object.
   /// Supports schema-based validation or free-form parsing.
   /// </summary>
   // ============================================================
   public class ArgParser
   {

   #region Fields

      private readonly CliSchema schema;
      private readonly HashSet<string> globalOptionNames;

   #endregion

   #region Constructors

      // ------------------------------------------------------------
      /// <summary>
      /// Creates a schema-based parser with validation.
      /// </summary>
      // ------------------------------------------------------------
      public ArgParser(CliSchema schema)
      {
         this.schema = schema;

         globalOptionNames = new HashSet<string>(schema.GlobalOptions.Keys);
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Creates a free-form parser without schema validation.
      /// <br/> Used when the server handles validation (e.g. UniCLI).
      /// </summary>
      // ----------------------------------------------------------------------
      public ArgParser(IEnumerable<string> globalOptionNames = null)
      {
         this.schema = null;

         this.globalOptionNames = globalOptionNames != null
            ? new HashSet<string>(globalOptionNames)
            : new HashSet<string>();
      }

   #endregion

   #region Parse

      // ------------------------------------------------------------
      /// <summary>
      /// Parses the argument array into a ParsedArgs object.
      /// </summary>
      // ------------------------------------------------------------
      public ParsedArgs Parse(string[] args)
      {
         var result    = new ParsedArgs();
         var remaining = new List<string>();

         // Pass 1: Extract global options
         for (int i = 0; i < args.Length; i++)
         {
            if (args[i] == "--help")
            {
               result.HelpRequested = true;
               continue;
            }

            if (args[i].StartsWith("--"))
            {
               string key = args[i].Substring(2);

               if (globalOptionNames.Contains(key))
               {
                  if (IsGlobalFlag(key))
                  {
                     result.GlobalOptions[key] = true;
                  }
                  else if (i + 1 < args.Length)
                  {
                     result.GlobalOptions[key] = args[++i];
                  }

                  continue;
               }
            }

            remaining.Add(args[i]);
         }

         if (remaining.Count == 0)
         {
            return result;
         }

         // Pass 2: Extract group
         result.Group = remaining[0];

         int argStart = 1;

         // Pass 3: Detect subcommand
         if (remaining.Count > 1 && !remaining[1].StartsWith("--"))
         {
            if (IsSubcommand(result.Group, remaining[1]))
            {
               result.Command = remaining[1];
               argStart       = 2;
            }
         }

         // Pass 4: Parse positional args and options
         for (int i = argStart; i < remaining.Count; i++)
         {
            string arg = remaining[i];

            if (arg.StartsWith("--"))
            {
               string key = arg.Substring(2);

               if (key == "help")
               {
                  result.HelpRequested = true;
                  continue;
               }

               if (i + 1 < remaining.Count && !remaining[i + 1].StartsWith("--"))
               {
                  string value = remaining[++i];

                  // Handle repeated options (e.g. --using A --using B)
                  if (result.Options.ContainsKey(key))
                  {
                     var existing = result.Options[key];

                     if (existing is List<object> list)
                     {
                        list.Add(value);
                     }
                     else
                     {
                        result.Options[key] = new List<object> { existing, value };
                     }
                  }
                  else
                  {
                     result.Options[key] = value;
                  }
               }
               else
               {
                  result.Options[key] = true;
               }
            }
            else
            {
               result.Positional.Add(arg);
            }
         }

         // Validate against schema if present
         if (schema != null && !result.HelpRequested)
         {
            Validate(result);
         }

         return result;
      }

   #endregion

   #region Validation

      private void Validate(ParsedArgs result)
      {
         if (result.Group == null)
         {
            return;
         }

         if (!schema.HasGroup(result.Group))
         {
            throw new CliException($"Unknown group: {result.Group}");
         }

         if (!string.IsNullOrEmpty(result.Command) && !schema.HasCommand(result.Group, result.Command))
         {
            throw new CliException($"Unknown command: {result.Group} {result.Command}");
         }
      }

   #endregion

   #region Helpers

      private bool IsGlobalFlag(string key)
      {
         if (schema != null && schema.GlobalOptions.TryGetValue(key, out var opt))
         {
            return opt.Type == "bool";
         }

         return false;
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Determines whether a token is a subcommand or a positional arg.
      /// <br/> With schema: checks if the group has a command with that name.
      /// <br/> Without schema: treats any non-option second token as command.
      /// </summary>
      // ----------------------------------------------------------------------
      private bool IsSubcommand(string group, string token)
      {
         if (schema != null)
         {
            return schema.HasCommand(group, token);
         }

         // Free-form mode: assume second token is always a command
         return true;
      }

   #endregion

   }
}
