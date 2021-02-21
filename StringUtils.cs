using System;

namespace RottenTomatoes
{
    public static class StringUtils
    {
        // Cut stuff before in a string
        public static string CutBefore(this string source, string target) =>
            source.Substring(source.IndexOf(target, StringComparison.Ordinal) + target.Length);

        // Cut stuff after in a string
        public static string CutAfter(this string source, string target) =>
            source.Substring(0, source.IndexOf(target, StringComparison.Ordinal));

        // Cut stuff before a string and cut stuff after a string
        public static string CutBeforeAndAfter(this string source, string targetOne, string targetTwo)
        {
            source = CutBefore(source, targetOne);
            return CutAfter(source, targetTwo);
        }
    }
}