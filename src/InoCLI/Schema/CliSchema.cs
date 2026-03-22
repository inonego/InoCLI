using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Root schema describing all groups, commands, and options
   /// for a CLI application. Loaded from schema.json.
   /// </summary>
   // ============================================================
   [Serializable]
   public class CliSchema
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Global options available to all commands (e.g. --timeout).
      /// </summary>
      // ------------------------------------------------------------
      public Dictionary<string, OptionSchema> GlobalOptions { get; set; } = new Dictionary<string, OptionSchema>();

      // ------------------------------------------------------------
      /// <summary>
      /// Command groups, keyed by group name.
      /// </summary>
      // ------------------------------------------------------------
      public Dictionary<string, GroupSchema> Groups { get; set; } = new Dictionary<string, GroupSchema>();

   #endregion

   #region Load

      // ------------------------------------------------------------
      /// <summary>
      /// Loads a CliSchema from a JSON file.
      /// </summary>
      // ------------------------------------------------------------
      public static CliSchema Load(string path)
      {
         string json = File.ReadAllText(path);
         return Parse(json);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Parses a CliSchema from a JSON string.
      /// </summary>
      // ------------------------------------------------------------
      public static CliSchema Parse(string json)
      {
         var doc  = JsonDocument.Parse(json);
         var root = doc.RootElement;

         var schema = new CliSchema();

         // Global options
         if (root.TryGetProperty("globalOptions", out var globalOpts))
         {
            foreach (var prop in globalOpts.EnumerateObject())
            {
               schema.GlobalOptions[prop.Name] = ParseOptionSchema(prop.Value);
            }
         }

         // Groups
         if (root.TryGetProperty("groups", out var groups))
         {
            foreach (var groupProp in groups.EnumerateObject())
            {
               schema.Groups[groupProp.Name] = ParseGroupSchema(groupProp.Value);
            }
         }

         return schema;
      }

   #endregion

   #region Query

      // ------------------------------------------------------------
      /// <summary>
      /// Returns true if the given group name exists in the schema.
      /// </summary>
      // ------------------------------------------------------------
      public bool HasGroup(string group)
      {
         return Groups.ContainsKey(group);
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Returns true if the given command exists within a group.
      /// <br/> Returns false if the group itself does not exist.
      /// </summary>
      // ----------------------------------------------------------------------
      public bool HasCommand(string group, string command)
      {
         if (!Groups.TryGetValue(group, out var groupSchema))
         {
            return false;
         }

         return groupSchema.Commands.ContainsKey(command);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Returns true if the given name is a known global option.
      /// </summary>
      // ------------------------------------------------------------
      public bool IsGlobalOption(string name)
      {
         return GlobalOptions.ContainsKey(name);
      }

   #endregion

   #region Parsing Helpers

      private static GroupSchema ParseGroupSchema(JsonElement element)
      {
         var group = new GroupSchema();

         if (element.TryGetProperty("description", out var desc))
         {
            group.Description = desc.GetString();
         }

         if (element.TryGetProperty("commands", out var commands))
         {
            foreach (var cmdProp in commands.EnumerateObject())
            {
               group.Commands[cmdProp.Name] = ParseCommandSchema(cmdProp.Value);
            }
         }

         return group;
      }

      private static CommandSchema ParseCommandSchema(JsonElement element)
      {
         var cmd = new CommandSchema();

         if (element.TryGetProperty("description", out var desc))
         {
            cmd.Description = desc.GetString();
         }

         if (element.TryGetProperty("args", out var args))
         {
            foreach (var argEl in args.EnumerateArray())
            {
               cmd.Args.Add(ParseArgSchema(argEl));
            }
         }

         if (element.TryGetProperty("options", out var options))
         {
            foreach (var optProp in options.EnumerateObject())
            {
               cmd.Options[optProp.Name] = ParseOptionSchema(optProp.Value);
            }
         }

         return cmd;
      }

      private static ArgSchema ParseArgSchema(JsonElement element)
      {
         var arg = new ArgSchema();

         if (element.TryGetProperty("name", out var name))
         {
            arg.Name = name.GetString();
         }

         if (element.TryGetProperty("type", out var type))
         {
            arg.Type = type.GetString();
         }

         if (element.TryGetProperty("required", out var required))
         {
            arg.Required = required.GetBoolean();
         }

         if (element.TryGetProperty("description", out var desc))
         {
            arg.Description = desc.GetString();
         }

         return arg;
      }

      private static OptionSchema ParseOptionSchema(JsonElement element)
      {
         var opt = new OptionSchema();

         if (element.TryGetProperty("type", out var type))
         {
            opt.Type = type.GetString();
         }

         if (element.TryGetProperty("repeated", out var repeated))
         {
            opt.Repeated = repeated.GetBoolean();
         }

         if (element.TryGetProperty("description", out var desc))
         {
            opt.Description = desc.GetString();
         }

         return opt;
      }

   #endregion

   }
}
