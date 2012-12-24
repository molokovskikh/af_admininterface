﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Web;
using AdminInterface.Controllers;
using AdminInterface.Controllers.Filters;
using AdminInterface.Helpers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models.Telephony;
using AdminInterface.Queries;
using Castle.Components.DictionaryAdapter;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using ExcelLibrary.BinaryFileFormat;
using ExcelLibrary.CompoundDocumentFormat;
using ExcelLibrary.Office.Excel.BinaryFileFormat.Records;
using ExcelLibrary.SpreadSheet;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using BorderStyle = NPOI.SS.UserModel.BorderStyle;
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

		private static ICellStyle GetDataStyle(HSSFWorkbook book, bool isCenter = false)
		{
			var style = book.CreateCellStyle();
			style.BorderRight = BorderStyle.THIN;
			style.BorderLeft = BorderStyle.THIN;
			style.BorderBottom = BorderStyle.THIN;
			style.BorderTop = BorderStyle.THIN;
			style.GetFont(book).Boldweight = (short)FontBoldWeight.None;
			style.WrapText = true;
			if(isCenter)
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
			// шрифт для заголовков (жирный)
			var font = book.CreateFont();
			font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.BOLD;
			// стиль для заголовков
			var headerStyle = book.CreateCellStyle();
			headerStyle.BorderRight = BorderStyle.MEDIUM;
			headerStyle.BorderLeft = BorderStyle.MEDIUM;
			headerStyle.BorderBottom = BorderStyle.MEDIUM;
			headerStyle.BorderTop = BorderStyle.MEDIUM;
			headerStyle.Alignment = HorizontalAlignment.CENTER;
			headerStyle.SetFont(font);
			headerStyle.WrapText = true;
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
			var dataStyle = book.CreateCellStyle();
			dataStyle.BorderRight = BorderStyle.THIN;
			dataStyle.BorderLeft = BorderStyle.THIN;
			dataStyle.BorderBottom = BorderStyle.THIN;
			dataStyle.BorderTop = BorderStyle.THIN;
			dataStyle.GetFont(book).Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.None;

			// стиль для ячеек с данными, выравненный по центру
			var centerDataStyle = book.CreateCellStyle();
			centerDataStyle.BorderRight = BorderStyle.THIN;
			centerDataStyle.BorderLeft = BorderStyle.THIN;
			centerDataStyle.BorderBottom = BorderStyle.THIN;
			centerDataStyle.BorderTop = BorderStyle.THIN;
			centerDataStyle.Alignment = HorizontalAlignment.CENTER;
			centerDataStyle.GetFont(book).Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.None;

			dateCell.CellStyle = dataStyle;
			// выводим данные
			foreach (var formPositionItem in report) {
				col = 0;
				sheetRow = sheet.CreateRow(row++);
				foreach (PropertyInfo prop in infos) {
					var value = prop.GetValue(formPositionItem, null);
					var cell = sheetRow.CreateCell(col++);
					if(value != null && value.ToString() == "*") {
						cell.CellStyle = centerDataStyle;
					}
					else
						cell.CellStyle = dataStyle;
					if(value != null)
						cell.SetCellValue(value.ToString());
				}
			}
			// устанавливаем ширину столбцов
			for(var i = col; i >= 0; i--) {
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

		public static byte[] GetNotParcedWaybills(DocumentFilter filter)
		{
			var book = new HSSFWorkbook();

			var sheet = book.CreateSheet("Лист1");

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
				filter.Period.Begin.ToString("hh.MM.yyyy"),
				filter.Period.End.ToString("hh.MM.yyyy")));
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
					if(value != null && value.ToString() == "*") {
						cell.CellStyle = centerDataStyle;
					}
					else
						cell.CellStyle = dataStyle;
					if(value != null)
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
	}
}
