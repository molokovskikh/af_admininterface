using System;
using System.IO;
using AdminInterface.Controllers;
using Castle.MonoRail.Framework;
using ExcelLibrary.SpreadSheet;

namespace AdminInterface.Models.Billing
{
	public class Merge
	{
		public Merge(int count)
		{
			Count = count;
		}

		public int Count { get; set; }
	}

	public class Exporter
	{
		private int begin;
		private Worksheet _worksheet;

		public Row Row(params object[] values)
		{
			CellStyle rowStyle = null;
			var cellIndex = 0;
			foreach (var value in values) {
				if (value is Merge) {
					var merge = ((Merge)value);
					var count = merge.Count - 1;
					for (var i = 0; i < count; i++)
						_worksheet.Cells[begin, cellIndex + i] = new Cell("") { Style = rowStyle };
					_worksheet.Merge(begin, cellIndex - 1, begin, cellIndex - 1 + count);
					cellIndex += count;
				}
				else if (value is CellStyle) {
					if (cellIndex > 0)
						_worksheet.Cells[begin, cellIndex - 1].Style = (CellStyle)value;
					else
						rowStyle = (CellStyle)value;
				}
				else {
					_worksheet.Cells[begin, cellIndex] = new Cell(value) { Style = rowStyle };
					cellIndex++;
				}
			}
			begin++;
			return _worksheet.Cells.Rows[begin - 1];
		}

		public void Skip(int count = 1)
		{
			begin += count;
		}

		public static Worksheet Export(RevisionAct act)
		{
			return new Exporter().DoExport(act);
		}

		public Worksheet DoExport(RevisionAct act)
		{
			_worksheet = new Worksheet("Акт сверки");
			_worksheet.Cells.ColumnWidth[0] = (ushort)2.5 * 256;
			_worksheet.Cells.ColumnWidth[1] = (ushort)23 * 256;
			_worksheet.Cells.ColumnWidth[2] = (ushort)7 * 256;
			_worksheet.Cells.ColumnWidth[3] = (ushort)7 * 256;
			_worksheet.Cells.ColumnWidth[4] = (ushort)2.5 * 256;
			_worksheet.Cells.ColumnWidth[5] = (ushort)23 * 256;
			_worksheet.Cells.ColumnWidth[6] = (ushort)7 * 256;
			_worksheet.Cells.ColumnWidth[7] = (ushort)7 * 256;
			var h1 = new CellStyle {
				Font = new Font("Arial", 11) {
					Bold = true
				},
				HorizontalAlignment = HorizontalAlignment.Center
			};
			var h2 = new CellStyle {
				Font = new Font("Arial", 10) {
					Bold = true,
				},
				Warp = true,
			};

			var mark = new CellStyle {
				HorizontalAlignment = HorizontalAlignment.Center,
				Warp = true,
			};

			var longWord = new CellStyle {
				Warp = true
			};

			var underScrore = new CellStyle {
				Borders = new Borders { Bottom = BorderStyle.Thin }
			};

			var table = new CellStyle {
				Font = new Font("Arial", 8),
				Borders = Borders.Box(BorderStyle.Thin),
			};

			Row(h1, "Акт сверки", new Merge(8));
			var row = Row(mark,
				String.Format("взаимных расчетов по состоянию на {0} между {1} и {2} по договору № {3}",
					act.EndDate.ToShortDateString(),
					act.Payer.Recipient.FullName,
					act.Payer.JuridicalName,
					act.Payer.Id),
				new Merge(8));
			row.Height = 1440 / 72 * 43;
			row = Row(longWord,
				String.Format("Мы, нижеподписавшиеся, __________________ {0} _________________________,"
					+ "с одной стороны, и ________________________ {1} __________________________, "
					+ "с другой стороны составили настоящий акт сверки в том, что состояние взаимных расчетов по данным учета следующее:",
					act.Payer.Recipient.FullName,
					act.Payer.JuridicalName),
				new Merge(8));
			row.Height = 1440 / 72 * 62;
			Row(table,
				String.Format("По данным {0}, руб.", act.Payer.Recipient.FullName), new Merge(4),
				String.Format("По данным {0}, руб.", act.Payer.JuridicalName), new Merge(4));
			Row(table,
				"№ п/п", "Наименование операции, документы", "Дебет", "Кредит",
				"№ п/п", "Наименование операции, документы", "Дебет", "Кредит");
			var index = 1;
			foreach (var move in act.Movements) {
				Row(table,
					index, move.Name, move.Debit.ToString("#.#"), move.Credit.ToString("#.#"),
					"", "", "", "");
				index++;
			}
			Skip();
			Row(String.Format("По данным {0}", act.Payer.Recipient.FullName), new Merge(4));
			row = Row(h2, act.Result, new Merge(4));
			row.Height = 1440 / 72 * 29;
			Skip();
			Row(String.Format("От {0}", act.Payer.Recipient.FullName), new Merge(4),
				String.Format("От {0}", act.Payer.JuridicalName), new Merge(4));
			Skip(2);
			Row("", underScrore, "", underScrore, String.Format("({0})", act.Payer.Recipient.Boss), "",
				"", underScrore, "", underScrore, "", "");
			Skip();
			Row("М.П.", "", "", "",
				"М.П.", "", "", "");
			_worksheet.Cells[0, 0] = new Cell("Акт сверки") { Style = h1 };
			return _worksheet;
		}

		public static void ToResponse(IResponse response, Worksheet worksheet)
		{
			response.Clear();
			response.AppendHeader("Content-Disposition",
				String.Format("attachment; filename=\"{0}\"", Uri.EscapeDataString(String.Format("{0}.xls", worksheet.Name))));
			response.ContentType = "application/vnd.ms-excel";

			using (var stream = new MemoryStream()) {
				var book = new Workbook();
				book.Worksheets.Add(worksheet);
				book.Save(stream);
				stream.Position = 0;
				stream.CopyTo(response.OutputStream);
			}
		}
	}
}