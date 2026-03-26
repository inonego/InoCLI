using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Xunit;

using InoCLI;

namespace InoCLI.TEST
{
   // ============================================================
   /// <summary>
   /// Tests for CLICommandAttribute, CommandScanner,
   /// CommandRegistry, and CommandArgs extensions.
   /// </summary>
   // ============================================================
   public class TEST_CommandRegistry
   {

   #region Test Commands

      public static class TestHandlers
      {
         [CLICommand("status", description = "Show status")]
         public static string HandleStatus(CommandArgs args)
         {
            return "ok";
         }

         [CLICommand("run", "fast", description = "Run fast mode")]
         public static string HandleRunFast(CommandArgs args)
         {
            return "fast:" + args[0];
         }

         [CLICommand("run", "slow", description = "Run slow mode")]
         public static string HandleRunSlow(CommandArgs args)
         {
            return "slow:" + args[0];
         }

         [CLICommand("config", "set", "value", description = "Set config value")]
         public static string HandleConfigSetValue(CommandArgs args)
         {
            return "set:" + args[0];
         }
      }

   #endregion

   #region CommandScanner

      [Fact]
      public void Scan_FindsAllCommands()
      {
         var commands = CommandScanner.Scan(typeof(TestHandlers).Assembly);

         Assert.True(commands.Count >= 4);
      }

      [Fact]
      public void Scan_CorrectDescription()
      {
         var commands = CommandScanner.Scan(typeof(TestHandlers).Assembly);

         var status = commands.Find(c => c.Key == "status");

         Assert.NotNull(status);
         Assert.Equal("Show status", status.Description);
      }

      [Fact]
      public void Scan_MultiSegmentPath()
      {
         var commands = CommandScanner.Scan(typeof(TestHandlers).Assembly);

         var runFast = commands.Find(c => c.Key == "run.fast");

         Assert.NotNull(runFast);
         Assert.Equal(new[] { "run", "fast" }, runFast.Path);
      }

      [Fact]
      public void Scan_NoDescription()
      {
         // CLICommandAttribute allows omitting description
         var attr = new CLICommandAttribute("test");

         Assert.Null(attr.Description);
      }

   #endregion

   #region CommandRegistry Resolve

      [Fact]
      public void Resolve_SingleSegment()
      {
         var registry = CreateRegistry();

         var parsed = MakeArgs("status");

         var (info, args) = registry.Resolve(parsed);

         Assert.Equal("status", info.Key);
         Assert.Empty(args.Positionals);
      }

      [Fact]
      public void Resolve_TwoSegments_WithRemainingArgs()
      {
         var registry = CreateRegistry();

         var parsed = MakeArgs("run", "fast", "input.txt");

         var (info, args) = registry.Resolve(parsed);

         Assert.Equal("run.fast", info.Key);
         Assert.Single(args.Positionals);
         Assert.Equal("input.txt", args[0]);
      }

      [Fact]
      public void Resolve_ThreeSegments()
      {
         var registry = CreateRegistry();

         var parsed = MakeArgs("config", "set", "value", "42");

         var (info, args) = registry.Resolve(parsed);

         Assert.Equal("config.set.value", info.Key);
         Assert.Single(args.Positionals);
         Assert.Equal("42", args[0]);
      }

      [Fact]
      public void Resolve_GreedyMatch_PrefersLongestPath()
      {
         var registry = CreateRegistry();

         // "run slow data" should match "run.slow", not just "run"
         var parsed = MakeArgs("run", "slow", "data");

         var (info, args) = registry.Resolve(parsed);

         Assert.Equal("run.slow", info.Key);
         Assert.Equal("data", args[0]);
      }

      [Fact]
      public void Resolve_PreservesOptionals()
      {
         var registry = CreateRegistry();

         var parsed = new CommandArgs
         {
            Positionals = new List<string> { "status" },
            Optionals = new Dictionary<string, List<string>>
            {
               ["verbose"] = new List<string>(),
               ["count"]   = new List<string> { "5" }
            }
         };

         var (info, args) = registry.Resolve(parsed);

         Assert.True(args.Flag("verbose"));
         Assert.Equal("5", args["count"]);
      }

      [Fact]
      public void Resolve_UnknownCommand_Throws()
      {
         var registry = CreateRegistry();

         var parsed = MakeArgs("nonexistent");

         Assert.Throws<ArgumentException>(() => registry.Resolve(parsed));
      }

      [Fact]
      public void Resolve_EmptyPositionals_Throws()
      {
         var registry = CreateRegistry();

         var parsed = MakeArgs();

         Assert.Throws<ArgumentException>(() => registry.Resolve(parsed));
      }

   #endregion

   #region CommandRegistry Query

      [Fact]
      public void Find_ExistingCommand()
      {
         var registry = CreateRegistry();

         var info = registry.Find("run", "fast");

         Assert.NotNull(info);
         Assert.Equal("Run fast mode", info.Description);
      }

      [Fact]
      public void Find_Missing_ReturnsNull()
      {
         var registry = CreateRegistry();

         var info = registry.Find("nonexistent");

         Assert.Null(info);
      }

      [Fact]
      public void GetRoots_ReturnsDistinctRoots()
      {
         var registry = CreateRegistry();

         var roots = registry.GetRoots();

         Assert.Contains("status", roots);
         Assert.Contains("run", roots);
         Assert.Contains("config", roots);
         Assert.Equal(3, roots.Count);
      }

      [Fact]
      public void GetAll_ReturnsAllCommands()
      {
         var registry = CreateRegistry();

         var all = registry.GetAll();

         Assert.True(all.Count >= 4);
      }

   #endregion

   #region CommandRegistry Help

      [Fact]
      public void GetHelp_ListsAllCommands()
      {
         var registry = CreateRegistry();

         string help = registry.GetHelp();

         Assert.Contains("status", help);
         Assert.Contains("run.fast", help);
         Assert.Contains("run.slow", help);
         Assert.Contains("config.set.value", help);
      }

      [Fact]
      public void GetHelp_IncludesDescriptions()
      {
         var registry = CreateRegistry();

         string help = registry.GetHelp();

         Assert.Contains("Show status", help);
         Assert.Contains("Run fast mode", help);
      }

      [Fact]
      public void GetHelp_FiltersByPath()
      {
         var registry = CreateRegistry();

         string help = registry.GetHelp("run");

         Assert.Contains("fast", help);
         Assert.Contains("slow", help);
         Assert.DoesNotContain("status", help);
         Assert.DoesNotContain("config", help);
      }

      [Fact]
      public void GetHelp_Cached()
      {
         var registry = CreateRegistry();

         string first  = registry.GetHelp();
         string second = registry.GetHelp();

         Assert.Same(first, second);
      }

   #endregion

   #region CommandArgs Helpers

      [Fact]
      public void Flag_ReturnsCorrectly()
      {
         var args = new CommandArgs
         {
            Positionals = new List<string>(),
            Optionals = new Dictionary<string, List<string>>
            {
               ["verbose"] = new List<string>()
            }
         };

         Assert.True(args.Flag("verbose"));
         Assert.False(args.Flag("missing"));
      }

      [Fact]
      public void GetInt_ThrowsAndFallback()
      {
         var args = new CommandArgs
         {
            Positionals = new List<string> { "42", "abc" },
            Optionals = new Dictionary<string, List<string>>
            {
               ["count"] = new List<string> { "10" }
            }
         };

         // Positional — throws
         Assert.Equal(42, args.GetInt(0));
         Assert.Throws<ArgumentException>(() => args.GetInt(1));

         // Positional — fallback
         Assert.Equal(42, args.GetInt(0, 0));
         Assert.Equal(99, args.GetInt(1, 99));
         Assert.Equal(99, args.GetInt(5, 99));

         // Optional
         Assert.Equal(10, args.GetInt("count", 0));
         Assert.Equal(99, args.GetInt("missing", 99));
      }

      [Fact]
      public void GetLong_ThrowsAndFallback()
      {
         var args = new CommandArgs
         {
            Positionals = new List<string> { "9999999999" },
            Optionals = new Dictionary<string, List<string>>
            {
               ["offset"] = new List<string> { "1234567890123" }
            }
         };

         Assert.Equal(9999999999L, args.GetLong(0));
         Assert.Equal(1234567890123L, args.GetLong("offset"));
         Assert.Equal(0L, args.GetLong(5, 0L));
         Assert.Equal(0L, args.GetLong("missing", 0L));
      }

      [Fact]
      public void GetFloat_ThrowsAndFallback()
      {
         var args = new CommandArgs
         {
            Positionals = new List<string> { "3.14" },
            Optionals = new Dictionary<string, List<string>>
            {
               ["ratio"] = new List<string> { "0.5" }
            }
         };

         Assert.Equal(3.14f, args.GetFloat(0));
         Assert.Equal(0.5f, args.GetFloat("ratio"));
         Assert.Equal(1.0f, args.GetFloat(5, 1.0f));
         Assert.Equal(1.0f, args.GetFloat("missing", 1.0f));
      }

      [Fact]
      public void GetDouble_ThrowsAndFallback()
      {
         var args = new CommandArgs
         {
            Positionals = new List<string> { "3.141592653589793" },
            Optionals = new Dictionary<string, List<string>>
            {
               ["precision"] = new List<string> { "0.001" }
            }
         };

         Assert.Equal(3.141592653589793, args.GetDouble(0));
         Assert.Equal(0.001, args.GetDouble("precision"));
         Assert.Equal(1.0, args.GetDouble(5, 1.0));
         Assert.Equal(1.0, args.GetDouble("missing", 1.0));
      }

      [Fact]
      public void GetBool_ThrowsAndFallback()
      {
         var args = new CommandArgs
         {
            Positionals = new List<string> { "true" },
            Optionals = new Dictionary<string, List<string>>
            {
               ["enabled"] = new List<string> { "false" }
            }
         };

         Assert.True(args.GetBool(0));
         Assert.False(args.GetBool("enabled"));
         Assert.True(args.GetBool(5, true));
         Assert.True(args.GetBool("missing", true));
      }

      [Fact]
      public void From_ReturnsSlice()
      {
         var args = new CommandArgs
         {
            Positionals = new List<string> { "a", "b", "c", "d" },
            Optionals   = new Dictionary<string, List<string>>()
         };

         Assert.Equal(new[] { "c", "d" }, args.From(2));
         Assert.Equal(new[] { "a", "b", "c", "d" }, args.From(0));
         Assert.Empty(args.From(10));
      }

      [Fact]
      public void All_ThrowsAndFallback()
      {
         var args = new CommandArgs
         {
            Positionals = new List<string>(),
            Optionals = new Dictionary<string, List<string>>
            {
               ["tag"] = new List<string> { "a", "b", "c" }
            }
         };

         Assert.Equal(new[] { "a", "b", "c" }, args.All("tag"));
         Assert.Throws<ArgumentException>(() => args.All("missing"));

         var fallback = new List<string> { "default" };

         Assert.Equal(fallback, args.All("missing", fallback));
      }

   #endregion

   #region Invoke

      [Fact]
      public void Resolve_ThenInvoke()
      {
         var registry = CreateRegistry();

         var parsed = MakeArgs("run", "fast", "input.txt");

         var (info, args) = registry.Resolve(parsed);

         var result = (string)info.Method.Invoke(null, new object[] { args });

         Assert.Equal("fast:input.txt", result);
      }

      [Fact]
      public void Resolve_ThenInvoke_ThreeSegments()
      {
         var registry = CreateRegistry();

         var parsed = MakeArgs("config", "set", "value", "hello");

         var (info, args) = registry.Resolve(parsed);

         var result = (string)info.Method.Invoke(null, new object[] { args });

         Assert.Equal("set:hello", result);
      }

   #endregion

   #region Helpers

      private static CommandRegistry CreateRegistry()
      {
         var registry = new CommandRegistry();
         registry.Initialize(typeof(TestHandlers).Assembly);

         return registry;
      }

      private static CommandArgs MakeArgs(params string[] positionals)
      {
         return new CommandArgs
         {
            Positionals = new List<string>(positionals),
            Optionals   = new Dictionary<string, List<string>>()
         };
      }

   #endregion

   }
}
