using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using Castle.MonoRail.Framework;

namespace AdminInterface.Components
{
	public class SettingCheckbox : ViewComponent
	{
		public override void Render()
		{
			var typeName = String.Empty;
			if (ComponentParams["TypeName"] == null)
				throw new Exception("Не указано имя типа. Параметер TypeName пуст.");
			typeName = Convert.ToString(ComponentParams["TypeName"]);

			var instanceName = String.Empty;
			if (ComponentParams["InstanceName"] == null)
				throw new Exception("Не указано имя переменной экземпляра. Параметр InstanceName пуст.");
			instanceName = Convert.ToString(ComponentParams["InstanceName"]);

			if (ComponentParams["Checkboxes"] == null)
				throw new Exception("Элементы для создания checkboxes не заданы. Параметер Checkboxes пуст.");

			var headerName = String.Empty;
			if (ComponentParams["Header"] != null)
				headerName = Convert.ToString(ComponentParams["Header"]);

			var writer = new StringWriter();
			if (!String.IsNullOrEmpty(headerName))
				writer.WriteLine(@"<h4>{0}</h4>", headerName);

			var checkboxes = (IDictionary)ComponentParams["Checkboxes"];
			foreach (var key in checkboxes.Keys)
			{
				var isChecked = String.Empty;
				if (Convert.ToBoolean(checkboxes[key]))
					isChecked = "checked";
				writer.WriteLine(@"
<input type='checkbox' id='{0}.{1}' name='{0}.{1}' value='true' {2} />
<input type='hidden' name='{0}.{1}' value='false' />
<label for='{0}.{1}'>{3}</label><br />", instanceName, key, isChecked, GetDescription(typeName, Convert.ToString(key)));
			}
			writer.WriteLine("<br />");
			RenderText(writer.ToString());
		}

		private static string GetDescription(string typeName, string propertyName)
		{
			var property = Type.GetType(typeName).GetProperty(propertyName);
			var attributes = property.GetCustomAttributes
					(typeof(DescriptionAttribute), false);
			return ((DescriptionAttribute)attributes[0]).Description;
		}
	}
}