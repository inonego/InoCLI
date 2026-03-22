using System;
using System.Text.Json;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Helper methods for extracting typed values from JsonElement.
   /// Handles both string and number representations.
   /// </summary>
   // ============================================================
   public static class JsonHelper
   {

   #region Int

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

         if (element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), out int val))
         {
            return val;
         }

         return fallback;
      }

   #endregion

   #region Long

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Extracts a long from a JsonElement.
      /// <br/> Handles both number and string representations.
      /// </summary>
      // ----------------------------------------------------------------------
      public static long GetLong(JsonElement element, long fallback = 0)
      {
         if (element.ValueKind == JsonValueKind.Number)
         {
            return element.GetInt64();
         }

         if (element.ValueKind == JsonValueKind.String && long.TryParse(element.GetString(), out long val))
         {
            return val;
         }

         return fallback;
      }

   #endregion

   #region String

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

   #region Bool

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

         if (element.ValueKind == JsonValueKind.String && bool.TryParse(element.GetString(), out bool val))
         {
            return val;
         }

         if (element.ValueKind == JsonValueKind.Number)
         {
            return element.GetInt32() != 0;
         }

         return fallback;
      }

   #endregion

   }
}
