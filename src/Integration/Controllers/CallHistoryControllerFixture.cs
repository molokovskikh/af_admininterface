using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Telephony;
using Common.Web.Ui.Helpers;
using ExcelLibrary.SpreadSheet;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class CallHistoryControllerFixture : ControllerFixture
	{
		private CallHistoryController _controller;
		private CallRecordFilter _filter;

		[SetUp]
		public new void Setup()
		{
			_controller = new CallHistoryController();
			Prepare(_controller);

			_filter = new CallRecordFilter();

			var query = session.CreateSQLQuery(@"
INSERT INTO logs.RecordCalls (`From`, `To`, WriteTime, NameFrom, NameTo, CallType)
VALUES ('123', '321', '2012-07-07', 'from', 'to', 1)");
			query.ExecuteUpdate();
			Flush();
		}

		[Test]
		public void ExportExcelTest()
		{
			_filter.BeginDate = new DateTime(2012, 7, 6);
			_filter.EndDate = new DateTime(2012, 7, 8);
			var buf = ExportModel.GetCallsHistory(_filter);
			var stream = new MemoryStream(buf);
			var wb = Workbook.Load(stream);
			var ws = wb.Worksheets.First();
			Assert.That(ws.Name, Is.StringContaining("История звонков"));
			Assert.That(ws.Cells.GetRow(0).GetCell(1).Value, Is.EqualTo("Дата звонка"));
			Assert.That(ws.Cells.GetRow(0).GetCell(2).Value, Is.EqualTo("Номер звонившего"));
			Assert.That(ws.Cells.GetRow(0).GetCell(3).Value, Is.EqualTo("Имя звонившего"));
			Assert.That(ws.Cells.GetRow(0).GetCell(4).Value, Is.EqualTo("Куда звонил"));
			Assert.That(ws.Cells.GetRow(0).GetCell(5).Value, Is.EqualTo("Кому звонил"));
			Assert.That(ws.Cells.GetRow(0).GetCell(6).Value, Is.EqualTo("Тип звонка"));

			Assert.That(ws.Cells.GetRow(1).GetCell(2).Value, Is.EqualTo("123"));
			Assert.That(ws.Cells.GetRow(1).GetCell(3).Value, Is.EqualTo("from"));
			Assert.That(ws.Cells.GetRow(1).GetCell(4).Value, Is.EqualTo("321"));
			Assert.That(ws.Cells.GetRow(1).GetCell(5).Value, Is.EqualTo("to"));
			Assert.That(ws.Cells.GetRow(1).GetCell(6).Value, Is.EqualTo(CallType.Incoming.GetDescription()));
		}
	}
}