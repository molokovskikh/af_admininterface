using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using ExcelLibrary.SpreadSheet;
using MySql.Data.Types;

namespace AdminInterface.Helpers
{
public static class ExcelHelper
	{
		private static CellStyle boldCell = null;
		private static CellStyle BoldCell
		{
			get
			{
				if (boldCell == null)
				{
					boldCell = new CellStyle();
					boldCell.Font = new Font("Arial", 11);
					boldCell.Font.Bold = true;
					boldCell.Borders = new Borders(){
						Bottom = BorderStyle.Thin,
						Left = BorderStyle.Thin,
						Right = BorderStyle.Thin,
						Top = BorderStyle.Thin};
				}
				return boldCell;
			}
		}

		private static CellStyle crossBoldCell = null;
		private static CellStyle CrossBoldCell
		{
			get
			{
				if (crossBoldCell == null)
				{
					crossBoldCell = new CellStyle();
					crossBoldCell.Font = new Font("Arial", 11);
					crossBoldCell.Font.Bold = true;
					crossBoldCell.Borders = new Borders()
					{
						Bottom = BorderStyle.Thin,
						Left = BorderStyle.Thin,
						Right = BorderStyle.Thin,
						Top = BorderStyle.Thin
					};
				}
				return crossBoldCell;
			}
		}

		private static CellStyle crossCell = null;
		private static CellStyle CrossCell
		{
			get
			{
				if (crossCell == null)
				{
					crossCell = new CellStyle();
					crossCell.Borders = new Borders()
					{
						Bottom = BorderStyle.Thin,
						Left = BorderStyle.Thin,
						Right = BorderStyle.Thin,
						Top = BorderStyle.Thin
					};
				}
				return crossCell;
			}
		}

		private static CellStyle headerCell = null;
		private static CellStyle HeaderCell
		{
			get
			{
				if (headerCell == null)
				{
					headerCell = new CellStyle();
					headerCell.Font = new Font("Arial", 11);
					headerCell.Font.Bold = true;
					headerCell.HorizontalAlignment = HorizontalAlignment.Center;
					headerCell.VerticalAlignment = VerticalAlignment.Center;
					headerCell.Warp = true;
				}
				return headerCell;
			}
		}

		private static CellStyle headerCrossCell = null;
		private static CellStyle HeaderCrossCell
		{
			get
			{
				if (headerCrossCell == null)
				{
					headerCrossCell = new CellStyle();
					headerCrossCell.Font = new Font("Arial", 11);
					headerCrossCell.Font.Bold = true;
					headerCrossCell.HorizontalAlignment = HorizontalAlignment.Center;
					headerCrossCell.VerticalAlignment = VerticalAlignment.Center;
					headerCrossCell.Warp = true;
					headerCrossCell.Borders = new Borders()
					{
						Bottom = BorderStyle.Thin,
						Left = BorderStyle.Thin,
						Right = BorderStyle.Thin,
						Top = BorderStyle.Thin
					};
				}
				return headerCrossCell;
			}
		}

		private static CellFormat dateCellFormat = null;
		private static CellFormat DateCellFormat
		{
			get{
				if(dateCellFormat == null)
					dateCellFormat = new CellFormat(CellFormatType.DateTime, "dd.mm.yyyy HH:MM:SS");
				return dateCellFormat;
			}
		}

		private static void CreateCell(Worksheet ws, int row, int col, object value)
		{
			object temp = null;
			if (value is MySqlDateTime)
				temp = ((MySqlDateTime)value).GetDateTime();
			else
				temp = value;
			
			if(temp != null && temp != DBNull.Value)
				if (temp is DateTime)
				{
					ws.Cells[row, col] = new Cell(temp);
					ws.Cells[row, col].Format = DateCellFormat;
				}
				else
					ws.Cells[row, col] = new Cell(temp);
			else
				ws.Cells[row, col] = new Cell(String.Empty);
		}

		public static void Write(Worksheet ws, int row, int col, object value, bool bordered)
		{
			Write(ws, row, col, value, bordered, false);
		}

		public static void Write(Worksheet ws, int row, int col, object value, bool bordered, bool bolded)
		{
			CreateCell(ws, row, col, value);

			if (bordered)
				BorderCell(ws.Cells[row, col], bolded);
			else if (bolded)
				ws.Cells[row, col].Style = BoldCell;
		}

		public static void WriteStringFromRecord(Worksheet ws, int row, int col,
			string fieldName, DbDataRecord reader, bool bordered, bool bolded)
		{
			Object value = reader[fieldName];

			CreateCell(ws, row, col, value);
			
			if (bordered)
				BorderCell(ws.Cells[row, col], bolded);
			else if (bolded)
				ws.Cells[row, col].Style = BoldCell;
		}

		public static void WriteStringFromRecord(Worksheet ws, int row, int col,
			string fieldName, DbDataRecord reader, bool bordered)
		{
			WriteStringFromRecord(ws, row, col, fieldName, reader, bordered, false);
		}

		public static void WriteBoolFromRecord(Worksheet ws, int row, int col, string fieldName, 
			DbDataRecord reader, bool bordered)
		{
			WriteBoolFromRecord(ws, row, col, fieldName, reader, bordered, false);
		}

		public static void WriteBoolFromRecord(Worksheet ws, int row, int col, string fieldName, 
			DbDataRecord reader, bool bordered, bool bolded)
		{
			Object value = reader[fieldName];
			if (value != null && value != DBNull.Value)
				ws.Cells[row, col] = new Cell(
						(Convert.ToInt64(value) == 1) ? "Да" : "Нет");

			if (bordered)
				BorderCell(ws.Cells[row, col], bolded);
			else if (bolded)
			{
				if (ws.Cells[row, col] == null)
					ws.Cells[row, col] = new Cell(String.Empty);
				ws.Cells[row, col].Style = BoldCell;
			}
		}

		public static void WriteInvertedBoolFromRecord(Worksheet ws, int row, int col, string fieldName,
			DbDataRecord reader, bool bordered)
		{
			Object value = reader[fieldName];
			if (value != null && value != DBNull.Value)
				ws.Cells[row, col] = new Cell(
						((long)value != 1) ? "Да" : "Нет");

			if (bordered)
				BorderCell(ws.Cells[row, col]);
		}

		public static void WriteBool(Worksheet ws, int row, int col, bool value, bool bordered)
		{
			ws.Cells[row, col] = new Cell( value  ? "Да" : "Нет");

			if (bordered)
				BorderCell(ws.Cells[row, col]);
		}

		public static void WriteHeader1(Worksheet ws, int row, int col, string value, bool bordered, bool bold)
		{
			Cell cell = new Cell(value);
			ws.Cells[row, col] = cell;

			if (bordered)
				cell.Style = HeaderCrossCell;
			else
				cell.Style = HeaderCell;
		}

		public static void BorderCell(Worksheet ws, int row, int col)
		{
			var cell = new Cell(String.Empty);
			ws.Cells[row, col] = cell;

			BorderCell(cell);
		}

		public static void BorderCell(Cell cell, bool bolded)
		{
			if (cell == null)
				cell = new Cell(String.Empty);

			if (bolded)
				cell.Style = CrossBoldCell;
			else
				cell.Style = CrossCell;
		}

		public static void BorderCell(Cell cell)
		{
			BorderCell(cell, false);
		}
	}
}