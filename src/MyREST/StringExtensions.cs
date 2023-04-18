using System.Text.RegularExpressions;

namespace MyREST
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
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
    }
}