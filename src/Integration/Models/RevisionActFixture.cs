using System.Diagnostics;
using AdminInterface.Models.Billing;
using ExcelLibrary.SpreadSheet;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class RevisionActFixture
	{
		[Test]
		public void Export_to_excel()
		{
			var act = DataMother.GetAct();
			var sheet = Exporter.Export(act);
			Assert.That(sheet, Is.Not.Null);
/*			var workbook = new Workbook();
			workbook.Worksheets.Add(sheet);
			workbook.Save("1.xls");
			Process.Start("1.xls");*/
		}
	}
}