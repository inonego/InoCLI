using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Generates help text from a CliSchema.
   /// </summary>
   // ============================================================
   public static class HelpFormatter
   {

   #region Public

      // ------------------------------------------------------------
      /// <summary>
      /// Generates top-level help listing all groups.
      /// </summary>
      // ------------------------------------------------------------
      public static string ForAll(CliSchema schema, string programName = null)
      {
         var sb = new StringBuilder();

         if (programName != null)
         {
            sb.AppendLine($"Usage: {programName} <group> [command] [args...] [--options]");
            sb.AppendLine();
         }

         sb.AppendLine("Groups:");

         foreach (var kv in schema.Groups)
         {
            string desc = kv.Value.Description != null ? $"  {kv.Value.Description}" : "";
            sb.AppendLine($"  {kv.Key,-20}{desc}");
         }

         if (schema.GlobalOptions.Count > 0)
         {
            sb.AppendLine();
            sb.AppendLine("Global options:");

            foreach (var kv in schema.GlobalOptions)
            {
               string typeHint = kv.Value.Type != "bool" ? $" <{kv.Value.Type}>" : "";
               string desc     = kv.Value.Description != null ? $"  {kv.Value.Description}" : "";

               sb.AppendLine($"  --{kv.Key}{typeHint,-12}{desc}");
            }
         }

         return sb.ToString().TrimEnd();
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Generates help for a specific group and its commands.
      /// </summary>
      // ------------------------------------------------------------
      public static string ForGroup(CliSchema schema, string group)
      {
         if (!schema.Groups.TryGetValue(group, out var groupSchema))
         {
            return $"Unknown group: {group}";
         }

         var sb = new StringBuilder();

         if (groupSchema.Description != null)
         {
            sb.AppendLine(groupSchema.Description);
            sb.AppendLine();
         }

         sb.AppendLine("Commands:");

         foreach (var kv in groupSchema.Commands)
         {
            string name = string.IsNullOrEmpty(kv.Key) ? "(default)" : kv.Key;
            string desc = kv.Value.Description != null ? $"  {kv.Value.Description}" : "";

            sb.AppendLine($"  {name,-20}{desc}");
         }

         return sb.ToString().TrimEnd();
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Generates detailed help for a specific command,
      /// <br/> including its arguments and options.
      /// </summary>
      // ----------------------------------------------------------------------
      public static string ForCommand(CliSchema schema, string group, string command)
      {
         if (!schema.Groups.TryGetValue(group, out var groupSchema))
         {
            return $"Unknown group: {group}";
         }

         if (!groupSchema.Commands.TryGetValue(command, out var cmdSchema))
         {
            return $"Unknown command: {group} {command}";
         }

         var sb = new StringBuilder();

         if (cmdSchema.Description != null)
         {
            sb.AppendLine(cmdSchema.Description);
            sb.AppendLine();
         }

         // Arguments
         if (cmdSchema.Args.Count > 0)
         {
            sb.AppendLine("Arguments:");

            foreach (var arg in cmdSchema.Args)
            {
               string req  = arg.Required ? " (required)" : "";
               string desc = arg.Description != null ? $"  {arg.Description}" : "";

               sb.AppendLine($"  <{arg.Name}>{req,-12}{desc}");
            }

            sb.AppendLine();
         }

         // Options
         if (cmdSchema.Options.Count > 0)
         {
            sb.AppendLine("Options:");

            foreach (var kv in cmdSchema.Options)
            {
               string typeHint = kv.Value.Type != "bool" ? $" <{kv.Value.Type}>" : "";
               string repeated = kv.Value.Repeated ? " (repeatable)" : "";
               string desc     = kv.Value.Description != null ? $"  {kv.Value.Description}" : "";

               sb.AppendLine($"  --{kv.Key}{typeHint}{repeated,-12}{desc}");
            }
         }

         return sb.ToString().TrimEnd();
      }

   #endregion

   }
}
