using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminInterface.ViewModels
{
	public class TablePaginator
	{
		public int PageSize {
			get
			{
				return PageSizes.Length > 0 && PageSizes.Length > PageSizeCurrentIndex ? PageSizes[PageSizeCurrentIndex] : 50;
			}
		}
		public int CurrentPage { get; set; }
		public int TotalItems { get; set; }
		public int[] PageSizes { get; set; }
		public int PageSizeCurrentIndex { get; set; }
	}

	public class TableHead
	{
		public string[] Headers { get; set; }
		public int SortOrder { get; set; }
	}
}