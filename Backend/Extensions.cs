using System;

namespace Backend
{
    public static class Extensions
    {
        public static bool IsAnyOf<T>(this T value, params T[] values) where T : Enum
        {
            foreach (var v in values)
            {
                if (value.Equals(v))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
