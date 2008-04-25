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
			
			var writer = new StringWriter();
			writer.WriteLine(@"
	<div class='CenterBlock' style='width:30%;'>
		<table>
			<tr>
");
			var legendItems = (IDictionary) ComponentParams["LegendItems"];
			foreach (var key in legendItems.Keys)
				writer.WriteLine(@"
<tr>
				<td>
					<div class='LegendMarker {0}'></div>
				</td>
				<td>
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
