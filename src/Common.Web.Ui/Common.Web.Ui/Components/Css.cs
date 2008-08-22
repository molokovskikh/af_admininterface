using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Castle.MonoRail.Framework;

namespace Common.Web.Ui.Components
{
	public class CssComponent : ViewComponent
	{
		public override void Render()
		{
			if (!Context.ContextVars.Contains("Css"))
			{
				Context.ContextVars.Add("Css", new List<string>());
				Context.ContextVars.Add("Css.@bubbleUp", true);
			}

			var css = (List<string>)Context.ContextVars["Css"];

			var buffer = new StringWriter();

			Context.RenderBody(buffer);

			var reader = new StringReader(buffer.ToString());
			var line = reader.ReadLine();
			while(line != null)
			{
				var lineToCheck = line.Trim().ToLower();
				if (!String.IsNullOrEmpty(lineToCheck) && !css.Contains(lineToCheck))
					css.Add(lineToCheck);
				line = reader.ReadLine();
			}
		}
	}

	public class CssHeader : ViewComponent
	{
		public override void Render()
		{
			if (!Context.ContextVars.Contains("Css"))
				return;

			foreach (var css in (IEnumerable<string>)Context.ContextVars["Css"])
				RenderText(String.Format("<link href='{0}' type='text/css' rel='stylesheet' />", css));
		}
	}
}
