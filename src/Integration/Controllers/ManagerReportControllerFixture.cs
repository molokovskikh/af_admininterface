using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Queries;
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
			Assert.That(ws.Name, Is.StringContaining("зарегистрированные пользователи и адреса в регионе"));
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

		[Test]
		public void ExportSwitchOffClientsTest()
		{
			var filter = new SwitchOffClientsFilter();
			var buf = ExportModel.ExcelSwitchOffClients(filter);
			var stream = new MemoryStream(buf);
			var wb = Workbook.Load(stream);
			var ws = wb.Worksheets.First();
			Assert.That(ws.Name, Is.StringContaining("Список отключенных клиентов"));
		}

		[Test]
		public void ExportWhoWasNotUpdatedFilterTest()
		{
			var filter = new WhoWasNotUpdatedFilter();
			filter.Session = session;
			var buf = ExportModel.ExcelWhoWasNotUpdated(filter);
			var stream = new MemoryStream(buf);
			var wb = Workbook.Load(stream);
			var ws = wb.Worksheets.First();
			Assert.That(ws.Name, Is.StringContaining("Кто не обновлялся с опред. даты"));
		}

		[Test]
		public void ExportUpdatedAndDidNotDoOrdersFilterTest()
		{
			var filter = new UpdatedAndDidNotDoOrdersFilter {
				Session = session
			};
			var stream = new MemoryStream(filter.Excel());
			var wb = Workbook.Load(stream);
			var ws = wb.Worksheets.First();
			Assert.That(ws.Name, Is.StringContaining("Кто обновлялся и не делал заказы"));
		}

		[Test]
		public void ExportAnalysisOfWorkDrugstoresTest()
		{
			var filter = new AnalysisOfWorkDrugstoresFilter();
			filter.Session = session;
			var buf = ExportModel.ExcelAnalysisOfWorkDrugstores(filter);
			var stream = new MemoryStream(buf);
			var wb = Workbook.Load(stream);
			var ws = wb.Worksheets.First();
			Assert.That(ws.Name, Is.StringContaining("Сравнительный анализ работы аптек"));
		}

		[Test]
		public void ExportClientConditionsMonitoringTest()
		{
			var filter = new ClientConditionsMonitoringFilter();
			filter.Session = session;
			var buf = ExportModel.GetClientConditionsMonitoring(filter);
			var stream = new MemoryStream(buf);
			var wb = Workbook.Load(stream);
			var ws = wb.Worksheets.First();
			Assert.That(ws.Name, Is.StringContaining("Мониторинг выставления условий"));
		}
	}
}