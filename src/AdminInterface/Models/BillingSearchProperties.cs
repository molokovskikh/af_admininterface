using System.Collections.Generic;
using System.ComponentModel;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;

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

		public Dictionary<object, string> GetClientTypeDescriptions()
		{
			var description = BindingHelper.GetDescriptionsDictionary(typeof (SearchClientType));
			if (SecurityContext.Administrator.HavePermisions(PermissionType.ViewDrugstore, PermissionType.ViewSuppliers))
				return description;

			if (!SecurityContext.Administrator.HavePermisions(PermissionType.ViewDrugstore))
				description.Remove((int)SearchClientType.Drugstore);

			if (!SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers))
				description.Remove((int)SearchClientType.Supplier);

			description.Remove((int)SearchClientType.All);
			return description;
		}
	}
}