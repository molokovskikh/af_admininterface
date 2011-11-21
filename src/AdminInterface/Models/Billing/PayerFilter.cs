using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;

namespace AdminInterface.Models.Billing
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

	public enum DocumentType
	{
		[Description("Счет")] Invoice,
		[Description("Акт")] Act,
		[Description("Счет и акт")] Both,
	}

	public class PayerFilter : Sortable
	{
		public string SearchText { get; set; }

		[Description("Получатель платежей:")]
		public Recipient Recipient { get; set; }

		[Description("Регион:")]
		public Region Region {get; set; }

		[Description("Должен\\Не должен:")]
		public PayerStateFilter PayerState { get; set; }

		[Description("Сегмент:")]
		public SearchSegment Segment { get; set; }

		[Description("Тип:")]
		public SearchClientType ClientType { get; set; }

		[Description("Отключен\\Не отключен:")]
		public SearchClientStatus ClientStatus { get; set; }

		[Description("Тип выставления счета:")]
		public InvoiceType? InvoiceType { get; set; }

		public SearchBy SearchBy { get; set; }

		[Description("Искать плательщиков без документов:")]
		public bool SearchWithoutDocuments { get; set; }

		[Description("Период:")]
		public Period Period { get; set; }

		[Description("Тип документа:")]
		public DocumentType DocumentType { get; set; }

		public PayerFilter()
		{
			ClientStatus = SearchClientStatus.Enabled;
			SearchBy = SearchBy.Name;
			SortBy = "ShortName";
			SortKeyMap = new Dictionary<string, string> {
				{"PayerId", "PayerId"},
				{"ShortName", "ShortName"},
				{"Recipient", "Recipient"},
				{"PaymentSum", "PaymentSum"},
				{"Balance", "Balance"},
				{"LastClientRegistrationDate", "LastClientRegistrationDate"},
				{"DisabledUsersCount", "DisabledUsersCount"},
				{"EnabledUsersCount", "EnabledUsersCount"},
				{"DisabledAddressesCount", "DisabledAddressesCount"},
				{"EnabledAddressesCount", "EnabledAddressesCount"}
			};
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
					And(having, "p.PayerId = :searchText");
					break;
				case SearchBy.Inn:
					And(where, "p.INN like :searchText");
					text = "%" + SearchText + "%";
					break;
				case SearchBy.Address:
					And(having, "sum(if(addresses.Address like :searchText, 1, 0)) > 0");
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
					And(where, "p.Balance >= 0");
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

			if (SearchWithoutDocuments)
			{
				//мы не должны выбирать тех плательщиков у которых не могло быть документов
				//за этот период тк они еще не были зарегистрированны
				And(where, "p.RegistrationDate <= :PeriodEnd");
				And(where, "p.PayCycle = :InvoicePeriod");
				And(where, GetDocumentSubQuery(DocumentType));
				query.SetParameter("InvoicePeriod", Period.GetInvoicePeriod());
				query.SetParameter("PeriodEnd", Period.GetPeriodEnd());
				query.SetParameter("Period", Period);
			}

			var sql = String.Format(@"
select p.PayerId,
		p.ShortName,
		p.JuridicalName,
		p.Balance,
		p.PaymentSum,
		max(cd.RegistrationDate) as LastClientRegistrationDate,
		count(distinct if(cd.Status = 1, cd.Id, null)) as EnabledClientCount,
		count(distinct if(s.Disabled = 0, s.Id, null)) as EnabledSupplierCount,
		count(distinct if(users.Enabled = 0, users.Id, null)) as DisabledUsersCount,
		count(distinct if(users.Enabled = 1, users.Id, null)) as EnabledUsersCount,
		count(distinct if(addresses.Enabled = 0, addresses.Id, null)) as DisabledAddressesCount,
		count(distinct if(addresses.Enabled = 1, addresses.Id, null)) as EnabledAddressesCount,

		not p.AutoInvoice as ShowPayDate,

		(select cast(group_concat(r.region order by r.region separator ', ') as char)
		from farm.regions r
		where r.regioncode & bit_or(cd.maskregion) > 0) as Regions,

		sum(if(cd.Segment = 1 or s.Segment = 1, 1, 0)) > 0 as HasRetailSegment,
		sum(if(cd.Segment = 0 or s.Segment = 0, 1, 0)) > 0 as HasWholesaleSegment,
		r.Name as Recipient
from billing.payers p
	left join Billing.Recipients r on r.Id = p.RecipientId
	left join future.Users users on users.PayerId = p.PayerId
	left join future.Clients cd on cd.Id = users.ClientId
	left join future.Addresses addresses on addresses.PayerId = p.PayerId
	left join future.Suppliers s on s.Payer = p.PayerId
where {0}
group by p.payerId
having {1}
order by p.ShortName
", where, having);

			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			var session = sessionHolder.CreateSession(typeof(BillingSearchItem));
			try
			{
				query.Sql = sql;
				var result = query.GetSqlQuery(session).ToList<BillingSearchItem>().ToList();
				result.Sort(new PropertyComparer<BillingSearchItem>(GetSortDirection(), GetSortProperty()));
				return result;
			}
			finally
			{
				sessionHolder.ReleaseSession(session);
			}
		}

		private static string GetDocumentSubQuery(DocumentType documentType)
		{
			var template = "not exists(select * from {0} d where d.Payer = p.PayerId and d.Period = :Period)";
			string table;
			if (documentType == DocumentType.Invoice)
			{
				table = "Billing.Invoices";
				return String.Format(template, table);
			}
			else if (documentType == DocumentType.Act)
			{
				table = "Billing.Acts";
				return String.Format(template, table);
			}
			else
			{
				return String.Format("{0} and {1}", GetDocumentSubQuery(DocumentType.Invoice), GetDocumentSubQuery(DocumentType.Act));
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