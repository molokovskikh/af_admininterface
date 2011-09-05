using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.NHibernateExtentions;

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
		[Description("Наименование")] Name,
		[Description("Код клиента")] ClientId,
		[Description("Код пользователя")] UserId,
		[Description("Договор")] PayerId,
		[Description("Инн")] Inn,
		[Description("Адрес доставки")] Address,
	}

	public class PayerFilter : Sortable, SortableContributor, IUrlContributor
	{
		public string SearchText { get; set; }

		public Recipient Recipient { get; set; }

		public Region Region {get; set; }

		public PayerStateFilter PayerState { get; set; }

		public SearchSegment Segment { get; set; }

		public SearchClientType ClientType { get; set; }

		public SearchClientStatus ClientStatus { get; set; }

		public InvoiceType? InvoiceType { get; set; }

		public SearchBy SearchBy { get; set; }

		public PayerFilter()
		{
			ClientStatus = SearchClientStatus.Enabled;
			SearchBy = SearchBy.Name;
			SortBy = "ShortName";
			SortKeyMap = new Dictionary<string, string> {
				{"BillingCode", "BillingCode"},
				{"ShortName", "ShortName"},
				{"Recipient", "Recipient"},
				{"Balance", "Balance"},
				{"LastClientRegistrationDate", "LastClientRegistrationDate"},
				{"DisabledUsersCount", "DisabledUsersCount"},
				{"EnabledUsersCount", "EnabledUsersCount"},
				{"DisabledAddressesCount", "DisabledAddressesCount"},
				{"EnabledAddressesCount", "EnabledAddressesCount"}
			};
		}

		public string GetUri()
		{
			return PublicPropertiesToUrlParts("filter")
				.Where(v => v.Key != "filter.SortBy" && v.Key != "filter.SortDirection")
				.Implode(v => String.Format("{0}={1}", v.Key, v.Value), "&");
		}

		public IDictionary GetQueryString()
		{
			return PublicPropertiesToUrlParts("filter");
		}

		public IList<BillingSearchItem> Find()
		{
			var where = new StringBuilder("1 = 1");
			var having = new StringBuilder();
			var groupFilter = new StringBuilder();

			var query = new DetachedSqlQuery();
			var text = SearchText;
			switch(SearchBy)
			{
				case SearchBy.Name:
					And(having, String.Format(
						@"(p.ShortName like :searchText
or p.JuridicalName like :searchText
or sum(if(cd.Name like :searchText or cd.FullName like :searchText, 1, 0)) > 0)"));
					text = "%" + SearchText + "%";
					break;
				case SearchBy.ClientId:
					And(having, "sum(if(cd.Id = :searchText, 1, 0)) > 0");
					break;
				case SearchBy.UserId:
					And(having, "sum(if(users.Id = :searchText, 1, 0)) > 0");
					break;
				case SearchBy.PayerId:
					And(having, "p.payerId = :searchText");
					break;
				case SearchBy.Inn:
					And(where, "p.INN = :searchText");
					text = "%" + SearchText + "%";
					break;
				case SearchBy.Address:
					And(having, "sum(if(addresses.Address = :searchText, 1, 0)) > 0");
					text = "%" + SearchText + "%";
					break;
			}
			query.SetParameter("searchText", text);

			switch (PayerState)
			{
				case PayerStateFilter.Debitors:
					And(where, "p.Balance < 0");
					break;
				case PayerStateFilter.NotDebitors:
					And(where, "p.oldpaydate >= 0");
					break;
			}

			if (InvoiceType.HasValue)
			{
				And(where, "p.AutoInvoice = :InvoiceType");
				query.SetParameter("InvoiceType", InvoiceType.Value);
			}

			switch(Segment)
			{
				case SearchSegment.Retail:
					And(groupFilter, "cd.Segment = 1 or s.Segment = 1");
					break;
				case SearchSegment.Wholesale:
					And(groupFilter, "cd.Segment = 0 or s.Segment = 1");
					break;
			}

			switch(ClientType)
			{
				case SearchClientType.Drugstore:
					And(groupFilter, "cd.Id is not null");
					break;
				case SearchClientType.Supplier:
					And(groupFilter, "s.Id is not null");
					break;
			}

			switch(ClientStatus)
			{
				case SearchClientStatus.Enabled:
					And(having, "sum(if(cd.Status = 1 or s.Disabled = 0, 1, 0)) > 0");
					break;
				case SearchClientStatus.Disabled:
					And(having, "sum(if(cd.Status = 1 or s.Disabled = 0, 1, 0)) = 0");
					break;
			}

			if (Recipient != null && Recipient.Id != 0)
			{
				And(groupFilter, " p.RecipientId = :recipientId");
				query.SetParameter("recipientId", Recipient.Id);
			}

			if (Region != null && Region.Id != 0)
			{
				And(groupFilter, "cd.MaskRegion & :RegionId > 0");
				query.SetParameter("RegionId", Region.Id);
			}

			if (groupFilter.Length > 0)
				And(having, String.Format("sum(if({0}, 1, 0)) > 0", groupFilter));

			var sql = String.Format(@"
select p.payerId as {{BillingSearchItem.BillingCode}},
		p.JuridicalName,
		p.ShortName as {{BillingSearchItem.ShortName}},
		p.Balance as {{BillingSearchItem.Balance}},
		p.OldTariff as {{BillingSearchItem.PaySum}},
		max(cd.RegistrationDate) as {{BillingSearchItem.LastClientRegistrationDate}},
		count(distinct if(cd.Status = 1, cd.Id, null)) as {{BillingSearchItem.EnabledClientCount}},
		count(distinct if(s.Disabled = 0, s.Id, null)) as {{BillingSearchItem.EnabledSupplierCount}},
		count(distinct if(users.Enabled = 0, users.Id, null)) as {{BillingSearchItem.DisabledUsersCount}},
		count(distinct if(users.Enabled = 1, users.Id, null)) as {{BillingSearchItem.EnabledUsersCount}},
		count(distinct if(addresses.Enabled = 0, addresses.Id, null)) as {{BillingSearchItem.DisabledAddressesCount}},
		count(distinct if(addresses.Enabled = 1, addresses.Id, null)) as {{BillingSearchItem.EnabledAddressesCount}},

		not p.AutoInvoice as {{BillingSearchItem.ShowPayDate}},

		(select cast(group_concat(r.region order by r.region separator ', ') as char)
		from farm.regions r
		where r.regioncode & bit_or(cd.maskregion) > 0) as {{BillingSearchItem.Regions}},

		sum(if(cd.Segment = 1 or s.Segment = 1, 1, 0)) > 0 as {{BillingSearchItem.HasRetailSegment}},
		sum(if(cd.Segment = 0 or s.Segment = 0, 1, 0)) > 0 as {{BillingSearchItem.HasWholesaleSegment}},
		r.Name as {{BillingSearchItem.Recipient}}
from billing.payers p
	left join Billing.Recipients r on r.Id = p.RecipientId
	left join future.Users users on users.PayerId = p.PayerId
	left join future.Clients cd on cd.Id = users.ClientId
	left join future.Addresses addresses on addresses.PayerId = p.PayerId
	left join future.Suppliers s on s.Payer = p.PayerId
where {0}
group by p.payerId
having {1}
order by {{BillingSearchItem.ShortName}}
", where, having);

			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(BillingSearchItem));
			try
			{
				query.Sql = sql;
				var result = query.GetSqlQuery(session)
					.AddEntity(typeof(BillingSearchItem))
					.List<BillingSearchItem>();

				ArHelper.Evict(session, result);
				result = result.ToList();
				((List<BillingSearchItem>)result).Sort(new PropertyComparer<BillingSearchItem>(GetSortDirection(), GetSortProperty()));
				return result;
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
			
		}

		private static void And(StringBuilder filter, string criteria)
		{
			if (filter.Length > 0)
				filter.Append(" and ");
			filter.Append(criteria);
		}
	}
}