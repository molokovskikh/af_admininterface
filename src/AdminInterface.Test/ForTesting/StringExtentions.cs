using System;
using System.Collections.Generic;

namespace AdminInterface.Test.ForTesting
{
    public static class StringExtentions
    {
        public static string Format(this string s, params object[] parameters)
        {
            return String.Format(s, parameters);
        }

        public static IEnumerable<TResult> Transform<T, TResult>(this IEnumerable<T> equatable,
                                                                 Func<T, TResult> action)
        {
            foreach (var t in equatable)
            {
                yield return action(t);
            }
        }
    }
}
