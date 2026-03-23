using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Result of parsing CLI arguments.
   /// </summary>
   // ============================================================
   public class ParsedArgs
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Positional arguments
      /// </summary>
      // ------------------------------------------------------------
      public List<string> Positionals { get; set; } = new();

      // ------------------------------------------------------------
      /// <summary>
      /// Optional arguments
      /// </summary>
      // ------------------------------------------------------------
      public Dictionary<string, List<string>> Optionals { get; set; } = new();

   #endregion

   #region Positional Helpers

      // ------------------------------------------------------------
      /// <summary>
      /// Returns true if the positional index exists.
      /// </summary>
      // ------------------------------------------------------------
      public bool Has(int index) => 0 <= index && index < Positionals.Count;

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional by index, or null if out of range.
      /// </summary>
      // ------------------------------------------------------------
      public string this[int index] => Has(index) ? Positionals[index] : null;

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as int. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public int GetInt(int index)
      {
         var s = this[index] ?? throw new CliException($"Missing positional at index {index}");
         if (!int.TryParse(s, out int v))
         {
            throw new CliException($"Invalid int at index {index}: {s}");
         }
         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as float. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public float GetFloat(int index)
      {
         var s = this[index] ?? throw new CliException($"Missing positional at index {index}");
         if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
         {
            throw new CliException($"Invalid float at index {index}: {s}");
         }
         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as bool. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public bool GetBool(int index)
      {
         var s = this[index] ?? throw new CliException($"Missing positional at index {index}");
         if (!bool.TryParse(s, out bool v))
         {
            throw new CliException($"Invalid bool at index {index}: {s}");
         }
         return v;
      }

   #endregion

   #region Optional Helpers

      // ------------------------------------------------------------
      /// <summary>
      /// Returns true if the optional key exists (flag or valued).
      /// </summary>
      // ------------------------------------------------------------
      public bool Has(string key) => Optionals.ContainsKey(key);

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value of an optional, or null if not present.
      /// </summary>
      // ------------------------------------------------------------
      public string this[string key] => Optionals.TryGetValue(key, out var v) && v.Count > 0 ? v[0] : null;

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as int. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public int GetInt(string key)
      {
         var s = this[key] ?? throw new CliException($"Missing option: --{key}");
         if (!int.TryParse(s, out int v))
         {
            throw new CliException($"Invalid int for --{key}: {s}");
         }
         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as float. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public float GetFloat(string key)
      {
         var s = this[key] ?? throw new CliException($"Missing option: --{key}");
         if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
         {
            throw new CliException($"Invalid float for --{key}: {s}");
         }
         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as bool. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public bool GetBool(string key)
      {
         var s = this[key] ?? throw new CliException($"Missing option: --{key}");
         if (!bool.TryParse(s, out bool v))
         {
            throw new CliException($"Invalid bool for --{key}: {s}");
         }
         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets all values for an optional key. Throws if missing.
      /// </summary>
      // ------------------------------------------------------------
      public List<string> All(string key)
      {
         if (!Optionals.TryGetValue(key, out var v))
         {
            throw new CliException($"Missing option: --{key}");
         }
         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets all values as int list. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public List<int> AllInt(string key)
      {
         var result = new List<int>();
         foreach (var s in All(key))
         {
            if (!int.TryParse(s, out int v))
            {
               throw new CliException($"Invalid int for --{key}: {s}");
            }
            result.Add(v);
         }
         return result;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets all values as float list. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public List<float> AllFloat(string key)
      {
         var result = new List<float>();
         foreach (var s in All(key))
         {
            if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            {
               throw new CliException($"Invalid float for --{key}: {s}");
            }
            result.Add(v);
         }
         return result;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets all values as bool list. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public List<bool> AllBool(string key)
      {
         var result = new List<bool>();
         foreach (var s in All(key))
         {
            if (!bool.TryParse(s, out bool v))
            {
               throw new CliException($"Invalid bool for --{key}: {s}");
            }
            result.Add(v);
         }
         return result;
      }

   #endregion

   }
}
