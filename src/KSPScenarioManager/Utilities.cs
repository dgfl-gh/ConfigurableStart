using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

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
                Utilities.LogErr($"Cannot parse {typeof(T)} from {input}");
                return false;
            }
        }
    }

    public class Utilities
    {
        [Conditional("DEBUG")]
        public static void Log(object s)
        {
            UnityEngine.Debug.Log("[CustomScenarioManager] " + s);
        }

        public static void LogErr(object s)
        {
            UnityEngine.Debug.LogError($"[CustomScenarioManager] " + s);
        }

        public static void LogWrn(object s)
        {
            UnityEngine.Debug.LogWarning($"[CustomScenarioManager] " + s);
        }

        public static Dictionary<string, T> DictionaryFromCommaSeparatedString<T>(string input, char[] separator = null, T defaultValue = default)
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
            return ArrayFromString<string>(listString, ',');
        }

        public static T[] ArrayFromString<T>(string listString, params char[] separator)
        {
            if (string.IsNullOrEmpty(listString)) return new T[] { };

            listString = listString.Trim();

            IEnumerable<string> selection = listString.Split(separator);
            selection = selection.Select(s => s.Trim()).Where(s => s != string.Empty);

            var result = new List<T>();

            foreach(var s in selection)
            {
                s.CSMTryParse<T>(out T temp);
                result.Add(temp);
            }

            return result.ToArray();
        }

        public static string FormatCommaSeparatedList(string input)
        {
            string[] array = ArrayFromCommaSeparatedList(input);
            if (array.Length == 0) return "";

            return string.Join(",", array);
        }
    }
}
