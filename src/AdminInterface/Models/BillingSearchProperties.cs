using System.ComponentModel;

namespace AdminInterface.Models
{
	public enum PayerStateFilter
	{
		[Description("���")] All,
		[Description("��������")] Debitors,
		[Description("�� ��������")] NotDebitors,
	}

	public enum SearchSegment
	{
		[Description("���")] All,
		[Description("�������")] Retail,
		[Description("���")] Wholesale,
	}

	public enum SearchClientType
	{
		[Description("���")] All,
		[Description("������")] Drugstore,
		[Description("���������")] Supplier
	}

	public enum SearchClientStatus
	{
		[Description("���")] All,
		[Description("�����������")] Disabled,
		[Description("���������")] Enabled
	}

	public enum SearchBy
	{
		Name,
		Code,
		BillingCode,
	}

	public class BillingSearchProperties
	{
		private string _shortName;
		private ulong _regionId;
		private PayerStateFilter _payerState;
		private SearchSegment _segment;
		private SearchClientType _clientType;
		private SearchClientStatus _clientStatus;
		private SearchBy _searchBy;

		public ulong RegionId
		{
			get { return _regionId; }
			set { _regionId = value; }
		}

		public string SearchText
		{
			get { return _shortName; }
			set { _shortName = value; }
		}

		public PayerStateFilter PayerState
		{
			get { return _payerState; }
			set { _payerState = value; }
		}

		public SearchSegment Segment
		{
			get { return _segment; }
			set { _segment = value; }
		}

		public SearchClientType ClientType
		{
			get { return _clientType; }
			set { _clientType = value; }
		}

		public SearchClientStatus ClientStatus
		{
			get { return _clientStatus; }
			set { _clientStatus = value; }
		}

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