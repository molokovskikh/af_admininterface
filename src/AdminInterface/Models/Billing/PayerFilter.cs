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
using NHibernate;

namespace AdminInterface.Models.Billing
{
	public enum PayerStateFilter
	{
		[Description("Все")] All,
		[Description("Должники")] Debitors,
		[Description("Не должники")] NotDebitors,
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
		[Description("Включенные")] Enabled
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
		private ISession session;

		public string SearchText { get; set; }

		[Description("Игнорировать плательщиков, содержащих Поставщиков, кроме Справки")]
		public bool WithoutSuppliers { get; set; }

		[Description("Получатель платежей:")]
		public Recipient Recipient { get; set; }

		[Description("Регион:")]
		public Region Region { get; set; }

		[Description("Должен\\Не должен:")]
		public PayerStateFilter PayerState { get; set; }

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

		public PayerFilter(ISession session)
		{
			this.session = session;
			WithoutSuppliers = true;
			Period = new Period();
			ClientStatus = SearchClientStatus.Enabled;
			SearchBy = SearchBy.Name;
			SortBy = "ShortName";
			SortKeyMap = new Dictionary<string, string> {
				{ "PayerId", "PayerId" },
				{ "ShortName", "ShortName" },
				{ "Recipient", "Recipient" },
				{ "PaymentSum", "PaymentSum" },
				{ "Balance", "Balance" },
				{ "LastClientRegistrationDate", "LastClientRegistrationDate" },
				{ "DisabledUsersCount", "DisabledUsersCount" },
				{ "EnabledUsersCount", "EnabledUsersCount" },
				{ "DisabledAddressesCount", "DisabledAddressesCount" },
				{ "EnabledAddressesCount", "EnabledAddressesCount" }
			};
		}

		public IList<BillingSearchItem> Find()
		{
			var where = new StringBuilder("1 = 1");
			var having = new StringBuilder("1 = 1");
			var groupFilter = new StringBuilder();

			var query = new DetachedSqlQuery();
			var text = SearchText;
			switch (SearchBy) {
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

			switch (PayerState) {
				case PayerStateFilter.Debitors:
					And(where, "p.Balance < 0");
					break;
				case PayerStateFilter.NotDebitors:
					And(where, "p.Balance >= 0");
					break;
			}

			if (InvoiceType.HasValue) {
				And(where, "p.AutoInvoice = :InvoiceType");
				query.SetParameter("InvoiceType", InvoiceType.Value);
			}

			switch (ClientType) {
				case SearchClientType.Drugstore:
					And(groupFilter, "cd.Id is not null");
					if(WithoutSuppliers) {
						And(where, "(s.Id is null OR exists (select * from farm.Regions rg where rg.RegionCode & s.RegionMask > 0 and rg.DrugsSearchRegion = 1))");
					}
					break;
				case SearchClientType.Supplier:
					And(groupFilter, "s.Id is not null");
					break;
			}

			switch (ClientStatus) {
				case SearchClientStatus.Enabled:
					And(having, "(EnabledClientCount > 0 or EnabledSupplierCount > 0 or EnabledReportsCount > 0)");
					break;
				case SearchClientStatus.Disabled:
					And(having, "(EnabledClientCount = 0 and EnabledSupplierCount = 0 and EnabledReportsCount = 0)");
					break;
			}

			if (Recipient != null && Recipient.Id != 0) {
				And(groupFilter, " p.RecipientId = :recipientId");
				query.SetParameter("recipientId", Recipient.Id);
			}

			if (Region != null && Region.Id != 0) {
				And(groupFilter, "(cd.MaskRegion & :RegionId > 0 or s.RegionMask & :RegionId > 0)");
				query.SetParameter("RegionId", Region.Id);
			}

			if (groupFilter.Length > 0)
				And(having, String.Format("sum(if({0}, 1, 0)) > 0", groupFilter));

			if (SearchWithoutDocuments) {
				//мы не должны выбирать тех плательщиков у которых не могло быть документов
				//за этот период тк они еще не были зарегистрированы
				And(where, "p.RegistrationDate <= :PeriodEnd");
				And(where, "p.PayCycle = :InvoicePeriod");
				And(where, GetDocumentSubQuery(DocumentType));
				query.SetParameter("InvoicePeriod", Period.GetInvoicePeriod());
				query.SetParameter("PeriodEnd", Period.GetPeriodEnd());
				query.SetParameter("Period", Period.ToSqlString());
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
		(select count(*) from Reports.General_Reports r where r.Allow = 1 and r.PayerId = p.PayerId) as EnabledReportsCount,

		not p.AutoInvoice as ShowPayDate,

		(select cast(group_concat(r.region order by r.region separator ', ') as char)
		from farm.regions r
		where r.regioncode & bit_or(ifnull(cd.MaskRegion, s.RegionMask)) > 0) as Regions,

		r.Name as Recipient
from billing.payers p
	left join Billing.Recipients r on r.Id = p.RecipientId
	left join Customers.Users users on users.PayerId = p.PayerId
	left join Customers.Clients cd on cd.Id = users.ClientId
	left join Customers.Addresses addresses on addresses.PayerId = p.PayerId
	left join Customers.Suppliers s on s.Payer = p.PayerId
where {0}
group by p.payerId
having {1}
order by p.ShortName
", where, having);

			query.Sql = sql;
			var result = query.GetSqlQuery(session).ToList<BillingSearchItem>().ToList();
			result.Sort(new PropertyComparer<BillingSearchItem>(GetSortDirection(), GetSortProperty()));
			return result;
		}

		private static string GetDocumentSubQuery(DocumentType documentType)
		{
			var template = "not exists(select * from {0} d where d.Payer = p.PayerId and d.Period = :Period)";
			string table;
			if (documentType == DocumentType.Invoice) {
				table = "Billing.Invoices";
				return String.Format(template, table);
			}
			else if (documentType == DocumentType.Act) {
				table = "Billing.Acts";
				return String.Format(template, table);
			}
			else {
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