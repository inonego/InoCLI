using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace InoCLI
{
   // ============================================================
   /// <summary>
   /// Discovers [CLICommand] methods via reflection.
   /// </summary>
   // ============================================================
   public static class CommandScanner
   {

   #region Methods

      // ------------------------------------------------------------
      /// <summary>
      /// Scans a single assembly for [CLICommand] methods.
      /// </summary>
      // ------------------------------------------------------------
      public static List<CommandInfo> Scan(Assembly assembly)
      {
         var result = new List<CommandInfo>();

         Type[] types;

         try
         {
            types = assembly.GetTypes();
         }
         catch (ReflectionTypeLoadException ex)
         {
            types = Array.FindAll(ex.Types, t => t != null);
         }
         catch
         {
            return result;
         }

         foreach (var type in types)
         {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

            foreach (var method in methods)
            {
               var attr = method.GetCustomAttribute<CLICommandAttribute>();

               if (attr == null)
               {
                  continue;
               }

               result.Add(new CommandInfo(attr.Path, attr.Description, method));
            }
         }

         return result;
      }

      // ------------------------------------------------------------
      /// <summary>
      /// Scans multiple assemblies for [CLICommand] methods.
      /// </summary>
      // ------------------------------------------------------------
      public static List<CommandInfo> ScanAll(IEnumerable<Assembly> assemblies)
      {
         var result = new List<CommandInfo>();

         foreach (var assembly in assemblies)
         {
            result.AddRange(Scan(assembly));
         }

         return result;
      }

   #endregion

   }
}
