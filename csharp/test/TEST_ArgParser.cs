using System;
using System.Collections;
using System.Collections.Generic;

using Xunit;

using InoCLI;

namespace InoCLI.TEST
{
   // ============================================================
   /// <summary>
   /// Tests for ArgParser: positionals, optionals, helpers.
   /// </summary>
   // ============================================================
   public class TEST_ArgParser
   {

   #region Positionals

      [Fact]
      public void Parse_SinglePositional()
      {
         var result = new ArgParser().Parse(new[] { "ping" });

         Assert.Single(result.Positionals);
         Assert.Equal("ping", result[0]);
      }

      [Fact]
      public void Parse_MultiplePositionals()
      {
         var result = new ArgParser().Parse(new[] { "build", "src/main.cs", "42" });

         Assert.Equal(3, result.Positionals.Count);
         Assert.Equal("build", result[0]);
         Assert.Equal("src/main.cs", result[1]);
         Assert.Equal("42", result[2]);
      }

      [Fact]
      public void Parse_NegativeNumber_IsPositional()
      {
         var result = new ArgParser().Parse(new[] { "calc", "-42" });

         Assert.Equal(2, result.Positionals.Count);
         Assert.Equal("-42", result[1]);
      }

   #endregion

   #region Optionals — Long

      [Fact]
      public void Parse_LongOption_WithValue()
      {
         var result = new ArgParser().Parse(new[] { "deploy", "--retries", "3" });

         Assert.True(result.Has("retries"));
         Assert.Equal("3", result["retries"]);
      }

      [Fact]
      public void Parse_LongOption_Flag()
      {
         var result = new ArgParser().Parse(new[] { "status", "--verbose" });

         Assert.True(result.Has("verbose"));
         Assert.Empty(result.Optionals["verbose"]);
      }

      [Fact]
      public void Parse_LongOption_Repeated()
      {
         var result = new ArgParser().Parse(new[] { "build", "src/main.cs", "--tag", "release", "--tag", "latest" });

         var tags = result.Optionals["tag"];

         Assert.Equal(2, tags.Count);
         Assert.Equal("release", tags[0]);
         Assert.Equal("latest", tags[1]);
      }

   #endregion

   #region Optionals — Short

      [Fact]
      public void Parse_ShortOption_WithValue()
      {
         var result = new ArgParser().Parse(new[] { "deploy", "-r", "3" });

         Assert.True(result.Has("r"));
         Assert.Equal("3", result["r"]);
      }

      [Fact]
      public void Parse_ShortOption_Flag()
      {
         var result = new ArgParser().Parse(new[] { "status", "-v" });

         Assert.True(result.Has("v"));
         Assert.Empty(result.Optionals["v"]);
      }

   #endregion

   #region Mixed

      [Fact]
      public void Parse_OptionsBetweenPositionals()
      {
         var result = new ArgParser().Parse(new[] { "build", "src/main.cs", "--filter", "x > 0", "42" });

         Assert.Equal("build", result[0]);
         Assert.Equal("src/main.cs", result[1]);
         Assert.Equal("42", result[2]);
         Assert.Equal("x > 0", result["filter"]);
      }

      [Fact]
      public void Parse_Empty()
      {
         var result = new ArgParser().Parse(new string[0]);

         Assert.Empty(result.Positionals);
         Assert.Empty(result.Optionals);
      }

   #endregion

   #region Error

      [Fact]
      public void Parse_DoubleDash_Throws()
      {
         Assert.Throws<ArgumentException>(() => new ArgParser().Parse(new[] { "--" }));
      }

   #endregion

   #region Indexer

      [Fact]
      public void Indexer_Positional_ReturnsValue()
      {
         var result = new ArgParser().Parse(new[] { "ping" });

         Assert.Equal("ping", result[0]);
      }

      [Fact]
      public void Indexer_Positional_OutOfRange_ReturnsNull()
      {
         var result = new ArgParser().Parse(new[] { "ping" });

         Assert.Null(result[5]);
      }

      [Fact]
      public void Indexer_Optional_ReturnsFirstValue()
      {
         var result = new ArgParser().Parse(new[] { "--retries", "3" });

         Assert.Equal("3", result["retries"]);
      }

      [Fact]
      public void Indexer_Optional_Missing_ReturnsNull()
      {
         var result = new ArgParser().Parse(new[] { "ping" });

         Assert.Null(result["missing"]);
      }

   #endregion

   #region Has

      [Fact]
      public void Has_Positional_Exists()
      {
         var result = new ArgParser().Parse(new[] { "ping" });

         Assert.True(result.Has(0));
      }

      [Fact]
      public void Has_Positional_Missing()
      {
         var result = new ArgParser().Parse(new[] { "ping" });

         Assert.False(result.Has(1));
      }

      [Fact]
      public void Has_Optional_Exists()
      {
         var result = new ArgParser().Parse(new[] { "--verbose" });

         Assert.True(result.Has("verbose"));
      }

      [Fact]
      public void Has_Optional_Missing()
      {
         var result = new ArgParser().Parse(new[] { "ping" });

         Assert.False(result.Has("missing"));
      }

   #endregion

   #region GetInt

      [Fact]
      public void GetInt_Positional()
      {
         var result = new ArgParser().Parse(new[] { "connect", "8080" });

         Assert.Equal(8080, result.GetInt(1));
      }

      [Fact]
      public void GetInt_Positional_Missing_Throws()
      {
         var result = new ArgParser().Parse(new[] { "ping" });

         Assert.Throws<ArgumentException>(() => result.GetInt(5));
      }

      [Fact]
      public void GetInt_Optional()
      {
         var result = new ArgParser().Parse(new[] { "--retries", "3" });

         Assert.Equal(3, result.GetInt("retries"));
      }

      [Fact]
      public void GetInt_Optional_Missing_Throws()
      {
         var result = new ArgParser().Parse(new[] { "ping" });

         Assert.Throws<ArgumentException>(() => result.GetInt("retries"));
      }

      [Fact]
      public void GetInt_Optional_Invalid_Throws()
      {
         var result = new ArgParser().Parse(new[] { "--retries", "abc" });

         Assert.Throws<ArgumentException>(() => result.GetInt("retries"));
      }

   #endregion

   #region GetFloat

      [Fact]
      public void GetFloat_Positional()
      {
         var result = new ArgParser().Parse(new[] { "resize", "1.5" });

         Assert.Equal(1.5f, result.GetFloat(1));
      }

      [Fact]
      public void GetFloat_Optional()
      {
         var result = new ArgParser().Parse(new[] { "--ratio", "1.5" });

         Assert.Equal(1.5f, result.GetFloat("ratio"));
      }

   #endregion

   #region GetBool

      [Fact]
      public void GetBool_Positional()
      {
         var result = new ArgParser().Parse(new[] { "toggle", "true" });

         Assert.True(result.GetBool(1));
      }

      [Fact]
      public void GetBool_Optional()
      {
         var result = new ArgParser().Parse(new[] { "--verbose", "true" });

         Assert.True(result.GetBool("verbose"));
      }

   #endregion

   #region All

      [Fact]
      public void All_ReturnsValues()
      {
         var result = new ArgParser().Parse(new[] { "--tag", "release", "--tag", "latest" });

         var values = result.All("tag");

         Assert.Equal(2, values.Count);
         Assert.Equal("release", values[0]);
         Assert.Equal("latest", values[1]);
      }

      [Fact]
      public void All_Missing_Throws()
      {
         var result = new ArgParser().Parse(new[] { "ping" });

         Assert.Throws<ArgumentException>(() => result.All("missing"));
      }

   #endregion

   }
}
