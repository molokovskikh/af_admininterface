using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Tools;

namespace AdminInterface.Helpers
{
	public class Styler
	{
		public IEnumerable<string> GetStyles(object item)
		{
			if (item == null)
				return Enumerable.Empty<string>();

			var properties = item.GetType().Types().SelectMany(t => GetProperties(t));

			return properties.Where(p => (bool)p.GetValue(item, null)).Select(p => ToStyle(p.Name));
		}

		private static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			var properties = type.GetProperties()
				.Where(p => p.GetCustomAttributes(typeof (StyleAttribute), true).Length > 0)
				.Where(p => p.PropertyType == typeof (bool));
			return properties;
		}

		public static string ToStyle(string name)
		{
			var words = new List<string>();
			var word = "";
			foreach (var @char in name)
			{
				if (!String.IsNullOrEmpty(word) && char.IsUpper(@char))
				{
					words.Add(word);
					word = @char.ToString().ToLower();
				}
				else
				{
					word += @char.ToString().ToLower()[0];
				}
			}

			if (!String.IsNullOrEmpty(word))
				words.Add(word);

			if (words[0] == "is")
				words = words.Skip(1).ToList();

			return String.Join("-", words);
		}
	}

	public class StyleAttribute : Attribute
	{
	}

}