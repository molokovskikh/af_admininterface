using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using ExcelLibrary.SpreadSheet;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class ManagerReportControllerFixture : ControllerFixture
	{
		private ManagerReportsController _controller;
		private UserFinderFilter _filter;

		[SetUp]
		public new void Setup()
		{
			_controller = new ManagerReportsController();
			Prepare(_controller);

			_filter = new UserFinderFilter();
		}

		[Test]
		public void ExportExcelTest()
		{
			_filter.FinderType = RegistrationFinderType.Addresses;
			var buf = ExportModel.GetUserOrAdressesInformation(_filter);
			var stream = new MemoryStream(buf);
			var wb = Workbook.Load(stream);
			var ws = wb.Worksheets.First();
			Assert.That(ws.Name, Is.StringContaining("Зарегистрированные пользователи и адреса в регионе"));
			Assert.That(ws.Cells.GetRow(1).GetCell(0).Value, Is.EqualTo("Регион:"));
			Assert.That(ws.Cells.GetRow(1).GetCell(1).Value, Is.EqualTo("Все"));
			Assert.That(ws.Cells.GetRow(2).GetCell(0).Value, Is.EqualTo("Период:"));
			Assert.That(ws.Cells.GetRow(2).GetCell(1).Value, Is.EqualTo(string.Format("С {0} по {1}", _filter.Period.Begin.ToShortDateString(), _filter.Period.End.ToShortDateString())));
			Assert.That(ws.Cells.GetRow(3).GetCell(0).Value, Is.EqualTo("Код клиента"));
			Assert.That(ws.Cells.GetRow(3).GetCell(1).Value, Is.EqualTo("Наименование клиента"));
			Assert.That(ws.Cells.GetRow(3).GetCell(2).Value, Is.EqualTo("Регион"));
			Assert.That(ws.Cells.GetRow(3).GetCell(3).Value, Is.EqualTo("Код адреса"));
			Assert.That(ws.Cells.GetRow(3).GetCell(4).Value, Is.EqualTo("Адрес"));
			Assert.That(ws.Cells.GetRow(3).GetCell(5).Value, Is.EqualTo("Дата регистрации"));
			Assert.That(ws.Cells.GetRow(3).GetCell(6).Value, Is.EqualTo("С этим адресом зарегистрированы пользователи, код пользователя (комментарий к пользователю)"));
		}
	}
}