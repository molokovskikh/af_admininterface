using System;
using System.Collections.Generic;

namespace Functional.ForTesting
{
	public static class StringExtentions
	{
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