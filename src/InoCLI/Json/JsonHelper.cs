using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// JSON read/write utilities.
   /// </summary>
   // ============================================================
   public static class JsonHelper
   {

   #region Read — Int

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Extracts an int from a JsonElement.
      /// <br/> Handles both number and string representations.
      /// </summary>
      // ----------------------------------------------------------------------
      public static int GetInt(JsonElement element, int fallback = 0)
      {
         if (element.ValueKind == JsonValueKind.Number)
         {
            return element.GetInt32();
         }

         if (element.ValueKind == JsonValueKind.String)
         {
            if (int.TryParse(element.GetString(), out int val))
            {
               return val;
            }
         }

         return fallback;
      }

   #endregion

   #region Read — Float

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Extracts a float from a JsonElement.
      /// <br/> Handles both number and string representations.
      /// </summary>
      // ----------------------------------------------------------------------
      public static float GetFloat(JsonElement element, float fallback = 0f)
      {
         if (element.ValueKind == JsonValueKind.Number)
         {
            return element.GetSingle();
         }

         if (element.ValueKind == JsonValueKind.String)
         {
            if (float.TryParse(element.GetString(), out float val))
            {
               return val;
            }
         }

         return fallback;
      }

   #endregion

   #region Read — String

      // ------------------------------------------------------------
      /// <summary>
      /// Extracts a string from a JsonElement with fallback.
      /// </summary>
      // ------------------------------------------------------------
      public static string GetString(JsonElement element, string fallback = null)
      {
         if (element.ValueKind == JsonValueKind.String)
         {
            return element.GetString();
         }

         return fallback;
      }

   #endregion

   #region Read — Bool

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Extracts a bool from a JsonElement.
      /// <br/> Handles bool, string ("true"/"false"), and number (0/1).
      /// </summary>
      // ----------------------------------------------------------------------
      public static bool GetBool(JsonElement element, bool fallback = false)
      {
         if (element.ValueKind == JsonValueKind.True)
         {
            return true;
         }

         if (element.ValueKind == JsonValueKind.False)
         {
            return false;
         }

         if (element.ValueKind == JsonValueKind.String)
         {
            if (bool.TryParse(element.GetString(), out bool val))
            {
               return val;
            }
         }

         if (element.ValueKind == JsonValueKind.Number)
         {
            return element.GetInt32() != 0;
         }

         return fallback;
      }

   #endregion

   #region Write

      private static readonly JsonSerializerOptions PrettyOptions = new() { WriteIndented = true };

      // ------------------------------------------------------------
      /// <summary>
      /// Writes JSON to stdout, optionally pretty-printed.
      /// </summary>
      // ------------------------------------------------------------
      public static void Write(string json, bool pretty = false)
      {
         if (pretty)
         {
            json = Prettify(json);
         }

         Console.WriteLine(json);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Writes JSON to stderr, optionally pretty-printed.
      /// </summary>
      // ------------------------------------------------------------
      public static void WriteError(string json, bool pretty = false)
      {
         if (pretty)
         {
            json = Prettify(json);
         }

         Console.Error.WriteLine(json);
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Re-formats a JSON string with indentation.
      /// </summary>
      // ------------------------------------------------------------
      public static string Prettify(string json)
      {
         var doc = JsonDocument.Parse(json);

         return JsonSerializer.Serialize(doc, PrettyOptions);
      }

   #endregion

   }
}
