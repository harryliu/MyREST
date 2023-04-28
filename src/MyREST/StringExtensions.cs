using System.Text.RegularExpressions;

namespace MyREST
{
    public static class StringExtensions
    {
        public static string toSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }

        public static string replaceWholeWord(string input, string searchWord, string replacement)
        {
            string pattern = @"\b" + searchWord + @"\b";
            string result = Regex.Replace(input, pattern, replacement);
            return result;
        }

        public static int occurrenceCount(string str, string subStr)
        {
            int count = 0;
            int index = str.IndexOf(subStr);

            while (index != -1)
            {
                count++;
                index = str.IndexOf(subStr, index + 1);
            }
            return count;
        }

        public static string removeSpecialChars(string paramName)
        {
            // Regular expression to match special characters
            Regex regex = new Regex("[@!#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]");

            // Replace special characters with an empty string
            return regex.Replace(paramName, "");
        }
    }
}