using System.ComponentModel;

namespace AdminInterface.Models
{
	public enum PayerStateFilter
	{
		[Description("Все")] All,
		[Description("Должники")] Debitors,
		[Description("Не должники")] NotDebitors,
	}

	public enum SearchSegment
	{
		[Description("Все")] All,
		[Description("Розница")] Retail,
		[Description("Опт")] Wholesale,
	}

	public enum SearchClientType
	{
		[Description("Все")] All,
		[Description("Аптека")] Drugstore,
		[Description("Поставщик")] Supplier
	}

	public enum SearchClientStatus
	{
		[Description("Все")] All,
		[Description("Отключенные")] Disabled,
		[Description("Включеные")] Enabled
	}

	public enum SearchBy
	{
		Name,
		Code,
		BillingCode,
	}

	public class BillingSearchProperties
	{
		private SearchBy _searchBy;

		public ulong RegionId { get; set; }

		public string SearchText { get; set; }

		public PayerStateFilter PayerState { get; set; }

		public SearchSegment Segment { get; set; }

		public SearchClientType ClientType { get; set; }

		public SearchClientStatus ClientStatus { get; set; }

		public SearchBy SearchBy
		{
			get { return _searchBy; }
			set { _searchBy = value; }
		}
		
		public bool IsSearchByName()
		{
			return _searchBy == SearchBy.Name;
		}

		public bool IsSearchByCode()
		{
			return _searchBy == SearchBy.Code;
		}

		public bool IsSearchByBillingCode()
		{
			return _searchBy == SearchBy.BillingCode;
		}
	}
}