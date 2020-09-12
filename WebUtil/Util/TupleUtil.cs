using System;
using System.Collections.Generic;

namespace WebUtil.Util
{
    public static class TupleUtil
    {
        public static string FindValue(this List<Tuple<string, string>> tuples, string key)
        {
            foreach (var tuple in tuples)
            {
                var value = tuple.FindValue(key);
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            return string.Empty;
        }

        public static string FindValue(this Tuple<string, string> tuple, string key)
        {
            if (string.IsNullOrEmpty(tuple.Item1))
            {
                return string.Empty;
            }

            return tuple.Item1.Contains(key) ? tuple.Item2 : string.Empty;
        }
    }
}
