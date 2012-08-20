using System.Collections.Generic;
using System.Linq;

namespace AdminInterface.Helpers
{
	public class Pager
	{
		public int Page;
		public int PageSize = 30;

		public int Total;

		public int TotalPages
		{
			get
			{
				var result = Total / PageSize;
				if (Total % PageSize > 0)
					result++;
				return result;
			}
		}

		public Pager()
		{
		}

		public Pager(int? page, int pageSize)
		{
			if (page != null)
				Page = page.Value;
			PageSize = pageSize;
		}

		public IEnumerable<T> DoPage<T>(IEnumerable<T> enumerable)
		{
			return enumerable
				.Skip(Page * PageSize)
				.Take(PageSize);
		}
	}
}