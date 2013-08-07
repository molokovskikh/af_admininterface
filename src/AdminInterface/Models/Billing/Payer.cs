using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Mailers;
using AdminInterface.Models.Audit;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.NHibernateExtentions;
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Common.Tools;
using Common.Tools.Calendar;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.Models.Audit;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Models.Billing
{
	public enum InvoiceType
	{
		[Description("Вручную")] Manual = 0,
		[Description("Автоматически")] Auto = 1
	}

	public enum InvoicePeriod
	{
		[Description("Помесячно")] Month,
		[Description("Поквартально")] Quarter
	}

	public class InvoiceSettings
	{
		[Property]
		public virtual bool EmailInvoice { get; set; }

		[Property]
		public virtual bool DocumentsOnLastWorkingDay { get; set; }

		[Property]
		public virtual bool DoNotGroupParts { get; set; }

		[Property, Description("Отправлять счета с помощью минипочты")]
		public bool SendToMinimail { get; set; }
	}

	[ActiveRecord(Schema = "billing", Lazy = true), Auditable, Description("Плательщик")]
	public class Payer : ActiveRecordValidationBase<Payer>, IMultiAuditable
	{
		public Payer(string name)
			: this(name, name)
		{
		}

		public Payer(string name, string fullname)
			: this()
		{
			Name = name;
			JuridicalName = fullname;
			ContactGroupOwner = new ContactGroupOwner();
			JuridicalOrganizations.Add(new LegalEntity(name, JuridicalName, this));
			Comment = "";

			Init(SecurityContext.Administrator);
		}

		public Payer()
		{
			JuridicalOrganizations = new List<LegalEntity>();
			InvoiceSettings = new InvoiceSettings();
			Users = new List<User>();
			Clients = new List<Client>();
			Addresses = new List<Address>();
			Suppliers = new List<Supplier>();
			Ads = new List<Advertising>();
			Reports = new List<Report>();
		}

		[PrimaryKey("PayerId"), Description("Договор")]
		public virtual uint Id { get; set; }

		[Property("ShortName")]
		public virtual string Name { get; set; }

		[Property]
		public virtual string JuridicalName { get; set; }

		[Property]
		public virtual string Customer { get; set; }

		[Property]
		public virtual string JuridicalAddress { get; set; }

		[Property]
		public virtual string ReceiverAddress { get; set; }

		[Property]
		public virtual string KPP { get; set; }

		[Property]
		public virtual string INN { get; set; }

		[BelongsTo(Column = "ContactGroupOwnerId", Lazy = FetchWhen.OnInvoke, Cascade = CascadeEnum.SaveUpdate)]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }

		[Property]
		public virtual string ActualAddressCountry { get; set; }

		[Property]
		public virtual string ActualAddressIndex { get; set; }

		[Property]
		public virtual string ActualAddressProvince { get; set; }

		[Property]
		public virtual string ActualAddressTown { get; set; }

		[Property]
		public virtual string ActualAddressRegion { get; set; }

		[Property]
		public virtual string ActualAddressStreet { get; set; }

		[Property]
		public virtual string ActualAddressHouse { get; set; }

		[Property]
		public virtual string ActualAddressOffice { get; set; }

		[Property]
		public virtual string BeforeNamePrefix { get; set; }

		[Property]
		public virtual string AfterNamePrefix { get; set; }

		[Property, Description("Комментарий")]
		public virtual string Comment { get; set; }

		[Property]
		public virtual InvoiceType AutoInvoice { get; set; }

		[Property]
		public virtual InvoicePeriod PayCycle { get; set; }

		[Property]
		public virtual bool HaveContract { get; set; }

		[Property]
		public virtual bool SendRegisteredLetter { get; set; }

		[Property]
		public virtual bool SendScannedDocuments { get; set; }

		[Property]
		public virtual decimal Balance { get; set; }

		[Property]
		public virtual decimal BeginBalance { get; set; }

		[Property]
		public virtual DateTime? BeginBalanceDate { get; set; }

		[Property]
		public virtual decimal PaymentSum { get; set; }

		[Property]
		public virtual bool SendPaymentNotification { get; set; }

		[Nested]
		public virtual InvoiceSettings InvoiceSettings { get; set; }

		[Nested]
		public virtual RegistrationInfo Registration { get; set; }

		[BelongsTo("RecipientId")]
		public virtual Recipient Recipient { get; set; }

		[HasMany(typeof(User), Lazy = true, Inverse = true, OrderBy = "Name")]
		public virtual IList<User> Users { get; set; }

		[HasMany(typeof(Address), Lazy = true, Inverse = true, OrderBy = "Address")]
		public virtual IList<Address> Addresses { get; set; }

		[HasMany(typeof(Supplier), Lazy = true, Inverse = true, OrderBy = "Name")]
		public virtual IList<Supplier> Suppliers { get; set; }

		[HasMany(typeof(LegalEntity), Lazy = true, Inverse = true, Cascade = ManyRelationCascadeEnum.All, OrderBy = "Name")]
		public virtual IList<LegalEntity> JuridicalOrganizations { get; set; }

		[HasMany(typeof(Report), Lazy = true, Inverse = true, OrderBy = "Comment")]
		public virtual IList<Report> Reports { get; set; }

		[HasMany(typeof(Advertising), Lazy = true, Inverse = true, Cascade = ManyRelationCascadeEnum.SaveUpdate)]
		public virtual IList<Advertising> Ads { get; set; }

		[HasAndBelongsToMany(typeof(Client),
			Lazy = true,
			Inverse = true,
			ColumnKey = "PayerId",
			Table = "PayerClients",
			Schema = "Billing",
			ColumnRef = "ClientId")]
		public virtual IList<Client> Clients { get; set; }

		public virtual string GetMailAddress()
		{
			return String.Join(", ",
				new[] {
					!String.IsNullOrWhiteSpace(ActualAddressStreet) ? "ул. " + ActualAddressStreet : null,
					!String.IsNullOrWhiteSpace(ActualAddressHouse) ? "д. " + ActualAddressHouse : null,
					!String.IsNullOrWhiteSpace(ActualAddressOffice) ? "оф. " + ActualAddressOffice : null,
					!String.IsNullOrWhiteSpace(ActualAddressProvince) ? "обл. " + ActualAddressProvince : null,
					!String.IsNullOrWhiteSpace(ActualAddressRegion) ? "р-н " + ActualAddressRegion : null,
					!String.IsNullOrWhiteSpace(ActualAddressTown) ? "г. " + ActualAddressTown : null
				}.Where(s => !String.IsNullOrWhiteSpace(s)));
		}

		public virtual string ShortName
		{
			get { return Name; }
			set { Name = value; }
		}

		public virtual List<string> Emails()
		{
			return ContactGroupOwner.ContactGroups
				.SelectMany(c => c.Contacts)
				.Union(ContactGroupOwner.ContactGroups.SelectMany(c => c.Persons).SelectMany(p => p.Contacts))
				.Where(c => c.Type == ContactType.Email)
				.Select(c => c.ContactText)
				.Distinct()
				.ToList();
		}

		public virtual string GetNamePrefix()
		{
			if (String.IsNullOrWhiteSpace(BeforeNamePrefix))
				return "для Аптеки";
			return BeforeNamePrefix;
		}

		public virtual bool IsManualPayments()
		{
			return AutoInvoice == InvoiceType.Manual;
		}

		public static IEnumerable<Payer> GetLikeAvaliable(ISession session, string searchPattern)
		{
			var resultList = session.Query<Payer>().Where(t => t.Name.Contains(searchPattern)).OrderBy(p => p.Name);
			return resultList;
		}

		public virtual IEnumerable<AuditLogRecord> GetAuditLogs()
		{
			var messages = ArHelper.WithSession(s => {
				var usersAuditLogs = Users.SelectMany(u => new MessageQuery(LogMessageType.User, LogMessageType.System).ExecuteUser(u, s));
				var service = Clients.Select(c => (Service)c).Concat(Suppliers.Select(sup => (Service)sup)).Concat(Addresses.Select(a => (Service)a.Client)).Distinct();
				var serviceMessages = service.SelectMany(ser => new MessageQuery(LogMessageType.User, LogMessageType.System).Execute(ser, s));
				return usersAuditLogs.Concat(serviceMessages);
			});
			return messages.Select(m => new AuditLogRecord {
				ObjectId = m.ObjectId,
				Message = m.Message,
				Name = m.Name,
				OperatorName = m.Operator,
				LogType = m.Type,
				LogTime = m.WriteTime
			});
		}

		public virtual decimal TotalSum
		{
			get { return GetAccounts().Sum(a => a.Payment); }
		}

		public virtual IEnumerable<Account> GetAccounts()
		{
			return UsersForInvoice().Concat(AddressesForInvoice()).Concat(ReportsForInvoice()).Concat(SupplierAccounts());
		}

		private IEnumerable<SupplierAccount> SupplierAccounts()
		{
			return Suppliers.Select(s => s.Account).Where(a => a.ShouldPay());
		}

		private IEnumerable<Account> AddressesForInvoice()
		{
			return Addresses.Select(a => a.Accounting).Where(a => a.ShouldPay()).Skip(UsersForInvoice().Count());
		}

		private IEnumerable<Account> UsersForInvoice()
		{
			return Users.Select(u => u.Accounting).Where(a => a.ShouldPay());
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(Name))
				return string.Empty;
			return Name;
		}

		public virtual IEnumerable<IAuditRecord> GetAuditRecords(IEnumerable<AuditableProperty> properties)
		{
			return Clients.Select(c => new AuditRecord(c));
		}

		public virtual void AddComment(string comment)
		{
			if (String.IsNullOrEmpty(comment))
				return;

			if (!String.IsNullOrEmpty(Comment))
				Comment += "\r\n";
			Comment += comment;
		}

		public virtual string GetInvocesAddress()
		{
			var mails = new List<string>();

			var group = ContactGroupOwner.ContactGroups
				.FirstOrDefault(g => g.Type == ContactGroupType.Invoice);
			if (group != null)
				mails.AddRange(group
					.Contacts
					.Where(c => c.Type == ContactType.Email)
					.Select(c => c.ContactText));

			var contacts = mails.Implode();
			if (String.IsNullOrWhiteSpace(contacts))
				throw new DoNotHaveContacts(String.Format("Для плательщика {0} - {1} не задана контактрая информаци для отправки счетов", Id, Name));

			return contacts;
		}

		public virtual IEnumerable<string> ClientsMinimailAddresses
		{
			get { return Clients.Where(c => !c.Disabled).Select(c => String.Format("{0}@client.docs.analit.net", c.Id)); }
		}

		public virtual string ClientsMinimailAddressesAsString
		{
			get { return ClientsMinimailAddresses.Implode(); }
		}

		public virtual void RecalculateBalance()
		{
			var invoicesSum = Invoice.Queryable.Where(i => i.Payer == this).Sum(i => i.BalanceAmount);
			var paymentsSum = Payment.Queryable.Where(p => p.Payer == this).Sum(p => p.BalanceAmount);
			var operationsSum = BalanceOperation.Queryable.Where(o => o.Payer == this).Sum(o => o.BalanceAmount);

			Balance = 0;
			Balance = BeginBalance
				+ invoicesSum
				+ paymentsSum
				+ operationsSum;
		}

		private void UpdateBalance()
		{
			if (this.IsChanged(p => p.BeginBalance)) {
				var oldBeginBalance = this.OldValue(p => p.BeginBalance);
				Balance += BeginBalance;
				Balance -= oldBeginBalance;
			}
		}

		public virtual List<ContactGroupType> NewGroupTypes
		{
			get
			{
				if (ContactGroupOwner == null)
					return null;
				var types = new List<ContactGroupType>();
				if (!ContactGroupOwner.HaveGroup(ContactGroupType.Billing))
					types.Add(ContactGroupType.Billing);
				if (!ContactGroupOwner.HaveGroup(ContactGroupType.Invoice))
					types.Add(ContactGroupType.Invoice);
				types.Add(ContactGroupType.Custom);
				return types;
			}
		}

		protected override void OnSave()
		{
			UpdateBalance();
			base.OnSave();
		}

		protected override void OnUpdate()
		{
			UpdateBalance();
			base.OnUpdate();
		}

		public virtual void Init(Administrator admin)
		{
			Registration = new RegistrationInfo(admin);

			if (ContactGroupOwner == null)
				ContactGroupOwner = new ContactGroupOwner();

			if (!ContactGroupOwner.HaveGroup(ContactGroupType.Billing))
				ContactGroupOwner.AddContactGroup(ContactGroupType.Billing);
		}

		public virtual DateTime GetDocumentDate(DateTime date)
		{
			if (!InvoiceSettings.DocumentsOnLastWorkingDay) {
				return date;
			}

			var lastDay = date.LastDayOfMonth();
			if (lastDay.DayOfWeek == DayOfWeek.Saturday)
				return lastDay.AddDays(-1);
			else if (lastDay.DayOfWeek == DayOfWeek.Sunday)
				return lastDay.AddDays(-2);
			else
				return lastDay;
		}

		public virtual IEnumerable<Invoice> BuildInvoices(DateTime date, Period period)
		{
			return GetAccounts()
				.GroupBy(a => a.InvoiceGroup)
				.Select(invoiceGroup => new Invoice(this, period, date, invoiceGroup.Key));
		}

		public virtual IEnumerable<IGrouping<int, Account>> GetInvoiceGroups()
		{
			return GetAccounts().GroupBy(a => a.InvoiceGroup).OrderBy(g => g.Key);
		}

		public virtual IEnumerable<ReportAccount> ReportsForInvoice()
		{
			return GetReportAccounts().Where(a => a.ShouldPay());
		}

		public virtual IList<ReportAccount> GetReportAccounts()
		{
			if (Reports.Count == 0)
				return Enumerable.Empty<ReportAccount>().ToList();

			var reportIds = Reports.Select(r => r.Id).ToArray();
			return ActiveRecordLinqBase<ReportAccount>.Queryable
				.Where(a => reportIds.Contains(a.Report.Id))
				.OrderBy(a => a.Report.Comment)
				.ToList();
		}

		public virtual void UpdatePaymentSum()
		{
			PaymentSum = TotalSum;
		}

		public virtual int[] Years
		{
			get { return Period.Years; }
		}

		public virtual bool CanDelete(ISession session)
		{
			var maxPayment = session.QueryOver<Payment>()
				.Where(p => p.Payer == this)
				.SelectList(l => l.SelectMax(p => p.PayedOn))
				.SingleOrDefault<DateTime>();

			var documentDates = new[] { maxPayment };

			return Reports.Count == 0
				&& Clients.All(c => c.CanDelete(session))
				&& Suppliers.All(s => s.CanDelete(session))
				&& documentDates.Max().AddMonths(15) < DateTime.Now;
		}

		public virtual void CheckBeforeDelete(ISession session)
		{
			var maxPayment = session.QueryOver<Payment>()
				.Where(p => p.Payer == this)
				.SelectList(l => l.SelectMax(p => p.PayedOn))
				.SingleOrDefault<DateTime>();

			if (maxPayment.AddMonths(36) > DateTime.Now)
				throw new EndUserException("За последние 36 месяцев имеются платежи");

			if (Reports.Count > 0)
				throw new EndUserException("Есть отчеты");

			Clients.Each(c => c.CheckBeforeDelete(session));
			Suppliers.Each(s => s.CheckBeforeDelete(session));
		}

		public virtual void Delete(ISession session)
		{
			CheckBeforeDelete(session);

			var clients = Clients.Where(c => c.CanDelete(session)).ToArray();
			foreach (var client in clients)
				client.Delete(session);

			foreach (var supplier in Suppliers)
				supplier.Delete(session);

			session.Delete(this);
		}

		public virtual void NotifyAboutDelete(MonorailMailer mailer, string deleteComment)
		{
			mailer.PayerDelete(this, deleteComment).Send();
		}

		public virtual IEnumerable<ModelAction> Actions
		{
			get
			{
				return new[] {
					new ModelAction(this, "Delete", "Удалить Плательщика")
				};
			}
		}

		public virtual void ApplySettingsTemplate(DrugstoreSettings settings)
		{
			if (Id == 921)
				settings.SendWaybillsFromClient = true;
		}

		public virtual void CheckCommentChangesAndLog(MonorailMailer mailer)
		{
			if (!this.IsChanged(p => p.Comment))
				return;

			var oldValue = this.OldValue(p => p.Comment);
			var propertyInfo = typeof(Payer).GetProperty("Comment");
			var property = new DiffAuditableProperty(propertyInfo, BindingHelper.GetDescription(propertyInfo), Comment, oldValue);
			mailer.NotifyAboutChanges(property, this, "BillingList@analit.net");
			foreach (var client in Clients) {
				var log = new AuditRecord(client) {
					Message = property.Message,
					IsHtml = property.IsHtml,
					MessageType = LogMessageType.Stat
				};
				log.Save();
			}
		}
	}

	public class DeleteReportsRequest : BaseRemoteRequest
	{
		public static void Process(IList<Report> reports)
		{
			if (reports.Count > 0) {
				var config = Global.Config;
				var request = reports.Implode(r => String.Format("ids={0}", r.Id), "&");
				MakeRequest(config.DeleteReportUri,
					config.ReportSystemUser,
					config.ReportSystemPassword,
					request);
			}
		}
	}

	public class DoNotHaveContacts : Exception
	{
		public DoNotHaveContacts(string message) : base(message)
		{
		}
	}
}