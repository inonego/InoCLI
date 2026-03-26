using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Scans, stores, and resolves CLI commands.
   /// </summary>
   // ============================================================
   public class CommandRegistry
   {

   #region Fields

      private readonly Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();

      private int maxDepth = 0;

      private List<string> cachedRoots = null;
      private string       cachedHelp  = null;

   #endregion

   #region Initialize

      // ------------------------------------------------------------
      /// <summary>
      /// Scans assemblies and registers all [CLICommand] methods.
      /// </summary>
      // ------------------------------------------------------------
      public void Initialize(params Assembly[] assemblies)
      {
         Initialize((IEnumerable<Assembly>)assemblies);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Scans assemblies and registers all [CLICommand] methods.
      /// </summary>
      // ------------------------------------------------------------
      public void Initialize(IEnumerable<Assembly> assemblies)
      {
         commands.Clear();

         maxDepth    = 0;
         cachedRoots = null;
         cachedHelp  = null;

         var discovered = CommandScanner.ScanAll(assemblies);

         foreach (var info in discovered)
         {
            commands[info.Key] = info;

            if (info.Path.Length > maxDepth)
            {
               maxDepth = info.Path.Length;
            }
         }
      }

   #endregion

   #region Resolve

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Matches positionals against registered command paths.
      /// <br/> Returns the matched command and a new CommandArgs
      /// <br/> with the path segments removed from positionals.
      /// </summary>
      // ----------------------------------------------------------------------
      public (CommandInfo info, CommandArgs args) Resolve(CommandArgs parsed)
      {
         // Greedy match: try longest path first
         int depth = Math.Min(parsed.Positionals.Count, maxDepth);

         for (; depth >= 1; depth--)
         {
            string key = string.Join(".", parsed.Positionals.GetRange(0, depth));

            if (commands.TryGetValue(key, out var info))
            {
               var args = new CommandArgs
               {
                  Positionals = parsed.Positionals.GetRange(depth, parsed.Positionals.Count - depth),
                  Optionals   = parsed.Optionals
               };

               return (info, args);
            }
         }

         string attempted = parsed.Positionals.Count > 0 ? parsed.Positionals[0] : "(empty)";

         throw new ArgumentException($"Unknown command: {attempted}");
      }

   #endregion

   #region Query

      // ------------------------------------------------------------
      /// <summary>
      /// Finds a command by exact path, or null if not found.
      /// </summary>
      // ------------------------------------------------------------
      public CommandInfo Find(params string[] path)
      {
         string key = string.Join(".", path);

         commands.TryGetValue(key, out var info);

         return info;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Returns all registered commands.
      /// </summary>
      // ------------------------------------------------------------
      public List<CommandInfo> GetAll()
      {
         return commands.Values.ToList();
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Returns distinct root path segments (Path[0]). Cached.
      /// </summary>
      // ------------------------------------------------------------
      public List<string> GetRoots()
      {
         if (cachedRoots != null)
         {
            return cachedRoots;
         }

         var roots = new HashSet<string>();

         foreach (var info in commands.Values)
         {
            if (info.Path.Length > 0)
            {
               roots.Add(info.Path[0]);
            }
         }

         cachedRoots = roots.ToList();

         return cachedRoots;
      }

   #endregion

   #region Help

      // ------------------------------------------------------------
      /// <summary>
      /// Generates help text for all registered commands. Cached.
      /// </summary>
      // ------------------------------------------------------------
      public string GetHelp()
      {
         if (cachedHelp != null)
         {
            return cachedHelp;
         }

         var sb = new StringBuilder();

         var sorted = commands.Values.OrderBy(c => c.Key).ToList();

         foreach (var info in sorted)
         {
            sb.Append("  ");
            sb.Append(info.Key.PadRight(30));

            if (!string.IsNullOrEmpty(info.Description))
            {
               sb.Append(info.Description);
            }

            sb.AppendLine();
         }

         cachedHelp = sb.ToString();

         return cachedHelp;
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Generates help text for commands under a specific path prefix.
      /// <br/> e.g. GetHelp("scene") lists all scene.* commands.
      /// </summary>
      // ----------------------------------------------------------------------
      public string GetHelp(params string[] path)
      {
         string prefix = string.Join(".", path);

         var sb = new StringBuilder();

         var matched = commands.Values
            .Where(c => c.Key.StartsWith(prefix, StringComparison.Ordinal))
            .OrderBy(c => c.Key)
            .ToList();

         foreach (var info in matched)
         {
            // Show path relative to the prefix
            string relative = info.Key.Length > prefix.Length
               ? info.Key.Substring(prefix.Length + 1)
               : info.Key;

            sb.Append("  ");
            sb.Append(relative.PadRight(30));

            if (!string.IsNullOrEmpty(info.Description))
            {
               sb.Append(info.Description);
            }

            sb.AppendLine();
         }

         return sb.ToString();
      }

   #endregion

   }
}
