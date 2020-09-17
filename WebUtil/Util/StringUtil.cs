using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WebUtil.Util
{
    public static class StringUtil
    {
        public static string CutAndComposite(this string input, string value, int startIndex, int n, string composite)
        {
            var position = input.IndexOfNth(value, startIndex, n);
            return input.Substring(0, position) + composite;
        }

        public static int IndexOfNth(this string input,
                             string value, int startIndex, int nth)
        {
            if (nth < 1)
                throw new NotSupportedException("Param 'nth' must be greater than 0!");
            if (nth == 1)
                return input.IndexOf(value, startIndex);
            var idx = input.IndexOf(value, startIndex);
            if (idx == -1)
                return -1;
            return input.IndexOfNth(value, idx + 1, --nth);
        }

        public static string Substring(this string input, string value)
        {
            var index = input.IndexOf(value);
            if (index == -1)
            {
                return input;
            }
            return input.Substring(0, index);
        }

        public static string GetValue(this string[] values, List<string> thList, string key, int cursor)
        {
            var at = thList.IndexOf(key);
            return at == -1 ? string.Empty : values[cursor + at];
        }

        public static int? ToIntNullable(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return int.Parse(str, NumberStyles.AllowThousands);
        }

        public static int ToInt(this string str, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }
            return int.Parse(str, NumberStyles.AllowThousands);
        }


        public static int ToIntShorthand(this string str, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }

            return int.Parse(str.Replace("k", "000")
                .Replace("m", "000000")
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty));
        }

        public static int ToInt(this double value)
        {
            return Convert.ToInt32(value);
        }

        public static double ToDouble(this string str)
        {
            return double.Parse(Regex.Match(str, @"[0-9\-.]+").Value);
        }

        public static float ToFloat(this string str)
        {
            return float.Parse(Regex.Match(str, @"[0-9\-.]+").Value);
        }

        public static string ExtractKorean(this string str)
        {
            return Regex.Replace(str, "[^가-힣]", "");
        }

    }
}
