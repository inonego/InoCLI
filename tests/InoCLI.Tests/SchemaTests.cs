using System;

using Xunit;

namespace InoCLI.Tests
{
   // ============================================================
   /// <summary>
   /// Tests for CliSchema parsing and query methods.
   /// </summary>
   // ============================================================
   public class SchemaTests
   {

   #region Parse

      [Fact]
      public void Parse_Groups()
      {
         var schema = CliSchema.Parse(@"{
            ""groups"": {
               ""flow"": {
                  ""description"": ""Flow control"",
                  ""commands"": {
                     ""step"": { ""description"": ""Step over"" }
                  }
               }
            }
         }");

         Assert.True(schema.HasGroup("flow"));
         Assert.False(schema.HasGroup("unknown"));
         Assert.Equal("Flow control", schema.Groups["flow"].Description);
      }

      [Fact]
      public void Parse_Commands()
      {
         var schema = CliSchema.Parse(@"{
            ""groups"": {
               ""break"": {
                  ""commands"": {
                     ""set"":    { ""description"": ""Set breakpoint"" },
                     ""remove"": { ""description"": ""Remove breakpoint"" }
                  }
               }
            }
         }");

         Assert.True(schema.HasCommand("break", "set"));
         Assert.True(schema.HasCommand("break", "remove"));
         Assert.False(schema.HasCommand("break", "fly"));
         Assert.False(schema.HasCommand("unknown", "set"));
      }

      [Fact]
      public void Parse_CommandArgs()
      {
         var schema = CliSchema.Parse(@"{
            ""groups"": {
               ""break"": {
                  ""commands"": {
                     ""set"": {
                        ""args"": [
                           { ""name"": ""file"", ""type"": ""string"", ""required"": true },
                           { ""name"": ""line"", ""type"": ""int"",    ""required"": true }
                        ]
                     }
                  }
               }
            }
         }");

         var cmd = schema.Groups["break"].Commands["set"];

         Assert.Equal(2,      cmd.Args.Count);
         Assert.Equal("file", cmd.Args[0].Name);
         Assert.True(cmd.Args[0].Required);
         Assert.Equal("int",  cmd.Args[1].Type);
      }

      [Fact]
      public void Parse_CommandOptions()
      {
         var schema = CliSchema.Parse(@"{
            ""groups"": {
               ""flow"": {
                  ""commands"": {
                     ""step"": {
                        ""options"": {
                           ""count"": { ""type"": ""int"", ""description"": ""Number of steps"" }
                        }
                     }
                  }
               }
            }
         }");

         var opt = schema.Groups["flow"].Commands["step"].Options["count"];

         Assert.Equal("int",             opt.Type);
         Assert.Equal("Number of steps", opt.Description);
      }

      [Fact]
      public void Parse_GlobalOptions()
      {
         var schema = CliSchema.Parse(@"{
            ""globalOptions"": {
               ""timeout"": { ""type"": ""int"" },
               ""pretty"":  { ""type"": ""bool"" }
            },
            ""groups"": {}
         }");

         Assert.True(schema.IsGlobalOption("timeout"));
         Assert.True(schema.IsGlobalOption("pretty"));
         Assert.False(schema.IsGlobalOption("unknown"));
         Assert.Equal("int",  schema.GlobalOptions["timeout"].Type);
         Assert.Equal("bool", schema.GlobalOptions["pretty"].Type);
      }

      [Fact]
      public void Parse_RepeatedOption()
      {
         var schema = CliSchema.Parse(@"{
            ""groups"": {
               ""eval"": {
                  ""commands"": {
                     ""cs"": {
                        ""options"": {
                           ""using"": { ""type"": ""string"", ""repeated"": true }
                        }
                     }
                  }
               }
            }
         }");

         var opt = schema.Groups["eval"].Commands["cs"].Options["using"];

         Assert.True(opt.Repeated);
      }

   #endregion

   }
}
