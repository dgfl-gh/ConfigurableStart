using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace CustomScenarioManager
{
    public static class CSMExtensions
    {
        // ConfigNode extensions
        public static bool CSMTryGetValue<T>(this ConfigNode node, string name, out T value)
        {
            value = default;
            string valueString = default;

            if (node.TryGetValue(name, ref valueString))
            {
                return valueString.CSMTryParse<T>(out value);
            }

            return false;
        }
        // string extensions
        public static bool CSMTryParse<T>(this string input, out T value )
        {
            value = default;
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    value = (T)converter.ConvertFromString(input) ?? default;
                    return true;
                }
                return false;
            }
            catch (NotSupportedException)
            {
                Utilities.Log($"Cannot parse {typeof(T)} from {input}");
                return false;
            }
        }
    }

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
        public static Dictionary<string, T> DictionaryFromStringArray<T>(string[] inputArray, char[] separator = null)
        {
            separator ??= new char[] { '@' };

            var dict = new Dictionary<string, T>();
            foreach (string s in inputArray)
            {
                string[] array = s.Split(separator, 2);
                if (array[1].CSMTryParse(out T value))
                    dict[array[0]] = value;
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
