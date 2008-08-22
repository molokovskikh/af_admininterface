using System.Collections;
using System.Collections.Generic;
using System;
using System.ComponentModel;

namespace Common.Web.Ui.Helpers
{
	public class BindingHelper
	{
		public static Dictionary<object, string> GetDescriptionsDictionary(Type type)
		{
			var description = new Dictionary<object, string>();
			foreach (var value in Enum.GetValues(type))
			{
				var fieldInfo = value.GetType().GetField(value.ToString());
				var attributes =  (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
				description.Add(Convert.ToInt32(value), attributes[0].Description);
			}
			return description;
		}

		public static Dictionary<object, string> GetDescriptionsDictionary(string typeName)
		{
			var type = Type.GetType(typeName);
			if (type == null)
				throw new Exception(String.Format("Тип c именем {0} не найден", typeName));
			return GetDescriptionsDictionary(Type.GetType(typeName));
		}

		public static string GetDescription(object value)
		{
			var descriptions = value.GetType().GetField(value.ToString()).GetCustomAttributes(false);
			return ((DescriptionAttribute)descriptions[0]).Description;
		}

		public static IDictionary GetDictionaryMapping()
		{
			return GetDictionaryMapping(null);
		}

		public static IDictionary GetDictionaryMapping(IDictionary dictionary)
		{
			if (dictionary == null)
				dictionary = new Dictionary<string, string>();
			dictionary.Add("value", "Key");
			dictionary.Add("text", "Value");
			return dictionary;
		}
	}
}
