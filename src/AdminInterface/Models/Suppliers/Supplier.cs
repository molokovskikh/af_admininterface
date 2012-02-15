using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Certificates;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using log4net;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord(Schema = "Future", Lazy = true), Auditable]
	public class Supplier : Service
	{
		private ContactGroupType[] _defaultGroups = new [] {
			ContactGroupType.ClientManagers,
			ContactGroupType.OrderManagers,
			ContactGroupType.MiniMails
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
			Registration = new RegistrationInfo();
			OrderRules = new List<OrderSendRules>();
			Prices = new List<Price>();
			RegionalData = new List<RegionalData>();
			Type = ServiceType.Supplier;
		}

		[JoinedKey("Id")]
		public virtual uint SupplierId { get; set; }

		[Property, ValidateNonEmpty, Auditable, Description("Краткое наименование")]
		public override string Name { get; set; }

		[Property, ValidateNonEmpty, Auditable, Description("Полное наименование")]
		public virtual string FullName { get; set; }

		[Property, Auditable, Description("Регионы работы")]
		public virtual ulong RegionMask { get; set; }

		[BelongsTo, Auditable, Description("Домашний регион")]
		public override Region HomeRegion { get; set; }

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore), Style]
		public override bool Disabled
		{
			get
			{
				return _disabled;
			}
			set
			{
				if (_disabled != value)
				{
					_disabled = value;
					if (Payer != null)
						Payer.UpdatePaymentSum();
				}
			}
		}

		[Property]
		public virtual Segment Segment { get; set; }

		[Nested]
		public virtual RegistrationInfo Registration { get; set;}

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

		[HasAndBelongsToMany(typeof (CertificateSource),
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

		public virtual IList<User> Users
		{
			get
			{
				if (Id > 0)
					return ActiveRecordLinqBase<User>.Queryable.Where(u => u.RootService == this).ToList();

				return Enumerable.Empty<User>().ToList();
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

		public static IList<Supplier> GetByPayerId(uint payerId)
		{
			return Queryable
				.Where(p => p.Payer.PayerID == payerId).OrderBy(s => s.Name)
				.ToList();
		}

		public static IOrderedQueryable<Supplier> Queryable
		{
			get
			{
				return ActiveRecordLinqBase<Supplier>.Queryable;
			}
		}

		public static Supplier Find(uint id)
		{
			return ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
		}

		public virtual void Save()
		{
			ActiveRecordMediator.Save(this);
		}

		public virtual void SaveAndFlush()
		{
			ActiveRecordMediator.Save(this);
		}

		public override string ToString()
		{
			return Name;
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

			new ClientInfoLogEntity("Сообщение в биллинг: " + billingMessage, this).Save();
			var user = Users.First();
			billingMessage = String.Format("О регистрации поставщика: {0} ( {1} ), пользователь: {2} ( {3} ): {4}", Id, Name, user.Id, user.Name, billingMessage);
			Payer.AddComment(billingMessage);
		}

		public virtual void CreateDirs()
		{
			CreateDirs(ConfigurationManager.AppSettings["OptBox"]);
		}

		public virtual IEnumerable<ContactGroup> GetAditionalContactGroups()
		{
			return _defaultGroups.Select(type => new ContactGroup(type));
		}

		public virtual void CreateDirs(string root)
		{
			var supplierRoot = Path.Combine(root, Id.ToString().PadLeft(3, '0'));
			var dirs = new[] { "Orders", "Waybills", "Reports", "Rejects" };
			try
			{
				if (!Directory.Exists(supplierRoot))
					Directory.CreateDirectory(supplierRoot);

				foreach (var directoryToCreate in dirs.Select(d => Path.Combine(supplierRoot, d)))
				{
					if (!Directory.Exists(directoryToCreate))
						Directory.CreateDirectory(directoryToCreate);
				}

				foreach (var user in Users)
					SetAccessControl(user.Login, supplierRoot);
			}
			catch (Exception e)
			{
				LogManager.GetLogger(GetType()).Error(String.Format(@"
Ошибка при создании папки на ftp для клиента, иди и создавай руками
Нужно создать папку {0}
А так же создать под папки Orders, Rejects, Waybills, Reports
Дать логинам {1} право читать, писать и получать список директорий и удалять под директории в папке Orders",
					supplierRoot, Users.Implode(u => u.Login)), e);
			}
		}

		public virtual void SetAccessControl(string username, string root)
		{
			if (!ADHelper.IsLoginExists(username))
				return;

			var index = 0;
			while (true)
			{
				try
				{
#if !DEBUG
					username = String.Format(@"ANALIT\{0}", username);
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
					if (Directory.Exists(orders))
					{
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
				catch(Exception e)
				{
					LogManager.GetLogger(GetType()).Error("Ошибка при назначении прав, пробую еще раз", e);
					index++;
					Thread.Sleep(500);
					if (index > 3)
						break;
				}
			}
		}

		public virtual Price AddPrice(string name, PriceType priceType)
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

		public virtual void AddRegion(Region region)
		{
			RegionMask |= region.Id;
			RegionalData.Add(new RegionalData(region, this));
			foreach (var price in Prices)
				price.RegionalData.Add(new PriceRegionalData(price, region));
		}


		public virtual void ChangePayer(Payer payer)
		{
			var oldPayers = Payer;
			Payer = payer;

			foreach (var user in Users)
			{
				user.Payer.Users.Remove(user);
				user.Payer = payer;
				user.Payer.Users.Add(user);
			}

			payer.UpdatePaymentSum();
			oldPayers.UpdatePaymentSum();
		}
	}

	[ActiveRecord("RegionalData", Schema = "Usersettings")]
	public class RegionalData
	{
		public RegionalData()
		{}

		public RegionalData(Region region, Supplier supplier)
		{
			Region = region;
			Supplier = supplier;
		}

		[PrimaryKey("RowId")]
		public uint Id { get; set; }

		[BelongsTo("RegionCode")]
		public Region Region { get; set; }

		[BelongsTo("FirmCode")]
		public Supplier Supplier { get; set; }
	}
}