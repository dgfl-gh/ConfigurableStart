using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public static bool CSMTryParse<T>(this string input, out T value, T defaultValue = default)
        {
            value = defaultValue;
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    value = (T)converter.ConvertFromString(input) ?? defaultValue;
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
            Debug.Log($"[CustomScenarioManager] {s}");
#endif
        }

        public static Dictionary<string, T> DictionaryFromString<T>(string input, char[] separator = null, T defaultValue = default)
        {
            string[] array = ArrayFromCommaSeparatedList(input);
            return DictionaryFromStringArray(array, separator, defaultValue);
        }

        /// <summary>
        /// Create a Dictionary&lt;string, int&gt; from an array of strings.
        /// The strings in the array have to be formatted as "Key{separator}Value".
        /// <br> separator is defaulted as '@'.</br>
        /// </summary>
        /// <param name="inputArray">The string array that will be the source for the dictionary</param>
        /// <param name="separator">The char array that will serve as the possible substring delimiters. Defaulted as '@'</param>
        /// <returns></returns>
        public static Dictionary<string, T> DictionaryFromStringArray<T>(string[] inputArray, char[] separator = null, T defaultValue = default)
        {
            separator ??= new char[] { '@' };
            var dict = new Dictionary<string, T>();

            if (inputArray != null && inputArray.Length > 0)
            {
                foreach (string s in inputArray)
                {
                    string[] array = s.Split(separator, 2);

                    if (array.Length > 1 && array[1].CSMTryParse(out T value, defaultValue))
                        dict[array[0]] = value;
                    else
                        dict[array[0]] = defaultValue;
                }
            }

            return dict;
        }

        public static string[] ArrayFromCommaSeparatedList(string listString)
        {
            if (string.IsNullOrEmpty(listString)) return new string[] { };

            listString = listString.Trim();

            IEnumerable<string> selection = listString.Split(',');
            selection = selection.Select(s => s.Trim());
            selection = selection.Where(s => s != string.Empty);

            return selection.ToArray();
        }

        public static string FormatCommaSeparatedList(string input)
        {
            string[] array = ArrayFromCommaSeparatedList(input);
            if (array.Length == 0) return "";

            return string.Join(",", array);
        }
    }
}
