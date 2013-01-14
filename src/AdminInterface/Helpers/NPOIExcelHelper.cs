using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;

namespace AdminInterface.Helpers
{
	public class NPOIExcelHelper
	{
		// стиль для заголовков
		public static ICellStyle GetHeaderStype(HSSFWorkbook book)
		{
			// шрифт для заголовков (жирный)
			var font = book.CreateFont();
			font.Boldweight = (short)FontBoldWeight.BOLD;

			var headerStyle = book.CreateCellStyle();
			headerStyle.BorderRight = BorderStyle.MEDIUM;
			headerStyle.BorderLeft = BorderStyle.MEDIUM;
			headerStyle.BorderBottom = BorderStyle.MEDIUM;
			headerStyle.BorderTop = BorderStyle.MEDIUM;
			headerStyle.Alignment = HorizontalAlignment.CENTER;
			headerStyle.SetFont(font);
			headerStyle.WrapText = true;
			return headerStyle;
		}

		// стиль для ячеек с данными
		public static ICellStyle GetDataStyle(HSSFWorkbook book, short colorIndex = 64)
		{
			var dataStyle = book.CreateCellStyle();
			dataStyle.BorderRight = BorderStyle.THIN;
			dataStyle.BorderLeft = BorderStyle.THIN;
			dataStyle.BorderBottom = BorderStyle.THIN;
			dataStyle.BorderTop = BorderStyle.THIN;
			dataStyle.GetFont(book).Boldweight = (short)FontBoldWeight.None;
			dataStyle.FillForegroundColor = colorIndex;
			if (colorIndex != 64)
				dataStyle.FillPattern = FillPatternType.SOLID_FOREGROUND;
			dataStyle.FillBackgroundColor = colorIndex;
			return dataStyle;
		}

		public static ICellStyle GetCenterDataStyle(HSSFWorkbook book, short colorIndex = 64)
		{
			var centerDataStyle = book.CreateCellStyle();
			centerDataStyle.BorderRight = BorderStyle.THIN;
			centerDataStyle.BorderLeft = BorderStyle.THIN;
			centerDataStyle.BorderBottom = BorderStyle.THIN;
			centerDataStyle.BorderTop = BorderStyle.THIN;
			centerDataStyle.Alignment = HorizontalAlignment.CENTER;
			centerDataStyle.FillForegroundColor = colorIndex;
			centerDataStyle.WrapText = true;
			if (colorIndex != 64)
				centerDataStyle.FillPattern = FillPatternType.SOLID_FOREGROUND;
			centerDataStyle.FillBackgroundColor = colorIndex;
			centerDataStyle.GetFont(book).Boldweight = (short)FontBoldWeight.None;
			return centerDataStyle;
		}

		public static ICell FillNewCell(IRow row, int collumnIndex, string value, ICellStyle style)
		{
			var cell = row.CreateCell(collumnIndex);
			cell.CellStyle = style;
			cell.SetCellValue(value);
			return cell;
		}
	}
}