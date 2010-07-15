using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
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
		UserId
	}

	public class BillingSearchProperties
	{
		public ulong RegionId { get; set; }

		public uint RecipientId { get; set; }

		public string SearchText { get; set; }

		public PayerStateFilter PayerState { get; set; }

		public SearchSegment Segment { get; set; }

		public SearchClientType ClientType { get; set; }

		public SearchClientStatus ClientStatus { get; set; }

		public SearchBy SearchBy { get; set; }

		public bool IsSearchByName()
		{
			return SearchBy == SearchBy.Name;
		}

		public bool IsSearchByCode()
		{
			return SearchBy == SearchBy.Code;
		}

		public bool IsSearchByUserId()
		{
			return SearchBy == SearchBy.UserId;
		}

		public bool IsSearchByBillingCode()
		{
			return SearchBy == SearchBy.BillingCode;
		}

		public IList<Recipient> Recipients()
		{
			return Recipient.Queryable.OrderBy(r => r.Name).ToList();
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