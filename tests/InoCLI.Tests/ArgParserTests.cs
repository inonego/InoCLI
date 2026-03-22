using System;
using System.Collections;
using System.Collections.Generic;

using Xunit;

namespace InoCLI.Tests
{
   // ============================================================
   /// <summary>
   /// Tests for ArgParser in both schema-based and free-form modes.
   /// </summary>
   // ============================================================
   public class ArgParserTests
   {

   #region Free-form Parsing

      [Fact]
      public void Parse_GroupOnly()
      {
         var parser = new ArgParser();
         var result = parser.Parse(new[] { "ping" });

         Assert.Equal("ping",  result.Group);
         Assert.Equal("",      result.Command);
         Assert.Empty(result.Positional);
         Assert.Empty(result.Options);
      }

      [Fact]
      public void Parse_GroupAndCommand()
      {
         var parser = new ArgParser();
         var result = parser.Parse(new[] { "editor", "play" });

         Assert.Equal("editor", result.Group);
         Assert.Equal("play",   result.Command);
      }

      [Fact]
      public void Parse_PositionalArgs()
      {
         var parser = new ArgParser();
         var result = parser.Parse(new[] { "eval", "cs", "return 1+1;" });

         Assert.Equal("eval",         result.Group);
         Assert.Equal("cs",           result.Command);
         Assert.Single(result.Positional);
         Assert.Equal("return 1+1;",  result.Positional[0]);
      }

      [Fact]
      public void Parse_Options()
      {
         var parser = new ArgParser();
         var result = parser.Parse(new[] { "go", "create", "Player", "--primitive", "cube" });

         Assert.Equal("go",      result.Group);
         Assert.Equal("create",  result.Command);
         Assert.Single(result.Positional);
         Assert.Equal("Player",  result.Positional[0]);
         Assert.Equal("cube",    result.Options["primitive"]);
      }

      [Fact]
      public void Parse_BoolFlag()
      {
         var parser = new ArgParser();
         var result = parser.Parse(new[] { "scene", "list", "--full" });

         Assert.Equal(true, result.Options["full"]);
      }

      [Fact]
      public void Parse_RepeatedOptions()
      {
         var parser = new ArgParser();
         var result = parser.Parse(new[] { "eval", "cs", "code", "--using", "System", "--using", "UnityEngine" });

         Assert.IsType<List<object>>(result.Options["using"]);

         var list = (List<object>)result.Options["using"];

         Assert.Equal(2,              list.Count);
         Assert.Equal("System",       list[0]);
         Assert.Equal("UnityEngine",  list[1]);
      }

      [Fact]
      public void Parse_HelpFlag()
      {
         var parser = new ArgParser();
         var result = parser.Parse(new[] { "--help" });

         Assert.True(result.HelpRequested);
      }

      [Fact]
      public void Parse_HelpAfterGroup()
      {
         var parser = new ArgParser();
         var result = parser.Parse(new[] { "flow", "--help" });

         Assert.Equal("flow", result.Group);
         Assert.True(result.HelpRequested);
      }

   #endregion

   #region Global Options

      [Fact]
      public void Parse_GlobalOptions()
      {
         var parser = new ArgParser(new[] { "port", "timeout" });
         var result = parser.Parse(new[] { "--port", "18960", "--timeout", "30", "ping" });

         Assert.Equal("18960", result.GlobalOptions["port"]);
         Assert.Equal("30",    result.GlobalOptions["timeout"]);
         Assert.Equal("ping",  result.Group);
      }

   #endregion

   #region Schema-based Parsing

      [Fact]
      public void Parse_Schema_ValidGroup()
      {
         var schema = CreateTestSchema();
         var parser = new ArgParser(schema);
         var result = parser.Parse(new[] { "flow", "continue" });

         Assert.Equal("flow",     result.Group);
         Assert.Equal("continue", result.Command);
      }

      [Fact]
      public void Parse_Schema_UnknownGroup_Throws()
      {
         var schema = CreateTestSchema();
         var parser = new ArgParser(schema);

         Assert.Throws<CliException>(() => parser.Parse(new[] { "unknown" }));
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> In schema mode, an unknown second token is treated as a
      /// <br/> positional arg (not a command), so no exception is thrown.
      /// <br/> The group "flow" is valid, command is empty.
      /// </summary>
      // ----------------------------------------------------------------------
      [Fact]
      public void Parse_Schema_UnknownToken_BecomesPositional()
      {
         var schema = CreateTestSchema();
         var parser = new ArgParser(schema);
         var result = parser.Parse(new[] { "flow", "fly" });

         Assert.Equal("flow", result.Group);
         Assert.Equal("",     result.Command);
         Assert.Single(result.Positional);
         Assert.Equal("fly",  result.Positional[0]);
      }

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> With schema, a second token that is not a known command
      /// <br/> should be treated as a positional argument, not a command.
      /// </summary>
      // ----------------------------------------------------------------------
      [Fact]
      public void Parse_Schema_NonCommandToken_IsPositional()
      {
         var schema = CreateTestSchema();
         var parser = new ArgParser(schema);
         var result = parser.Parse(new[] { "break", "path/to/file.cs", "42" });

         Assert.Equal("break", result.Group);
         Assert.Equal("",      result.Command);
         Assert.Equal(2,       result.Positional.Count);
         Assert.Equal("path/to/file.cs", result.Positional[0]);
         Assert.Equal("42",              result.Positional[1]);
      }

      [Fact]
      public void Parse_Schema_HelpBypassesValidation()
      {
         var schema = CreateTestSchema();
         var parser = new ArgParser(schema);

         // "unknown" group would throw, but --help bypasses validation
         var result = parser.Parse(new[] { "unknown", "--help" });

         Assert.True(result.HelpRequested);
         Assert.Equal("unknown", result.Group);
      }

      [Fact]
      public void Parse_Schema_GlobalOptionFromSchema()
      {
         var schema = CreateTestSchema();
         var parser = new ArgParser(schema);
         var result = parser.Parse(new[] { "--timeout", "10", "flow", "continue" });

         Assert.Equal("10",       result.GlobalOptions["timeout"]);
         Assert.Equal("flow",     result.Group);
         Assert.Equal("continue", result.Command);
      }

   #endregion

   #region Helpers

      private static CliSchema CreateTestSchema()
      {
         return CliSchema.Parse(@"{
            ""globalOptions"": {
               ""timeout"": { ""type"": ""int"", ""description"": ""Timeout in seconds"" }
            },
            ""groups"": {
               ""flow"": {
                  ""description"": ""Execution flow control"",
                  ""commands"": {
                     ""continue"": { ""description"": ""Continue execution"" },
                     ""step"":     { ""description"": ""Step over"" }
                  }
               },
               ""break"": {
                  ""description"": ""Breakpoints"",
                  ""commands"": {
                     ""remove"": { ""description"": ""Remove breakpoint"" },
                     ""list"":   { ""description"": ""List breakpoints"" }
                  }
               }
            }
         }");
      }

   #endregion

   }
}
