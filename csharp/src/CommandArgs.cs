using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Parsed CLI arguments: positionals and optionals.
   /// Used both as parse result and as command handler input.
   /// </summary>
   // ============================================================
   public class CommandArgs
   {

   #region Fields

      // ------------------------------------------------------------
      /// <summary>
      /// Positional arguments.
      /// </summary>
      // ------------------------------------------------------------
      public List<string> Positionals { get; set; } = new();

      // ------------------------------------------------------------
      /// <summary>
      /// Optional arguments (keyed, each key maps to a value list).
      /// </summary>
      // ------------------------------------------------------------
      public Dictionary<string, List<string>> Optionals { get; set; } = new();

   #endregion

   #region Positional Helpers

      // ------------------------------------------------------------
      /// <summary>
      /// Number of positional arguments.
      /// </summary>
      // ------------------------------------------------------------
      public int Count => Positionals.Count;

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional by index, or null if out of range.
      /// </summary>
      // ------------------------------------------------------------
      public string this[int index] => Has(index) ? Positionals[index] : null;

      // ------------------------------------------------------------
      /// <summary>
      /// Returns true if the positional index exists.
      /// </summary>
      // ------------------------------------------------------------
      public bool Has(int index) => 0 <= index && index < Positionals.Count;

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional by index, or fallback if out of range.
      /// </summary>
      // ------------------------------------------------------------
      public string Get(int index, string fallback = null)
      {
         return this[index] ?? fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as int. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public int GetInt(int index)
      {
         var s = this[index] ?? throw new ArgumentException($"Missing positional at index {index}");

         if (!int.TryParse(s, out int v))
         {
            throw new ArgumentException($"Invalid int at index {index}: {s}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as int, or fallback if missing/invalid.
      /// </summary>
      // ------------------------------------------------------------
      public int GetInt(int index, int fallback)
      {
         var s = this[index];

         if (s != null && int.TryParse(s, out int v))
         {
            return v;
         }

         return fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as long. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public long GetLong(int index)
      {
         var s = this[index] ?? throw new ArgumentException($"Missing positional at index {index}");

         if (!long.TryParse(s, out long v))
         {
            throw new ArgumentException($"Invalid long at index {index}: {s}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as long, or fallback if missing/invalid.
      /// </summary>
      // ------------------------------------------------------------
      public long GetLong(int index, long fallback)
      {
         var s = this[index];

         if (s != null && long.TryParse(s, out long v))
         {
            return v;
         }

         return fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as float. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public float GetFloat(int index)
      {
         var s = this[index] ?? throw new ArgumentException($"Missing positional at index {index}");

         if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
         {
            throw new ArgumentException($"Invalid float at index {index}: {s}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as float, or fallback if missing/invalid.
      /// </summary>
      // ------------------------------------------------------------
      public float GetFloat(int index, float fallback)
      {
         var s = this[index];

         if (s != null && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
         {
            return v;
         }

         return fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as double. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public double GetDouble(int index)
      {
         var s = this[index] ?? throw new ArgumentException($"Missing positional at index {index}");

         if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
         {
            throw new ArgumentException($"Invalid double at index {index}: {s}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as double, or fallback if missing/invalid.
      /// </summary>
      // ------------------------------------------------------------
      public double GetDouble(int index, double fallback)
      {
         var s = this[index];

         if (s != null && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
         {
            return v;
         }

         return fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as bool. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public bool GetBool(int index)
      {
         var s = this[index] ?? throw new ArgumentException($"Missing positional at index {index}");

         if (!bool.TryParse(s, out bool v))
         {
            throw new ArgumentException($"Invalid bool at index {index}: {s}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets a positional as bool, or fallback if missing/invalid.
      /// </summary>
      // ------------------------------------------------------------
      public bool GetBool(int index, bool fallback)
      {
         var s = this[index];

         if (s != null && bool.TryParse(s, out bool v))
         {
            return v;
         }

         return fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Returns positionals from the given index onward.
      /// </summary>
      // ------------------------------------------------------------
      public string[] From(int index)
      {
         if (index < 0 || index >= Positionals.Count)
         {
            return Array.Empty<string>();
         }

         return Positionals.GetRange(index, Positionals.Count - index).ToArray();
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

      // ----------------------------------------------------------------------
      /// <summary>
      /// <br/> Returns true if the flag exists.
      /// <br/> A flag is an optional with no value (empty list).
      /// </summary>
      // ----------------------------------------------------------------------
      public bool Flag(string key) => Optionals.ContainsKey(key);

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value, or fallback if not present.
      /// </summary>
      // ------------------------------------------------------------
      public string Get(string key, string fallback = null)
      {
         return this[key] ?? fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as int. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public int GetInt(string key)
      {
         var s = this[key] ?? throw new ArgumentException($"Missing option: --{key}");

         if (!int.TryParse(s, out int v))
         {
            throw new ArgumentException($"Invalid int for --{key}: {s}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as int, or fallback if missing/invalid.
      /// </summary>
      // ------------------------------------------------------------
      public int GetInt(string key, int fallback)
      {
         var s = this[key];

         if (s != null && int.TryParse(s, out int v))
         {
            return v;
         }

         return fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as long. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public long GetLong(string key)
      {
         var s = this[key] ?? throw new ArgumentException($"Missing option: --{key}");

         if (!long.TryParse(s, out long v))
         {
            throw new ArgumentException($"Invalid long for --{key}: {s}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as long, or fallback if missing/invalid.
      /// </summary>
      // ------------------------------------------------------------
      public long GetLong(string key, long fallback)
      {
         var s = this[key];

         if (s != null && long.TryParse(s, out long v))
         {
            return v;
         }

         return fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as float. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public float GetFloat(string key)
      {
         var s = this[key] ?? throw new ArgumentException($"Missing option: --{key}");

         if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
         {
            throw new ArgumentException($"Invalid float for --{key}: {s}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as float, or fallback if missing/invalid.
      /// </summary>
      // ------------------------------------------------------------
      public float GetFloat(string key, float fallback)
      {
         var s = this[key];

         if (s != null && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
         {
            return v;
         }

         return fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as double. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public double GetDouble(string key)
      {
         var s = this[key] ?? throw new ArgumentException($"Missing option: --{key}");

         if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
         {
            throw new ArgumentException($"Invalid double for --{key}: {s}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as double, or fallback if missing/invalid.
      /// </summary>
      // ------------------------------------------------------------
      public double GetDouble(string key, double fallback)
      {
         var s = this[key];

         if (s != null && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
         {
            return v;
         }

         return fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as bool. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public bool GetBool(string key)
      {
         var s = this[key] ?? throw new ArgumentException($"Missing option: --{key}");

         if (!bool.TryParse(s, out bool v))
         {
            throw new ArgumentException($"Invalid bool for --{key}: {s}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets the first value as bool, or fallback if missing/invalid.
      /// </summary>
      // ------------------------------------------------------------
      public bool GetBool(string key, bool fallback)
      {
         var s = this[key];

         if (s != null && bool.TryParse(s, out bool v))
         {
            return v;
         }

         return fallback;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets all values for a key. Throws if missing.
      /// </summary>
      // ------------------------------------------------------------
      public List<string> All(string key)
      {
         if (!Optionals.TryGetValue(key, out var v))
         {
            throw new ArgumentException($"Missing option: --{key}");
         }

         return v;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets all values for a key, or fallback if missing.
      /// </summary>
      // ------------------------------------------------------------
      public List<string> All(string key, List<string> fallback)
      {
         if (Optionals.TryGetValue(key, out var v))
         {
            return v;
         }

         return fallback;
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
               throw new ArgumentException($"Invalid int for --{key}: {s}");
            }

            result.Add(v);
         }

         return result;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets all values as long list. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public List<long> AllLong(string key)
      {
         var result = new List<long>();

         foreach (var s in All(key))
         {
            if (!long.TryParse(s, out long v))
            {
               throw new ArgumentException($"Invalid long for --{key}: {s}");
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
               throw new ArgumentException($"Invalid float for --{key}: {s}");
            }

            result.Add(v);
         }

         return result;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Gets all values as double list. Throws if missing or invalid.
      /// </summary>
      // ------------------------------------------------------------
      public List<double> AllDouble(string key)
      {
         var result = new List<double>();

         foreach (var s in All(key))
         {
            if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
            {
               throw new ArgumentException($"Invalid double for --{key}: {s}");
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
               throw new ArgumentException($"Invalid bool for --{key}: {s}");
            }

            result.Add(v);
         }

         return result;
      }

   #endregion

   }
}
