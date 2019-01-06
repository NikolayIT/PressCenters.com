namespace PressCenters.Common
{
    using System;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        public static string GetStringBetween(this string input, string startString, string endString, int startFrom = 0)
        {
            input = input.Substring(startFrom);
            if (!input.Contains(startString) || !input.Contains(endString))
            {
                return string.Empty;
            }

            var startPosition = input.IndexOf(startString, StringComparison.Ordinal) + startString.Length;
            if (startPosition == -1)
            {
                return string.Empty;
            }

            var endPosition = input.IndexOf(endString, startPosition, StringComparison.Ordinal);
            if (endPosition == -1)
            {
                return string.Empty;
            }

            return input.Substring(startPosition, endPosition - startPosition);
        }

        public static string GetLastStringBetween(this string input, string startString, string endString, string defaultValue = "")
        {
            var endIndex = input.LastIndexOf(endString, StringComparison.Ordinal);
            if (endIndex == -1)
            {
                return defaultValue;
            }

            var startIndex = input.LastIndexOf(startString, endIndex, StringComparison.Ordinal);
            if (startIndex == -1)
            {
                return defaultValue;
            }

            return input.Substring(startIndex + 1, endIndex - startIndex - 1);
        }

        public static int ToInteger(this string input)
        {
            int.TryParse(input, out var integerValue);
            return integerValue;
        }

        public static string StripHtml(this string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
