using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Castle.MonoRail.Framework;

namespace AdminInterface.Components
{
	public class Legend : ViewComponent
	{
		public override void Render()
		{
			if (ComponentParams["LegendItems"] == null)
				throw new Exception("Элементы для заполнения легенды не заданы. Параметер LegendItems пуст.");

			var style = "";
			if (ComponentParams["ByCenter"] == null || Convert.ToBoolean(ComponentParams["ByCenter"]))
				style = "class='CenterBlock' style='width:30%;'";
			var writer = new StringWriter();
			writer.WriteLine(@"
	<div {0}>
		<table>
			<tr>
", style);
			var legendItems = (IDictionary)ComponentParams["LegendItems"];
			foreach (var key in legendItems.Keys)
				writer.WriteLine(@"
<tr>
				<td>
					<div class='LegendMarker {0}'></div>
				</td>
				<td align='left'>
					- {1}
				</td>
</tr>
", legendItems[key], key);

			writer.WriteLine(@"
			</tr>
		</table>
	</div>");

			RenderText(writer.ToString());
		}
	}
}