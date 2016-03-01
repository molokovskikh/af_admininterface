using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using AdminInterface.Helpers;
using AdminInterface.Models.Audit;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Certificates;
using AdminInterface.Models.Listeners;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Validators;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Common.MySql;
using Common.Tools;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.Models.Audit;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate;
using log4net;
using NPOI.SS.Formula.Functions;

namespace AdminInterface.Models.Suppliers
{
	public class RegionEdit
	{
		private Supplier supplier;
		private Administrator admin;
		private List<Region> regions;

		public RegionEdit(ISession session, Supplier supplier, Administrator admin)
		{
			this.supplier = supplier;
			this.admin = admin;
			regions = Region.GetRegionsByMask(admin.RegionMask).Except(supplier.GetRegions(session, admin)).ToList();
		}

		[ValidateNonEmpty, Description("Регион")]
		public Region Region { get; set; }

		[ValidateNonEmpty, Description("ФИО согласовавшего включение")]
		public string PermitedBy { get; set; }

		[ValidateNonEmpty, Description("Фиксированная ежемесячная оплата")]
		public decimal FixedPayAmount { get; set; }

		[ValidateNonEmpty, Description("Процентная ставка по заказам")]
		public decimal PercentPayAmount { get; set; }

		[ValidateNonEmpty, Description("Контактное лицо поставщика")]
		public string RequestedBy { get; set; }

		public bool ShouldNotify()
		{
			return FixedPayAmount > 0 || PercentPayAmount > 0;
		}

		public List<Region> Regions()
		{
			return regions;
		}

		public string Subject()
		{
			return String.Format("Поставщик {0} код {1} добавлен регион {2}", supplier.Name, supplier.Id, Region.Name);
		}

		public string Body()
		{
			return String.Format("Дата изменения: {0}\r\n" +
				"Сотрудник: {1}\r\n" +
				"Хост: {2}\r\n" +
				"Регион: {3}\r\n" +
				"ФИО согласовавшего включение: {4}\r\n" +
				"Фиксированная ежемесячная оплата: {5}\r\n" +
				"Процентная ставка по заказам: {6}\r\n" +
				"Контактное лицо поставщика: {7}",
				DateTime.Now, admin.Name, admin.Host,
				Region.Name, PermitedBy, FixedPayAmount, PercentPayAmount, RequestedBy);
		}

		public AuditRecord GetAuditRecord()
		{
			var message = String.Format("Подключен регион: {0}\r\n" +
				"ФИО согласовавшего включение: {1}\r\n" +
				"Фиксированная ежемесячная оплата: {2}\r\n" +
				"Процентная ставка по заказам: {3}\r\n" +
				"Контактное лицо поставщика: {4}",
				Region.Name, PermitedBy, FixedPayAmount, PercentPayAmount, RequestedBy);
			return new AuditRecord(message, supplier);
		}
	}

	[ActiveRecord(Schema = "Customers", Lazy = true), Auditable, Description("Поставщик")]
	public class Supplier : Service, IChangesNotificationAware, IMultiAuditable
	{
		private ContactGroupType[] _defaultGroups = {
			ContactGroupType.ClientManagers,
			ContactGroupType.OrderManagers
		};

		public Supplier(Region homeRegion, Payer payer)
			: this()
		{
			Payer = payer;
			HomeRegion = homeRegion;
			RegionMask = homeRegion.Id;
			Account = new SupplierAccount(this);
		}

		public Supplier()
		{
			CertificateSources = new List<CertificateSource>();
			Registration = new RegistrationInfo();
			OrderRules = new List<OrderSendRules>();
			Prices = new List<Price>();
			RegionalData = new List<RegionalData>();
			Type = ServiceType.Supplier;
		}

		[JoinedKey("Id")]
		public virtual uint SupplierId { get; set; }

		[Property,
		Auditable,
		Notify,
		Description("Краткое наименование"),
		ValidateNonEmpty,
		ValidateRegExpAttribute(@"^[\wа-яА-Я-Ёё\.,\(\)\+ ]+$",
			"Поле может содержать только пробел, буквы, цифры и знаки('_', '-', '+', '.', ',', '(', ')')")]
		public override string Name { get; set; }

		[Property, ValidateNonEmpty, Auditable, Notify, Description("Полное наименование")]
		public virtual string FullName { get; set; }

		[Property, Auditable, NotifyBilling, ValidateGreaterThanZero("Вы не выбрали регионы работы"), Description("Регионы работы"), SetForceReplication]
		public virtual ulong RegionMask { get; set; }

		[Property, Auditable, Description("VendorId")]
		public virtual string VendorId { get; set; }

		[BelongsTo, Auditable, Description("Домашний регион")]
		public override Region HomeRegion { get; set; }

		[Property]
		public virtual string Address { get; set; }

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore), Style, Auditable("Включен")]
		public override bool Disabled
		{
			get { return _disabled; }
			set
			{
				if (_disabled != value) {
					_disabled = value;
					if (Payer != null)
						Payer.UpdatePaymentSum();
				}
			}
		}

		public virtual string INN
		{
			get {
				if(Payer != null && !string.IsNullOrEmpty(Payer.INN))
					return "ИНН: " + Payer.INN;
				return "";
			}
		}

		[Nested]
		public virtual RegistrationInfo Registration { get; set; }

		[OneToOne(Cascade = CascadeEnum.All)]
		public virtual WaybillSource WaybillSource { get; set; }

		[BelongsTo(Cascade = CascadeEnum.All)]
		public virtual Payer Payer { get; set; }

		[BelongsTo("ContactGroupOwnerId", Cascade = CascadeEnum.All)]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }

		[BelongsTo(Cascade = CascadeEnum.All, Lazy = FetchWhen.OnInvoke)]
		public virtual SupplierAccount Account { get; set; }

		[HasMany(Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<Price> Prices { get; set; }

		[HasMany(Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<RegionalData> RegionalData { get; set; }

		[HasMany(Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<OrderSendRules> OrderRules { get; set; }

		[HasMany(Lazy = true, Cascade = ManyRelationCascadeEnum.AllDeleteOrphan)]
		public virtual IList<WaybillExcludeFile> ExcludeFiles { get; set; }

		[HasAndBelongsToMany(typeof(CertificateSource),
			Lazy = true,
			ColumnKey = "SupplierId",
			Table = "SourceSuppliers",
			Schema = "Documents",
			ColumnRef = "CertificateSourceId")]
		public virtual IList<CertificateSource> CertificateSources { get; set; }

		public virtual CertificateSource GetSertificateSource()
		{
			return CertificateSources.FirstOrDefault();
		}

		[Description("Источник сертификатов:")]
		public virtual CertificateSource CertificateSource
		{
			get { return CertificateSources.FirstOrDefault(); }
			set
			{
				CertificateSources.Clear();
				if (value != null)
					CertificateSources.Add(value);
			}
		}

		public virtual IList<User> Users
		{
			get
			{
				if (Id > 0)
					return ActiveRecordLinqBase<User>.Queryable.Where(u => u.RootService == this).ToList();

				return Enumerable.Empty<User>().ToList();
			}
		}

		public virtual IEnumerable<ModelAction> Actions
		{
			get
			{
				return ArHelper.WithSession(s => {
					return new[] {
						new ModelAction(this, "Delete", "Удалить", !CanDelete(s))
					};
				});
			}
		}

		public virtual DateTime GetLastPriceUpdate()
		{
			return Prices
				.SelectMany(p => p.Costs)
				.Where(c => c.PriceItem != null)
				.Select(c => c.PriceItem.PriceDate)
				.DefaultIfEmpty()
				.Max();
		}

		public override string ToString()
		{
			return Name;
		}

		public virtual IEnumerable<IAuditRecord> GetAuditRecords(IEnumerable<AuditableProperty> properties)
		{
			if (properties != null && properties.Any(p => p.Property.Name.Equals("Disabled") || p.Property.Name.Equals("Enabled")))
				return new List<IAuditRecord> {
					new PayerAuditRecord(Payer, "$$$", EditComment) {
						ShowOnlyPayer = true,
						ObjectType = LogObjectType.Supplier,
						ObjectId = Id,
						Name = Name
					},
					new AuditRecord(this) { MessageType = LogMessageType.System, Type = LogObjectType.Supplier, Name = Name }
				};
			return new List<IAuditRecord> { new AuditRecord(this) { MessageType = LogMessageType.System, Type = LogObjectType.Supplier, Name = Name } };
		}

		public virtual bool ShouldNotify()
		{
			return Payer.Id != 921;
		}

		public virtual string GetEmailsForBilling()
		{
			return ContactGroupOwner
				.GetEmails(ContactGroupType.Billing)
				.Implode();
		}

		public virtual void AddBillingComment(string billingMessage)
		{
			if (String.IsNullOrEmpty(billingMessage))
				return;

			new AuditRecord("Сообщение в биллинг: " + billingMessage, this).Save();
			var user = Users.First();
			billingMessage = String.Format("О регистрации поставщика: {0} ( {1} ), пользователь: {2} ( {3} ): {4}", Id, Name, user.Id, user.Name, billingMessage);
			Payer.AddComment(billingMessage);
		}

		public virtual IEnumerable<ContactGroup> GetAditionalContactGroups()
		{
			return _defaultGroups.Select(type => new ContactGroup(type));
		}

		/// <summary>
		/// Получение пути к папке поставщика на ftp
		/// </summary>
		/// <returns></returns>
		public virtual string GetRootPath()
		{
			var root = ConfigurationManager.AppSettings["OptBox"];
			var supplierRoot = Path.Combine(root, Id.ToString().PadLeft(3, '0'));
			return supplierRoot;
		}

		public virtual void CreateDirs()
		{
			var supplierRoot = GetRootPath();
			var dirs = new[] { "Orders", "Waybills", "Reports", "Rejects" };
			try {
				if (!Directory.Exists(supplierRoot))
					Directory.CreateDirectory(supplierRoot);

				foreach (var directoryToCreate in dirs.Select(d => Path.Combine(supplierRoot, d))) {
					if (!Directory.Exists(directoryToCreate))
						Directory.CreateDirectory(directoryToCreate);
				}

				foreach (var user in Users) {
					user.SetFtpAccess();
				}
			}
			catch (Exception e) {
				LogManager.GetLogger(GetType()).Error(String.Format(@"
Ошибка при создании папки на ftp для клиента, иди и создавай руками
Нужно создать папку {0}
А так же создать под папки Orders, Rejects, Waybills, Reports
Дать логинам {1} право читать, писать и получать список директорий и удалять под директории в папке Orders",
					supplierRoot, Users.Implode(u => u.Login)), e);
			}
		}

		/// <summary>
		/// Установка пользователю прав на папку поставщика
		/// </summary>
		/// <param name="username">Логин пользователя</param>
		/// <param name="type">Если true, то даем права. Иначе забираем.</param>
		public virtual void SetFtpAccessControl(string username, bool type = true)
		{
			if (!ADHelper.IsLoginExists(username))
				return;

			if(type)
				AddFtpAccess(username);
			else
				RemoveFtpAccess(username);
		}

		/// <summary>
		/// Добавление прав пользователя к папке поставщика
		/// </summary>
		/// <param name="username">Логин пользователя</param>
		/// <param name="login"></param>
		public virtual void AddFtpAccess(string login)
		{
			var root = GetRootPath();
			var index = 0;
			while (true) {
				try {
#if !DEBUG
					var username = String.Format(@"ANALIT\{0}", login);
					var rootDirectorySecurity = Directory.GetAccessControl(root);
					rootDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
						FileSystemRights.Read,
						InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
						PropagationFlags.None,
						AccessControlType.Allow));
					rootDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
						FileSystemRights.Write,
						InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
						PropagationFlags.None,
						AccessControlType.Allow));
					rootDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
						FileSystemRights.ListDirectory,
						InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
						PropagationFlags.None,
						AccessControlType.Allow));

					Directory.SetAccessControl(root, rootDirectorySecurity);
					var orders = Path.Combine(root, "Orders");
					if (Directory.Exists(orders)) {
						var ordersDirectorySecurity = Directory.GetAccessControl(orders);
						ordersDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
							FileSystemRights.DeleteSubdirectoriesAndFiles,
							InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
							PropagationFlags.None,
							AccessControlType.Allow));
						Directory.SetAccessControl(orders, ordersDirectorySecurity);
					}
#endif
					break;
				}
				catch (Exception e) {
					LogManager.GetLogger(GetType()).Error("Ошибка при назначении прав, пробую еще раз", e);
					index++;
					Thread.Sleep(500);
					if (index > 3)
						break;
				}
			}
		}

		/// <summary>
		/// Удаление прав пользователя к папке поставщика
		/// </summary>
		/// <param name="username">Логин пользователя</param>
		/// <param name="login"></param>
		public virtual void RemoveFtpAccess(string login)
		{
			var root = GetRootPath();
			var index = 0;
			while (true) {
				try {
#if !DEBUG
					var accessControl = Directory.GetAccessControl(root);
					AuthorizationRuleCollection rules = accessControl.GetAccessRules(true, true, typeof(SecurityIdentifier));
					foreach (FileSystemAccessRule rule in rules) {
						var name = rule.IdentityReference.Translate(typeof(NTAccount)).Value;
						if (name.Contains(login))
							accessControl.RemoveAccessRule(rule);
					}
					Directory.SetAccessControl(root, accessControl);

					var orders = Path.Combine(root, "Orders");
					accessControl = Directory.GetAccessControl(orders);
					rules = accessControl.GetAccessRules(true, true, typeof(SecurityIdentifier));
					foreach (FileSystemAccessRule rule in rules) {
						var name = rule.IdentityReference.Translate(typeof(NTAccount)).Value;
						if (name.Contains(login))
							accessControl.RemoveAccessRule(rule);
					}
#endif
					break;
				}
				catch (Exception e) {
					LogManager.GetLogger(GetType()).Error("Ошибка при удалении прав, пробую еще раз", e);
					index++;
					Thread.Sleep(500);
					if (index > 3)
						break;
				}
			}
		}

		public virtual Price AddPrice(string name, PriceType priceType = PriceType.Regular)
		{
			var price = new Price {
				AgencyEnabled = true,
				Enabled = true,
				Name = name,
				Supplier = this,
				PriceType = priceType
			};
			price.AddCost();
			price.RegionalData.Add(new PriceRegionalData(price, HomeRegion));
			Prices.Add(price);
			return price;
		}

		public virtual void AddRegion(Region region, ISession session)
		{
			RegionMask |= region.Id;
			if (RegionalData.All(r => r.Region != region)) {
				var regionalData = new RegionalData(region, this);
				RegionalData.Add(regionalData);
				foreach (var value in Enum.GetValues(typeof(DayOfWeek))) {
					var day = (DayOfWeek)value;
					var reorderSchedule = new ReorderSchedule(regionalData, day);
					reorderSchedule.TimeOfStopsOrders = TimeSpan.FromTicks(684000000000);
					if (day == DayOfWeek.Saturday)
						reorderSchedule.TimeOfStopsOrders = TimeSpan.FromTicks(504000000000);
					if (day == DayOfWeek.Sunday)
						reorderSchedule.TimeOfStopsOrders = TimeSpan.FromTicks(863400000000);
					regionalData.Rules.Add(reorderSchedule);
				}
			}
			foreach (var price in Prices) {
				if (price.RegionalData.All(r => r.Region != region))
					price.RegionalData.Add(new PriceRegionalData(price, region));
			}
		}

		public virtual void ChangePayer(Payer payer)
		{
			var oldPayers = Payer;
			Payer = payer;

			foreach (var user in Users) {
				user.Payer.Users.Remove(user);
				user.Payer = payer;
				user.Payer.Users.Add(user);
			}

			payer.UpdatePaymentSum();
			oldPayers.UpdatePaymentSum();
		}

		public override User AddUser(User user)
		{
			if (!Users.Contains(user))
				Users.Add(user);
			return user;
		}

		public virtual bool CanDelete(ISession session)
		{
			try {
				CheckBeforeDelete(session);
				return true;
			}
			catch (EndUserException e) {
				return false;
			}
		}

		public virtual void CheckBeforeDelete(ISession session)
		{
			if (!Disabled)
				throw new EndUserException(String.Format("Поставщик {0} не отключен", Name));

			var synonymCount = Convert.ToUInt32(session.CreateSQLQuery(@"
select count(*)
from Customers.Suppliers s
	join Usersettings.PricesData pd on pd.FirmCode = s.Id
	join Farm.Synonym s on s.PriceCode = pd.PriceCode
where s.Id = :supplierId")
				.SetParameter("supplierId", Id)
				.UniqueResult());

			if (synonymCount > 200)
				throw new EndUserException(String.Format("У поставщика {0} больше 200 синонимов", Name));
		}

		public virtual void Delete(ISession session)
		{
			//если мы будем удалять плательщика он попробует рекурсивно удалить
			//меня что бы этого избежать обнуляем плательщика
			var payer = Payer;
			Payer = null;

			foreach (var user in Users.ToArray())
				user.Delete();
			session.Delete(this);

			if (payer.CanDelete(session))
				payer.Delete();
		}

		/// <summary>
		/// Регионы, в которых размещены прайс-листы
		/// </summary>
		public virtual List<Region> PricesRegions
		{
			get
			{
				var res = Prices.Where(p => p.Enabled && p.AgencyEnabled)
					.SelectMany(p => p.RegionalData.Where(d => d.Enabled).Select(r => r.Region));
				res = res.Where(r => (r.Id & RegionMask) > 0);
				return res.Distinct().OrderBy(r => r.Name).ToList();
			}
		}

		public virtual void RemoveRegion(Region region)
		{
			RegionMask &= ~region.Id;
		}

		public virtual List<Region> GetRegions(ISession dbSession, Administrator admin)
		{
			return Region.GetRegionsByMask(admin.RegionMask & RegionMask).ToList();
		}

		public virtual void MergePerson(ContactGroupType type, Person person)
		{
			var group = ContactGroupOwner.ContactGroups.FirstOrDefault(g => g.Type == type)
				?? ContactGroupOwner.AddContactGroup(type);

			var exists = group.Persons.FirstOrDefault(p => String.Equals(p.Name, person.Name, StringComparison.CurrentCultureIgnoreCase));
			if (exists == null) {
				group.Persons.Add(person);
				group.Adopt();
			}
			else {
				foreach (var contact in person.Contacts) {
					if (!exists.Contacts.Any(c => c.Type == contact.Type && String.Equals(contact.ContactText, c.ContactText, StringComparison.CurrentCultureIgnoreCase))) {
						exists.AddContact(new Contact(contact.Type, contact.ContactText, contact.Comment));
					}
				}
			}
		}
	}
}