using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CustomScenarioManager
{
    public class DateHandler
    {
        public static long Epoch { get; set; } = 0;

        private static readonly IDateTimeFormatter timeFormatter = KSPUtil.dateTimeFormatter;

        public static string GetFormattedDateString(long UT)
        {
            return timeFormatter.PrintDateCompact(UT, true);
        }

        public static string GetDatePreview(string dateString)
        {
            if (!long.TryParse(dateString, out long newUT))
            {
                if (TryParseStockDate(dateString, out newUT)) { }
                else if (TryParseDate(dateString, out DateTime newEpoch))
                {
                    DateTime gameStart = new DateTime(1951, 01, 01, 0, 0, 0);

                    TimeSpan span = newEpoch.Subtract(gameStart);
                    newUT = TimeSpanToSeconds(span);
                }
                else { newUT = Epoch; }
            }

            if (newUT != 0)
                return GetFormattedDateString(newUT);
            else
                return "Invalid date";
        }

        public static long GetUTFromDate(string dateString)
        {
            if (!long.TryParse(dateString, out long newUT))
            {
                if (TryParseStockDate(dateString, out newUT))
                {
                    Utilities.Log("Stock date format detected");
                }
                else if (TryParseDate(dateString, out DateTime newEpoch))
                {
                    Utilities.Log("RSS date format detected");

                    DateTime gameStart = new DateTime(1951, 01, 01, 0, 0, 0);

                    TimeSpan span = newEpoch.Subtract(gameStart);
                    newUT = TimeSpanToSeconds(span);
                }
                else
                {
                    Utilities.LogWrn("Unsupported date format");
                    newUT = Epoch;
                }
            }
            else
            {
                Utilities.Log("Epoch format detected");
            }

            return newUT;
        }

        public static long TimeSpanToSeconds(TimeSpan span)
        {
            long years = span.Days / 365;
            long days = span.Days - years * 365;
            long hours = span.Hours;
            long minutes = span.Minutes;
            long seconds = span.Seconds;

            long totalSeconds = years * timeFormatter.Year + days * timeFormatter.Day + hours * timeFormatter.Hour + minutes * timeFormatter.Minute + seconds;

            return totalSeconds;
        }

        public static bool TryParseDate(string dateString, out DateTime dateTime)
        {
            try
            {
                dateTime = DateTime.Parse(dateString);
                return true;
            }
            catch
            {
                dateTime = DateTime.MinValue;
                return false;
            }
        }

        public static bool TryParseStockDate(string dateString, out long newUT)
        {
            newUT = Epoch;
            var pattern = new Regex(@"^[Y]?\d{4}\-[dD]?\d+$");
            Match m = pattern.Match(dateString);

            if (m.Success)
            {
                string[] yearDayArray = m.Value.Split(new char[] { '-' }, 2);
                if (!long.TryParse(yearDayArray[0], out long years))
                    return false;
                if (!long.TryParse(yearDayArray[1], out long days))
                    return false;

                newUT += --years * timeFormatter.Year + --days * timeFormatter.Day;
            }

            return m.Success;
        }
    }
}
