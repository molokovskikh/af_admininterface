using System.Collections.Generic;
using System.Linq;
using Common.Tools;

namespace AdminInterface.ViewModels
{
	public class ReportTable<TF, TD> where TF : new()
	{
		public ReportTable()
		{
			TableData = new List<TD>();
			TableHead = new TableHead {Headers = new string[0], SortOrder = 1};
			TablePaginator = new TablePaginator() {
				CurrentPage = 0,
				TotalItems = 0,
				PageSizes = new[] {50, 100, 150, 200, 250, 400},
				PageSizeCurrentIndex = 3
			};
			TableFilter = new TF();
		}

		public ReportTable(List<TD> tableData, int totalItems = 0, int currentPage = 0, int currentPageSize = 3,
			string[] headers = null, int sortOrder = 1, int[] pageSizes = null)
		{
			TableData = tableData;
			TableHead = new TableHead {Headers = headers ?? new string[0], SortOrder = sortOrder};
			TablePaginator = new TablePaginator() {
				CurrentPage = currentPage,
				TotalItems = totalItems,
				PageSizes = pageSizes ?? new[] {50, 100, 150, 200, 250, 400 },
				PageSizeCurrentIndex = currentPageSize
			};
			TableFilter = new TF();
		}

		public TableHead TableHead { get; set; }
		public TablePaginator TablePaginator { get; set; }
		public TF TableFilter { get; set; }
		public List<TD> TableData { get; set; }


		public int CurrentPage
		{
			get { return TablePaginator.CurrentPage; }
			set { TablePaginator.CurrentPage = value; }
		}

		public int PageSize
		{
			get { return TablePaginator.PageSizeCurrentIndex; }
			set { TablePaginator.PageSizeCurrentIndex = value; }
		}

		public int SortOrder
		{
			get { return TableHead.SortOrder; }
			set { TableHead.SortOrder = value; }
		}

		public int GetColumsNumber(int additionalColumns = 0)
		{
			return additionalColumns + typeof (TD).GetProperties().Count(s => s.PropertyType.IsPublic);
		}
	}
}