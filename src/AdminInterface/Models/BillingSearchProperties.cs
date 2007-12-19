using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using Common.Web.Ui.Models;
using NHibernate.Expression;

namespace AdminInterface.Model
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

	public class BillingSearchProperties
	{
		private string _shortName;
		private ulong _regionId;
		private PayerStateFilter _payerState;
		private SearchSegment _segment;
		private SearchClientType _clientType;
		private SearchClientStatus _clientStatus;

		public ulong RegionId
		{
			get { return _regionId; }
			set { _regionId = value; }
		}

		public string ShortName
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
	}
}
