using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Web;
using AdminInterface.Controllers;
using AdminInterface.Controllers.Filters;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models.Telephony;
using AdminInterface.Queries;
using Castle.Components.DictionaryAdapter;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Excel;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using ExcelLibrary.BinaryFileFormat;
using ExcelLibrary.CompoundDocumentFormat;
using ExcelLibrary.Office.Excel.BinaryFileFormat.Records;
using ExcelLibrary.SpreadSheet;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using BorderStyle = NPOI.SS.UserModel.BorderStyle;
using ExcelHelper = AdminInterface.Helpers.ExcelHelper;
using HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment;

namespace AdminInterface.Models
{
	public class ExportModel
	{
		private static IEnumerable<RegistrationInformation> GetRegistrationInformation(UserFinderFilter filter)
		{
			var criteria = filter.GetCriteria();

			filter.ApplySort(criteria);

			return ArHelper.WithSession(
				s => criteria.GetExecutableCriteria(s).ToList<RegistrationInformation>())
				.ToList();
		}

		private static IEnumerable<SwitchOffCounts> GetExcelSwitchOffClients(SwitchOffClientsFilter filter)
		{
			var criteria = filter.GetCriteria();

			filter.ApplySort(criteria);

			return ArHelper.WithSession(s => filter.Find(s, true)).ToList();
		}

		private static void FormatUserAndAdresses(Worksheet ws, UserFinderFilter filter)
		{
			int headerRow = 3;
			ExcelHelper.WriteHeader1(ws, headerRow, 0, "Код клиента", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 1, "Наименование клиента", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 2, "Регион", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 3, filter.HeadCodeName, true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 4, filter.HeadName, true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 5, "Дата регистрации", true, true);
			if (filter.ShowUserNames())
				ExcelHelper.WriteHeader1(ws, headerRow, 6, "С этим адресом зарегистрированы пользователи, код пользователя (комментарий к пользователю)", true, true);

			ws.Cells.ColumnWidth[0] = 4000;
			ws.Cells.ColumnWidth[1] = 12000;
			ws.Cells.ColumnWidth[2] = 6000;
			ws.Cells.ColumnWidth[3] = 3000;
			ws.Cells.ColumnWidth[4] = 12000;
			ws.Cells.ColumnWidth[5] = 8000;
			if (filter.ShowUserNames())
				ws.Cells.ColumnWidth[6] = 15000;

			ws.Cells.Rows[headerRow].Height = 514;
		}

		private static void FormatSwitchOffClients(Worksheet ws, SwitchOffClientsFilter filter)
		{
			int headerRow = 3;
			ExcelHelper.WriteHeader1(ws, headerRow, 0, "Код клиента", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 1, "Наименование клиента", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 2, "Регион", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 3, "Дата отключения", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 4, "Комментарий оператора", true, true);

			ws.Cells.ColumnWidth[0] = 4000;
			ws.Cells.ColumnWidth[1] = 12000;
			ws.Cells.ColumnWidth[2] = 6000;
			ws.Cells.ColumnWidth[3] = 5000;
			ws.Cells.ColumnWidth[4] = 20000;

			ws.Cells.Rows[headerRow].Height = 514;
		}

		private static void FormatWhoWasNotUpdated(Worksheet ws, int headerRow = 4)
		{
			ExcelHelper.WriteHeader1(ws, headerRow, 0, "Код клиента", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 1, "Наименование клиента", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 2, "Код пользователя", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 3, "Комментарий пользователя", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 4, "Регион", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 5, "Регистратор", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 6, "Дата обновления", true, true);

			ws.Cells.ColumnWidth[0] = 4000;
			ws.Cells.ColumnWidth[1] = 12000;
			ws.Cells.ColumnWidth[2] = 4000;
			ws.Cells.ColumnWidth[3] = 12000;
			ws.Cells.ColumnWidth[4] = 6000;
			ws.Cells.ColumnWidth[5] = 8000;
			ws.Cells.ColumnWidth[6] = 6000;

			ws.Cells.Rows[headerRow].Height = 514;
		}

		private static void UpdatedAndDidNotDoOrders(Worksheet ws)
		{
			int headerRow = 5;
			FormatWhoWasNotUpdated(ws, 5);
			ExcelHelper.WriteHeader1(ws, headerRow, 7, "Дата последнего заказа", true, true);
			ws.Cells.ColumnWidth[7] = 6000;
		}

		private static void AnalysisOfWorkDrugstoresFormat(Worksheet ws)
		{
			int headerRow = 5;
			ExcelHelper.WriteHeader1(ws, headerRow, 0, "Код", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 1, "Наименование", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 2, "Количество пользователей", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 3, "Количество адресов доставки", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 4, "Регион", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 5, "Обновления (Новый/Старый)", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 6, "Падение обновлений", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 7, "Заказы (Новый/Старый)", true, true);
			ExcelHelper.WriteHeader1(ws, headerRow, 8, "Падение заказов", true, true);

			ws.Cells.ColumnWidth[0] = 4000;
			ws.Cells.ColumnWidth[1] = 6000;
			ws.Cells.ColumnWidth[2] = 4000;
			ws.Cells.ColumnWidth[3] = 4000;
			ws.Cells.ColumnWidth[4] = 6000;
			ws.Cells.ColumnWidth[5] = 6000;
			ws.Cells.ColumnWidth[6] = 4000;
			ws.Cells.ColumnWidth[7] = 10000;
			ws.Cells.ColumnWidth[8] = 4000;

			ws.Cells.Rows[headerRow].Height = 514;
		}

		public static byte[] GetUserOrAdressesInformation(UserFinderFilter filter)
		{
			var wb = new Workbook();
			var ws = new Worksheet("Зарегистрированные пользователи и адреса в регионе");

			int row = 4;
			int colShift = 0;

			ws.Merge(0, 0, 0, 6);

			if (filter.FinderType == RegistrationFinderType.Users)
				ExcelHelper.WriteHeader1(ws, 0, 0, "Зарегистрированные пользователи", false, true);
			if (filter.FinderType == RegistrationFinderType.Addresses)
				ExcelHelper.WriteHeader1(ws, 0, 0, "Зарегистрированные адреса", false, true);

			ws.Merge(1, 1, 1, 2);
			ExcelHelper.Write(ws, 1, 0, "Регион:", false);
			string regionName;
			if (filter.Region == null)
				regionName = "Все";
			else {
				regionName = filter.Region.Name;
			}
			ExcelHelper.Write(ws, 1, 1, regionName, false);

			ws.Merge(2, 1, 2, 2);
			ExcelHelper.Write(ws, 2, 0, "Период:", false);
			if (filter.Period.Begin != filter.Period.End)
				ExcelHelper.Write(ws, 2, 1,
					"С " + filter.Period.Begin.ToString("dd.MM.yyyy") + " по " + filter.Period.End.ToString("dd.MM.yyyy"), false);
			else
				ExcelHelper.Write(ws, 2, 1, "За " + filter.Period.Begin.ToString("dd.MM.yyyy"), false);

			var reportData = GetRegistrationInformation(filter);

			var showUserNames = filter.ShowUserNames();

			foreach (var item in reportData) {
				ExcelHelper.Write(ws, row, colShift + 0, item.ClientId, true);
				ExcelHelper.Write(ws, row, colShift + 1, item.ClientName, true);
				ExcelHelper.Write(ws, row, colShift + 2, item.RegionName, true);
				ExcelHelper.Write(ws, row, colShift + 3, item.Id, true);
				ExcelHelper.Write(ws, row, colShift + 4, item.Name, true);
				ExcelHelper.Write(ws, row, colShift + 5, item.RegistrationDate, true);
				if (showUserNames)
					ExcelHelper.Write(ws, row, colShift + 6, item.UserNames, true);
				row++;
			}

			FormatUserAndAdresses(ws, filter);

			wb.Worksheets.Add(ws);
			var ms = new MemoryStream();
			wb.Save(ms);

			return ms.ToArray();
		}

		public static byte[] ExcelSwitchOffClients(SwitchOffClientsFilter filter)
		{
			var wb = new Workbook();
			var ws = new Worksheet("Список отключенных клиентов");

			int row = 4;
			int colShift = 0;

			ws.Merge(0, 0, 0, 6);

			ExcelHelper.WriteHeader1(ws, 0, 0, "Список отключенных клиентов", false, true);

			ws.Merge(1, 1, 1, 2);
			ExcelHelper.Write(ws, 1, 0, "Регион:", false);
			string regionName;
			if (filter.Region == null)
				regionName = "Все";
			else {
				regionName = filter.Region.Name;
			}
			ExcelHelper.Write(ws, 1, 1, regionName, false);

			ws.Merge(2, 1, 2, 2);
			ExcelHelper.Write(ws, 2, 0, "Период:", false);
			if (filter.Period.Begin != filter.Period.End)
				ExcelHelper.Write(ws, 2, 1,
					"С " + filter.Period.Begin.ToString("dd.MM.yyyy") + " по " + filter.Period.End.ToString("dd.MM.yyyy"), false);
			else
				ExcelHelper.Write(ws, 2, 1, "За " + filter.Period.Begin.ToString("dd.MM.yyyy"), false);

			var reportData = GetExcelSwitchOffClients(filter);

			foreach (var item in reportData) {
				ExcelHelper.Write(ws, row, colShift + 0, item.ClientId, true);
				ExcelHelper.Write(ws, row, colShift + 1, item.ClientName, true);
				ExcelHelper.Write(ws, row, colShift + 2, item.RegionName, true);
				ExcelHelper.Write(ws, row, colShift + 3, item.LogTime, true);
				ExcelHelper.Write(ws, row, colShift + 4, item.Comment, true);
				row++;
			}

			FormatSwitchOffClients(ws, filter);

			wb.Worksheets.Add(ws);
			var ms = new MemoryStream();
			wb.Save(ms);

			return ms.ToArray();
		}

		public static byte[] ExcelUpdatedAndDidNotDoOrders(UpdatedAndDidNotDoOrdersFilter filter)
		{
			var wb = new Workbook();
			var ws = new Worksheet("Кто обновлялся и не делал заказы");

			int row = 6;
			int colShift = 0;

			ws.Merge(0, 0, 0, 6);

			ExcelHelper.WriteHeader1(ws, 0, 0, "Кто обновлялся и не делал заказы", false, true);

			ws.Merge(1, 1, 1, 2);
			ExcelHelper.Write(ws, 1, 0, "Регион:", false);
			string regionName;
			if (filter.Region == null)
				regionName = "Все";
			else {
				regionName = filter.Region.Name;
			}
			ExcelHelper.Write(ws, 1, 1, regionName, false);

			ws.Merge(2, 1, 2, 2);
			ExcelHelper.Write(ws, 2, 0, "Период:", false);
			if (filter.UpdatePeriod.Begin != filter.UpdatePeriod.End)
				ExcelHelper.Write(ws, 2, 1, "С " + filter.UpdatePeriod.Begin.ToString("dd.MM.yyyy") + " по " + filter.UpdatePeriod.End.ToString("dd.MM.yyyy"), false);
			else
				ExcelHelper.Write(ws, 2, 1, "За " + filter.UpdatePeriod.Begin.ToString("dd.MM.yyyy"), false);

			ws.Merge(3, 0, 3, 3);
			ExcelHelper.Write(ws, 3, 0, "Не делались заказы с: " + filter.OrderDate.ToString("dd.MM.yyyy"), false);
			//ExcelHelper.Write(ws, 3, 1, , false);

			var reportData = ArHelper.WithSession(s => {
				filter.Session = s;
				var result = filter.Find(true);
				foreach (var updatedAndDidNotDoOrdersField in result) {
					updatedAndDidNotDoOrdersField.ForExport = true;
				}
				return result;
			});

			foreach (var item in reportData) {
				ExcelHelper.Write(ws, row, colShift + 0, item.ClientId, true);
				ExcelHelper.Write(ws, row, colShift + 1, item.ClientName, true);
				ExcelHelper.Write(ws, row, colShift + 2, item.UserId, true);
				ExcelHelper.Write(ws, row, colShift + 3, item.UserName, true);
				ExcelHelper.Write(ws, row, colShift + 4, item.RegionName, true);
				ExcelHelper.Write(ws, row, colShift + 5, item.Registrant, true);
				ExcelHelper.Write(ws, row, colShift + 6, item.UpdateDate, true);
				ExcelHelper.Write(ws, row, colShift + 7, item.LastOrderDate, true);
				row++;
			}

			UpdatedAndDidNotDoOrders(ws);

			wb.Worksheets.Add(ws);
			var ms = new MemoryStream();
			wb.Save(ms);

			return ms.ToArray();
		}


		public static byte[] ExcelAnalysisOfWorkDrugstores(AnalysisOfWorkDrugstoresFilter filter)
		{
			var wb = new Workbook();
			var ws = new Worksheet("Сравнительный анализ работы аптек");

			int row = 6;
			int colShift = 0;

			ws.Merge(0, 0, 0, 6);

			ExcelHelper.WriteHeader1(ws, 0, 0, "Сравнительный анализ работы аптек", false, true);

			ws.Merge(1, 1, 1, 2);
			ExcelHelper.Write(ws, 1, 0, "Регион:", false);
			string regionName;
			if (filter.Region == null)
				regionName = "Все";
			else {
				regionName = filter.Region.Name;
			}
			ExcelHelper.Write(ws, 1, 1, regionName, false);

			ws.Merge(2, 1, 2, 2);
			ExcelHelper.Write(ws, 2, 0, "Старый период:", false);
			if (filter.FistPeriod.Begin != filter.FistPeriod.End)
				ExcelHelper.Write(ws, 2, 1, "С " + filter.FistPeriod.Begin.ToString("dd.MM.yyyy") + " по " + filter.FistPeriod.End.ToString("dd.MM.yyyy"), false);
			else
				ExcelHelper.Write(ws, 2, 1, "За " + filter.FistPeriod.Begin.ToString("dd.MM.yyyy"), false);

			ws.Merge(3, 1, 3, 2);
			ExcelHelper.Write(ws, 3, 0, "Новый период:", false);
			if (filter.LastPeriod.Begin != filter.LastPeriod.End)
				ExcelHelper.Write(ws, 3, 1, "С " + filter.LastPeriod.Begin.ToString("dd.MM.yyyy") + " по " + filter.LastPeriod.End.ToString("dd.MM.yyyy"), false);
			else
				ExcelHelper.Write(ws, 3, 1, "За " + filter.LastPeriod.Begin.ToString("dd.MM.yyyy"), false);

			var reportData = ArHelper.WithSession(s => {
				filter.Session = s;
				var result = filter.Find(true);
				foreach (var updatedAndDidNotDoOrdersField in result) {
					((AnalysisOfWorkFiled)updatedAndDidNotDoOrdersField).ForExport = true;
				}
				return result;
			});

			foreach (AnalysisOfWorkFiled item in reportData) {
				ExcelHelper.Write(ws, row, colShift + 0, item.Id, true);
				ExcelHelper.Write(ws, row, colShift + 1, item.Name, true);
				ExcelHelper.Write(ws, row, colShift + 2, item.UserCount, true);
				ExcelHelper.Write(ws, row, colShift + 3, item.AddressCount, true);
				ExcelHelper.Write(ws, row, colShift + 4, item.RegionName, true);
				ExcelHelper.Write(ws, row, colShift + 5, item.Obn, true);
				ExcelHelper.Write(ws, row, colShift + 6, item.ProblemObn, true);
				ExcelHelper.Write(ws, row, colShift + 7, item.Zak, true);
				ExcelHelper.Write(ws, row, colShift + 8, item.ProblemZak, true);
				row++;
			}

			AnalysisOfWorkDrugstoresFormat(ws);

			wb.Worksheets.Add(ws);
			var ms = new MemoryStream();
			wb.Save(ms);

			return ms.ToArray();
		}

		public static byte[] ExcelWhoWasNotUpdated(WhoWasNotUpdatedFilter filter)
		{
			var wb = new Workbook();
			var ws = new Worksheet("Кто не обновлялся с опред. даты");

			int row = 4;
			int colShift = 0;

			ws.Merge(0, 0, 0, 6);

			ExcelHelper.WriteHeader1(ws, 0, 0, "Кто не обновлялся с опред. даты", false, true);

			ws.Merge(1, 1, 1, 2);
			ExcelHelper.Write(ws, 1, 0, "Регион:", false);
			string regionName;
			if (filter.Region == null)
				regionName = "Все";
			else {
				regionName = filter.Region.Name;
			}
			ExcelHelper.Write(ws, 1, 1, regionName, false);

			ws.Merge(2, 1, 2, 2);
			ExcelHelper.Write(ws, 2, 0, "Нет обновлений с:", false);
			ExcelHelper.Write(ws, 2, 1, filter.BeginDate.ToString("dd.MM.yyyy"), false);

			var reportData = ArHelper.WithSession(s => filter.SqlQuery2(s, true)).ToList();

			foreach (var item in reportData) {
				ExcelHelper.Write(ws, row, colShift + 0, item.ClientId, true);
				ExcelHelper.Write(ws, row, colShift + 1, item.ClientName, true);
				ExcelHelper.Write(ws, row, colShift + 2, item.UserId, true);
				ExcelHelper.Write(ws, row, colShift + 3, item.UserName, true);
				ExcelHelper.Write(ws, row, colShift + 4, item.RegionName, true);
				ExcelHelper.Write(ws, row, colShift + 5, item.Registrant, true);
				ExcelHelper.Write(ws, row, colShift + 6, item.UpdateDate, true);
				row++;
			}

			FormatWhoWasNotUpdated(ws);

			wb.Worksheets.Add(ws);
			var ms = new MemoryStream();
			wb.Save(ms);

			return ms.ToArray();
		}

		public static byte[] GetCallsHistory(CallRecordFilter filter)
		{
			var wb = new Workbook();
			var ws = new Worksheet("История звонков");

			int row = 1;
			int col = 1;

			ExcelHelper.WriteHeader1(ws, 0, 1, "Дата звонка", true, true);
			ExcelHelper.WriteHeader1(ws, 0, 2, "Номер звонившего", true, true);
			ExcelHelper.WriteHeader1(ws, 0, 3, "Имя звонившего", true, true);
			ExcelHelper.WriteHeader1(ws, 0, 4, "Куда звонил", true, true);
			ExcelHelper.WriteHeader1(ws, 0, 5, "Кому звонил", true, true);
			ExcelHelper.WriteHeader1(ws, 0, 6, "Тип звонка", true, true);
			filter.PageSize = UInt16.MaxValue;
			var calls = filter.Find();
			foreach (var call in calls) {
				ExcelHelper.Write(ws, row, col, call.WriteTime, true);
				ExcelHelper.Write(ws, row, col + 1, call.From, true);
				ExcelHelper.Write(ws, row, col + 2, call.NameSource, true);
				ExcelHelper.Write(ws, row, col + 3, call.To, true);
				ExcelHelper.Write(ws, row, col + 4, call.NameDestination, true);
				ExcelHelper.Write(ws, row, col + 5, call.GetCallType(), true);
				row++;
			}
			for (ushort i = 1; i <= 6; i++) {
				ws.Cells.ColumnWidth[i] = 20 * 256;
			}
			wb.Worksheets.Add(ws);
			var buffer = new MemoryStream();
			wb.Save(buffer);
			return buffer.ToArray();
		}

		private static ICellStyle GetHeaderStyle(HSSFWorkbook book)
		{
			var font = book.CreateFont();
			font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.BOLD;
			var style = book.CreateCellStyle();
			style.BorderRight = BorderStyle.MEDIUM;
			style.BorderLeft = BorderStyle.MEDIUM;
			style.BorderBottom = BorderStyle.MEDIUM;
			style.BorderTop = BorderStyle.MEDIUM;
			style.Alignment = HorizontalAlignment.CENTER;
			style.SetFont(font);
			style.WrapText = true;
			return style;
		}

		private static ICellStyle GetDataStyle(HSSFWorkbook book, bool isCenter = false, bool isWrap = true)
		{
			var style = book.CreateCellStyle();
			style.BorderRight = BorderStyle.THIN;
			style.BorderLeft = BorderStyle.THIN;
			style.BorderBottom = BorderStyle.THIN;
			style.BorderTop = BorderStyle.THIN;
			style.GetFont(book).Boldweight = (short)FontBoldWeight.None;
			if (isWrap)
				style.WrapText = true;
			if (isCenter)
				style.Alignment = HorizontalAlignment.CENTER;
			return style;
		}

		public static byte[] GetFormPosition(FormPositionFilter filter)
		{
			var book = new HSSFWorkbook();

			var sheet = book.CreateSheet("Лист1");

			var row = 0;
			var col = 0;
			// выбираем данные
			var report = filter.Find();
			var type = typeof(FormPositionItem);
			// получаем список всех свойств
			var infos = type.GetProperties();
			// создаем строку
			var sheetRow = sheet.CreateRow(row++);
			var headerStyle = NPOIExcelHelper.GetHeaderStype(book);
			// выводим наименование отчета
			var headerCell = sheetRow.CreateCell(0);
			headerCell.CellStyle = headerStyle;
			headerCell.SetCellValue("Отчет о состоянии формализуемых полей в прайс-листах поставщиков");
			// дата построения отчета
			sheetRow = sheet.CreateRow(row++);
			var dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("Дата подготовки отчета: {0}", DateTime.Now));
			// наименование поставщика
			sheetRow = sheet.CreateRow(row++);
			dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("Наименование поставщика: {0}", filter.SupplierName));
			// регион работы прайса
			sheetRow = sheet.CreateRow(row++);
			dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("Регион работы прайса: {0}", filter.Region != null ? filter.Region.Name : "Все"));
			sheetRow = sheet.CreateRow(row++);
			dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("* В отчет не включены прайс-листы с фиксированной шириной колонки"));
			// добавляем пустую строку перед таблицей
			sheet.CreateRow(row++);
			var tableHeaderRow = row;
			sheetRow = sheet.CreateRow(row++);
			// заполняем наименования столбцов таблицы
			foreach (PropertyInfo prop in infos) {
				var cell = sheetRow.CreateCell(col++);
				cell.CellStyle = headerStyle;
				var value = ((DisplayAttribute)prop.GetCustomAttributes(typeof(DisplayAttribute), false).First()).Name;
				cell.SetCellValue(value);
			}
			// стиль для ячеек с данными
			var dataStyle = NPOIExcelHelper.GetDataStyle(book);

			// стиль для ячеек с данными, выравненный по центру
			var centerDataStyle = NPOIExcelHelper.GetCenterDataStyle(book);

			dateCell.CellStyle = dataStyle;
			// выводим данные
			foreach (var formPositionItem in report) {
				col = 0;
				sheetRow = sheet.CreateRow(row++);
				foreach (PropertyInfo prop in infos) {
					var value = prop.GetValue(formPositionItem, null);
					var cell = sheetRow.CreateCell(col++);
					if (value != null && value.ToString() == "*") {
						cell.CellStyle = centerDataStyle;
					}
					else
						cell.CellStyle = dataStyle;
					if (value != null)
						cell.SetCellValue(value.ToString());
				}
			}
			// устанавливаем ширину столбцов
			for (var i = col; i >= 0; i--) {
				sheet.SetColumnWidth(i, 2700);
			}
			sheet.SetColumnWidth(1, sheet.GetColumnWidth(1) * 2);
			sheet.SetColumnWidth(2, sheet.GetColumnWidth(2) * 2);
			sheet.SetColumnWidth(4, sheet.GetColumnWidth(4) * 2);

			// добавляем автофильтр
			sheet.SetAutoFilter(new CellRangeAddress(tableHeaderRow, row, 0, col - 1));

			sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, col - 1));

			var buffer = new MemoryStream();
			book.Write(buffer);
			return buffer.ToArray();
		}

		public static byte[] GetClientConditionsMonitoring(ClientConditionsMonitoringFilter filter)
		{
			var book = new HSSFWorkbook();

			var sheet = book.CreateSheet("Мониторинг выставления условий клиенту");

			var row = 0;
			var col = 0;
			// выбираем данные
			var report = filter.Find();
			// создаем строку
			var sheetRow = sheet.CreateRow(row++);
			var headerStyle = NPOIExcelHelper.GetHeaderStype(book);
			// выводим наименование отчета
			var headerCell = sheetRow.CreateCell(0);
			headerCell.CellStyle = headerStyle;
			headerCell.SetCellValue("Мониторинг выставления условий клиенту");
			// дата построения отчета
			sheetRow = sheet.CreateRow(row++);
			var dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("Дата подготовки отчета: {0}", DateTime.Now));
			// наименование аптеки
			sheetRow = sheet.CreateRow(row++);
			dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("Наименование аптеки: {0}", filter.ClientName));

			// добавляем пустую строку перед таблицей
			sheet.CreateRow(row++);
			sheetRow = sheet.CreateRow(row++);
			// заполняем наименования столбцов таблицы
			NPOIExcelHelper.FillNewCell(sheetRow, 0, "Код поставщика", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 1, "Наименование поставщика", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 2, "Регион", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 3, "Прайс лист", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 4, "Тип", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 5, "Ценовая колонка", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 6, "В работе", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 7, "Подключен к прайсам", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 8, "Наценка", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 9, "Код клиента", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 10, "Код доставки", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 11, "Код оплаты", headerStyle);
			// стиль для ячеек с данными
			var dataStyle = NPOIExcelHelper.GetDataStyle(book);

			// стиль для ячеек с данными, выравненный по центру
			var centerDataStyle = NPOIExcelHelper.GetCenterDataStyle(book);

			ICellStyle LIGHT_ORANGE_STYLE;
			ICellStyle LIGHT_YELLOW_STYLE;
			ICellStyle LIGHT_BLUE_STYLE;
			ICellStyle PINK_STYLE;
			ICellStyle LIGHT_GREEN_STYLE;
			ICellStyle LAVENDER_STYLE;

			dateCell.CellStyle = dataStyle;
			// выводим данные
			foreach (var monitoringItem in report) {
				sheetRow = sheet.CreateRow(row++);

				NPOIExcelHelper.FillNewCell(sheetRow, 0, monitoringItem.SupplierCode.ToString(), centerDataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 1, monitoringItem.SupplierName, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 2, monitoringItem.RegionName, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 3, monitoringItem.PriceName, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 4, monitoringItem.PriceType, dataStyle);

				if (monitoringItem.CostCollumn) {
					LIGHT_ORANGE_STYLE = NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.LIGHT_ORANGE.index);
					NPOIExcelHelper.FillNewCell(sheetRow, 5, monitoringItem.CostName, LIGHT_ORANGE_STYLE);
				}
				else {
					NPOIExcelHelper.FillNewCell(sheetRow, 5, monitoringItem.CostName, centerDataStyle);
				}

				NPOIExcelHelper.FillNewCell(sheetRow, 6, !monitoringItem.AvailableForClient ? "Нет" : string.Empty, centerDataStyle);

				if (monitoringItem.NoPriceConnected) {
					LIGHT_YELLOW_STYLE = NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.LIGHT_YELLOW.index);
					NPOIExcelHelper.FillNewCell(sheetRow, 7, monitoringItem.CountAvailableForClient, LIGHT_YELLOW_STYLE);
				}
				else {
					NPOIExcelHelper.FillNewCell(sheetRow, 7, monitoringItem.CountAvailableForClient, centerDataStyle);
				}

				if (monitoringItem.PriceMarkupStyle) {
					LIGHT_BLUE_STYLE = NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.LIGHT_BLUE.index);
					NPOIExcelHelper.FillNewCell(sheetRow, 8, monitoringItem.PriceMarkup > 0 ? monitoringItem.PriceMarkup.ToString() : string.Empty, LIGHT_BLUE_STYLE);
				}
				else {
					NPOIExcelHelper.FillNewCell(sheetRow, 8, monitoringItem.PriceMarkup > 0 ? monitoringItem.PriceMarkup.ToString() : string.Empty, centerDataStyle);
				}

				if (monitoringItem.ClientCodeStyle) {
					PINK_STYLE = NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.PINK.index);
					NPOIExcelHelper.FillNewCell(sheetRow, 9, monitoringItem.SupplierClientId, PINK_STYLE);
				}
				else {
					NPOIExcelHelper.FillNewCell(sheetRow, 9, monitoringItem.SupplierClientId, centerDataStyle);
				}

				if (monitoringItem.DeliveryStyle) {
					LIGHT_GREEN_STYLE = NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.LIGHT_GREEN.index);
					NPOIExcelHelper.FillNewCell(sheetRow, 10, monitoringItem.DeliveryStyle ? monitoringItem.SupplierDeliveryAdresses.Replace("</br>", Environment.NewLine) : string.Empty, LIGHT_GREEN_STYLE);
				}
				else {
					NPOIExcelHelper.FillNewCell(sheetRow, 10, monitoringItem.DeliveryStyle ? monitoringItem.SupplierDeliveryAdresses.Replace("</br>", Environment.NewLine) : string.Empty, centerDataStyle);
				}

				if (!string.IsNullOrEmpty(monitoringItem.SupplierDeliveryAdresses)) {
					var rows = monitoringItem.SupplierDeliveryAdresses.Split(new[] { "</br>" }, StringSplitOptions.RemoveEmptyEntries);
					sheetRow.Height = (short)(sheetRow.Height * rows.Count());
				}

				if (monitoringItem.PaymentCodeStyle) {
					LAVENDER_STYLE = NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.LAVENDER.index);
					NPOIExcelHelper.FillNewCell(sheetRow, 11, monitoringItem.SupplierPaymentId, LAVENDER_STYLE);
				}
				else {
					NPOIExcelHelper.FillNewCell(sheetRow, 11, monitoringItem.SupplierPaymentId, centerDataStyle);
				}
			}
			// устанавливаем ширину столбцов
			for (var i = col; i >= 0; i--) {
				sheet.SetColumnWidth(i, 3500);
			}
			sheet.SetColumnWidth(0, sheet.GetColumnWidth(1) * 4);
			sheet.SetColumnWidth(1, sheet.GetColumnWidth(2) * 4);
			sheet.SetColumnWidth(3, sheet.GetColumnWidth(3) * 2);
			sheet.SetColumnWidth(5, sheet.GetColumnWidth(5) * 4);
			sheet.SetColumnWidth(9, sheet.GetColumnWidth(9) * 2);
			sheet.SetColumnWidth(10, sheet.GetColumnWidth(10) * 6);
			sheet.SetColumnWidth(11, sheet.GetColumnWidth(11) * 4);


			sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 11));
			var dataRowCount = row;

			for (int i = 1; i < 7; i++) {
				sheet.AddMergedRegion(new CellRangeAddress(dataRowCount + i, dataRowCount + i, 1, 11));
			}

			dataStyle = book.CreateCellStyle();
			dataStyle.GetFont(book).Boldweight = (short)FontBoldWeight.None;

			sheet.CreateRow(row++);
			sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, string.Empty, NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.LIGHT_ORANGE.index));
			NPOIExcelHelper.FillNewCell(sheetRow, 1, "- Если у поставщика менее 30% клиентов подключены к базовой ценовой колонке и данной аптеке назначена базовая", dataStyle);
			sheetRow = sheet.CreateRow(row++);

			NPOIExcelHelper.FillNewCell(sheetRow, 0, string.Empty, NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.LIGHT_YELLOW.index));
			NPOIExcelHelper.FillNewCell(sheetRow, 1, "- Не подключен ни к одному из проайсов", dataStyle);
			sheetRow = sheet.CreateRow(row++);

			NPOIExcelHelper.FillNewCell(sheetRow, 0, string.Empty, NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.LIGHT_BLUE.index));
			NPOIExcelHelper.FillNewCell(sheetRow, 1, "- Более половины аптек, подключенных к прайсу имеют наценку, отличную от Нуля, а для рассматриваемой аптеки наценка 0", dataStyle);
			sheetRow = sheet.CreateRow(row++);

			NPOIExcelHelper.FillNewCell(sheetRow, 0, string.Empty, NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.PINK.index));
			NPOIExcelHelper.FillNewCell(sheetRow, 1, "- Более половины аптек, подключенных к прайсу имеют код клиента, а для рассматриваемой аптеки этот код не прописан", dataStyle);
			sheetRow = sheet.CreateRow(row++);

			NPOIExcelHelper.FillNewCell(sheetRow, 0, string.Empty, NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.LIGHT_GREEN.index));
			NPOIExcelHelper.FillNewCell(sheetRow, 1, "- Более половины аптек, подключенных к прайсу имеют коды адресов доставки, а для рассматриваемой аптеки этот код не прописан, либо для рассмастриваемой аптеки прописаны не все коды адресов доставки", dataStyle);
			sheetRow = sheet.CreateRow(row++);

			NPOIExcelHelper.FillNewCell(sheetRow, 0, string.Empty, NPOIExcelHelper.GetCenterDataStyle(book, HSSFColor.LAVENDER.index));
			NPOIExcelHelper.FillNewCell(sheetRow, 1, "- Более половины аптек, подключенных к прайсу имеют код оплаты, а для рассматриваемой аптеки этот код не прописан", dataStyle);

			var buffer = new MemoryStream();
			book.Write(buffer);
			return buffer.ToArray();
		}

		public static byte[] GetNotParcedWaybills(DocumentFilter filter)
		{
			var book = new HSSFWorkbook();

			var sheet = book.CreateSheet("Отчет о состоянии неформализованных накладных");

			var row = 0;
			var col = 0;

			// создаем строку
			var sheetRow = sheet.CreateRow(row++);

			// стиль для заголовков
			var headerStyle = ExportModel.GetHeaderStyle(book);

			// выводим наименование отчета
			var headerCell = sheetRow.CreateCell(0);
			headerCell.CellStyle = headerStyle;
			headerCell.SetCellValue("Отчет о состоянии неформализованных накладных");
			// дата построения отчета
			sheetRow = sheet.CreateRow(row++);
			var dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("Дата подготовки отчета: {0}", DateTime.Now));
			// период построения отчета
			sheetRow = sheet.CreateRow(row++);
			dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("Период: с {0} по {1}",
				filter.Period.Begin.ToString("dd.MM.yyyy"),
				filter.Period.End.ToString("dd.MM.yyyy")));
			// добавляем пустую строку перед таблицей
			sheet.CreateRow(row++);
			var tableHeaderRow = row;
			var type = typeof(NotParcedStat);
			// формируем заголовки таблицы
			row = CreateTableHeader(sheet, type, headerStyle, row);
			var dataStyle = GetDataStyle(book);
			var centerDataStyle = GetDataStyle(book, true);
			// формируем данные таблицы
			row = CreateTableData(sheet, type, dataStyle, centerDataStyle, row, filter.FindStat);

			// устанавливаем ширину столбцов
			sheet.SetColumnWidth(0, sheet.GetColumnWidth(0) * 2);
			sheet.SetColumnWidth(1, sheet.GetColumnWidth(1) * 3);
			sheet.SetColumnWidth(2, sheet.GetColumnWidth(2) * 2);
			sheet.SetColumnWidth(3, sheet.GetColumnWidth(3) * 2);
			sheet.SetColumnWidth(4, sheet.GetColumnWidth(4) * 10);

			// добавляем автофильтр
			sheet.SetAutoFilter(new CellRangeAddress(tableHeaderRow, row, 0, 4));

			sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

			var buffer = new MemoryStream();
			book.Write(buffer);
			return buffer.ToArray();
		}

		private static int CreateTableData(ISheet sheet, Type type, ICellStyle dataStyle, ICellStyle centerDataStyle, int row, Func<IEnumerable> func)
		{
			var report = func();
			// получаем список всех свойств
			var infos = type.GetProperties()
				.OrderBy(p => ((DisplayAttribute)p.GetCustomAttributes(typeof(DisplayAttribute), false).First()).Order);
			// выводим данные
			foreach (var formPositionItem in report) {
				var col = 0;
				var sheetRow = sheet.CreateRow(row++);
				foreach (PropertyInfo prop in infos) {
					var value = prop.GetValue(formPositionItem, null);
					var cell = sheetRow.CreateCell(col++);
					if (value != null && value.ToString() == "*") {
						cell.CellStyle = centerDataStyle;
					}
					else
						cell.CellStyle = dataStyle;
					if (value != null)
						cell.SetCellValue(value.ToString());
				}
			}
			return row;
		}

		private static int CreateTableHeader(ISheet sheet, Type type, ICellStyle style, int row)
		{
			var col = 0;
			// получаем список всех свойств
			var infos = type.GetProperties()
				.OrderBy(p => ((DisplayAttribute)p.GetCustomAttributes(typeof(DisplayAttribute), false).First()).Order);

			var sheetRow = sheet.CreateRow(row++);
			// заполняем наименования столбцов таблицы
			foreach (PropertyInfo prop in infos) {
				var cell = sheetRow.CreateCell(col++);
				cell.CellStyle = style;
				var value = ((DisplayAttribute)prop.GetCustomAttributes(typeof(DisplayAttribute), false).First()).Name;
				cell.SetCellValue(value);
			}
			return row;
		}

		public static byte[] GetParcedWaybills(ParsedWaybillsFilter filter)
		{
			var book = new HSSFWorkbook();

			var sheet = book.CreateSheet("Отчет о состоянии формализованных накладных");

			var row = 0;

			// создаем строку
			var sheetRow = sheet.CreateRow(row++);

			// стиль для заголовков
			var headerStyle = GetHeaderStyle(book);

			// выводим наименование отчета
			var headerCell = sheetRow.CreateCell(0);
			headerCell.CellStyle = headerStyle;
			headerCell.SetCellValue("Отчет о состоянии формализованных накладных");
			// дата построения отчета
			sheetRow = sheet.CreateRow(row++);
			var dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("Дата подготовки отчета: {0}", DateTime.Now));
			// период построения отчета
			sheetRow = sheet.CreateRow(row++);
			dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("Период: с {0} по {1}",
				filter.Period.Begin.ToString("dd.MM.yyyy"),
				filter.Period.End.ToString("dd.MM.yyyy")));
			// клиент
			sheetRow = sheet.CreateRow(row++);
			dateCell = sheetRow.CreateCell(0);
			dateCell.SetCellValue(String.Format("Клиент: {0}",
				filter.ClientName));
			// добавляем пустую строку перед таблицей
			sheet.CreateRow(row++);
			var tableHeaderRow = row;
			var type = typeof(ParsedWaybillsItem);
			// формируем заголовки таблицы
			row = CreateTableHeader(sheet, type, headerStyle, row);
			var dataStyle = GetDataStyle(book);
			var centerDataStyle = GetDataStyle(book, true);
			// формируем данные таблицы
			row = CreateTableData(sheet, type, dataStyle, centerDataStyle, row, filter.Find);

			// устанавливаем ширину столбцов
			for (int i = 0; i < type.GetProperties().Length; i++) {
				sheet.SetColumnWidth(i, (sheet.GetColumnWidth(i) * 3) / 2);
			}

			// добавляем автофильтр
			sheet.SetAutoFilter(new CellRangeAddress(tableHeaderRow, row, 0, type.GetProperties().Length - 1));

			sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 6));

			var buffer = new MemoryStream();
			book.Write(buffer);
			return buffer.ToArray();
		}

		public static byte[] GetClientAddressesMonitor(ClientAddressFilter filter)
		{
			var book = new HSSFWorkbook();

			var sheet = book.CreateSheet("Клиенты и адреса в регионе, по которым не принимаются накладные");

			var row = 0;
			var headerStyle = NPOIExcelHelper.GetHeaderStype(book);
			var dataStyle = NPOIExcelHelper.GetDataStyle(book);
			var sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, "Клиенты и адреса в регионе, по которым не принимаются накладные", headerStyle);
			sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, String.Format("Период: с {0} по {1}",
				filter.Period.Begin.ToString("dd.MM.yyyy"),
				filter.Period.End.ToString("dd.MM.yyyy")),
				book.CreateCellStyle());
			sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, String.Format("Дата подготовки отчета: {0}", DateTime.Now), book.CreateCellStyle());
			sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, String.Format("Регион: {0}", filter.Region == null ? "Все" : filter.Region.Name), book.CreateCellStyle());
			sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, String.Format("Клиент: {0}", filter.ClientText), book.CreateCellStyle());
			sheet.CreateRow(row++);
			var tableHeaderRow = row;
			sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, "Код клиента", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 1, "Наименование клиента", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 2, "Регион", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 3, "Код адреса", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 4, "Адрес", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 5, "Код поставщика", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 6, "Наименование поставщика", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 7, "Количество непринятых накладных", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 8, "Причина", headerStyle);

			var items = filter.Find(true);
			foreach (var item in items) {
				sheetRow = sheet.CreateRow(row++);
				NPOIExcelHelper.FillNewCell(sheetRow, 0, item.ClientId.ToString(), dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 1, item.ClientName, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 2, item.RegionName, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 3, item.AddressId.ToString(), dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 4, item.AddressName, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 5, item.SupplierId.ToString(), dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 6, item.SupplierName, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 7, item.Count.ToString(), dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 8, item.RejectReasonName, dataStyle);
			}

			// добавляем автофильтр
			sheet.SetAutoFilter(new CellRangeAddress(tableHeaderRow, row, 0, 8));

			sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

			// устанавливаем ширину столбцов
			for (int i = 0; i < 9; i++) {
				sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) * 2);
			}

			sheet.SetColumnWidth(4, sheet.GetColumnWidth(4) * 3);
			sheet.SetColumnWidth(8, sheet.GetColumnWidth(8) * 3);

			var buffer = new MemoryStream();
			book.Write(buffer);
			return buffer.ToArray();
		}

		public static byte[] DocumentsLog(DocumentFilter filter)
		{
			var book = new HSSFWorkbook();
			var sheet = book.CreateSheet("Неразобранные накладные");
			var row = 0;
			var headerStyle = NPOIExcelHelper.GetHeaderStype(book);
			var dataStyle = NPOIExcelHelper.GetDataStyle(book);
			var sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, "Неразобранные накладные", headerStyle);
			sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, String.Format("Период: с {0} по {1}",
				filter.Period.Begin.ToString("dd.MM.yyyy"),
				filter.Period.End.ToString("dd.MM.yyyy")),
				book.CreateCellStyle());
			sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, String.Format("Дата подготовки отчета: {0}", DateTime.Now), book.CreateCellStyle());
			sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, String.Format("Регион: {0}", filter.Region == null ? "Все" : filter.Region.Name), book.CreateCellStyle());
			sheet.CreateRow(row++);
			var tableHeaderRow = row;
			sheetRow = sheet.CreateRow(row++);
			NPOIExcelHelper.FillNewCell(sheetRow, 0, "Номер документа", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 1, "Дата получения", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 2, "Тип документа", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 3, "От поставщика", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 4, "Клиенту", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 5, "На адрес", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 6, "Название файла", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 7, "Размер", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 8, "Парсер", headerStyle);
			NPOIExcelHelper.FillNewCell(sheetRow, 9, "Комментарий", headerStyle);

			var items = filter.Find(true);
			foreach (var item in items) {
				sheetRow = sheet.CreateRow(row++);
				NPOIExcelHelper.FillNewCell(sheetRow, 0, item.Id.ToString(), dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 1, item.LogTime.ToString(), dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 2, item.DocumentType.GetDescription(), dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 3, item.Supplier, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 4, item.Client, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 5, item.Address, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 6, item.FileName, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 7,
					item.DocumentSize == null ? ""
						: ViewHelper.ConvertToUserFriendlySize(item.DocumentSize.Value),
					dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 8, item.Parser, dataStyle);
				NPOIExcelHelper.FillNewCell(sheetRow, 9, item.Addition, dataStyle);
			}

			// добавляем автофильтр
			sheet.SetAutoFilter(new CellRangeAddress(tableHeaderRow, row, 0, 9));

			sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 9));

			// устанавливаем ширину столбцов
			for (int i = 0; i < 10; i++) {
				sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) * 2);
			}

			sheet.SetColumnWidth(5, sheet.GetColumnWidth(5) * 2);

			var buffer = new MemoryStream();
			book.Write(buffer);
			return buffer.ToArray();
		}
	}
}
