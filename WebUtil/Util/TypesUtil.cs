using System;
using WebUtil.Common;

namespace WebUtil.Util
{
    public static class TypesUtil
    {
        public static int Code<T>(this T e) where T : struct
        {
            return (int)(object)e;
        }

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }

        public static Enum FromDescription(Type type, string description)
        {
            foreach (var e in Enum.GetValues(type))
            {
                Enum eValue = (Enum)Enum.ToObject(type, e);
                if (eValue.GetDescription() == description)
                {
                    return eValue;
                }
            }

            return null;
        }

        public static Nullable<T> FromDescription<T>(string description) where T : struct
        {
            foreach (T e in (T[])Enum.GetValues(typeof(T)))
            {
                Enum eValue = (Enum)Enum.ToObject(typeof(T), e);
                if (eValue.GetDescription() == description)
                {
                    return e;
                }
            }

            return null;
        }

        public static Nullable<T> FromName<T>(string name) where T : struct
        {
            name = name.ToUpper();
            foreach (T e in (T[])Enum.GetValues(typeof(T)))
            {
                if (e.ToString().ToUpper() == name)
                {
                    return e;
                }
            }

            return null;
        }
    }
}
