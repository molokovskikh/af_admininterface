using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models.Telephony;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using ExcelLibrary.SpreadSheet;

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
	}
}
