using System;
using System.Collections;
using System.IO;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

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
<label for='{0}.{1}'>{3}</label><br />", instanceName, key, isChecked, BindingHelper.GetDescription(typeName + ", AdminInterface", Convert.ToString(key)));
			}
			writer.WriteLine("<br />");
			RenderText(writer.ToString());
		}
	}
}