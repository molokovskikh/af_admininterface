using System;
using Castle.MonoRail.Framework;

namespace Common.Web.Ui.Components
{
	public class ValidationErrorViewer : ViewComponent
	{
		public override bool SupportsSection(string name)
		{
			return name == "item";
		}

		public override void Render()
		{
			var instance = ComponentParams["instance"];
			var validationErrorMessages = new string[0];

			if (validationErrorMessages.Length == 0 &&
				instance.GetType().GetProperty("ValidationErrorMessages") != null)
			{
				validationErrorMessages = (string[])instance
					.GetType()
					.GetProperty("ValidationErrorMessages")
					.GetValue(instance, null);
			}


			if (validationErrorMessages.Length == 0) 
				return;

			PropertyBag["ValidationError"] = String.Join(", ", validationErrorMessages);
			Context.RenderSection("item");
		}
	}
}