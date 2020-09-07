using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace CustomScenarioManager
{
    public class Utilities
    {
        private static bool? _isKCTInstalled = null;

        public static bool KCTFound
        {
            get
            {
                if (!_isKCTInstalled.HasValue)
                {
                    _isKCTInstalled = AssemblyLoader.loadedAssemblies.Any(a => string.Equals(a.name, "KerbalConstructionTime", StringComparison.OrdinalIgnoreCase));
                }
                return _isKCTInstalled.Value;
            }
        }

        public static void Log(string s)
        {
#if DEBUG
            Debug.Log($"[CustomScenarioManager]: {s}");
#endif
        }

        public static string[] ArrayFromCommaSeparatedList(string listString)
        {
            listString.Trim();

            IEnumerable<string> selection = listString.Split(',');
            selection = selection.Select(s => s.Trim());
            selection = selection.Where(s => s != string.Empty);

            return selection.ToArray();
        }
    }
}
