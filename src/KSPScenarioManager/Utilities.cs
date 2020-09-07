using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace CustomScenarioManager
{
    public class Utilities
    {
        public static void Log(string s)
        {
#if DEBUG
            Debug.Log($"[CustomScenarioManager]: {s}");
#endif
        }

        /// <summary>
        /// Create a Dictionary&lt;string, int&gt; from an array of strings.
        /// The strings in the array have to be formatted as "Key{separator}Value".
        /// <br> separator is defaulted as '@'.</br>
        /// </summary>
        /// <param name="inputArray">The string array that will be the source for the dictionary</param>
        /// <param name="separator">The char array that will serve as the possible substring delimiters. Defaulted as '@'</param>
        /// <returns></returns>
        public static Dictionary<string, int> DictionaryFromStringArray(string[] inputArray, char[] separator = null)
        {
            separator ??= new char[] { '@' };

            var dict = new Dictionary<string, int>();
            foreach (string s in inputArray)
            {
                string[] array = s.Split(separator, 2);
                if (int.TryParse(array[1], out int level))
                    dict[array[0]] = level;
            }

            return dict;
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
