using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using AdminInterface.Helpers;
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

		[Property]
		public override bool Disabled { get; set; }

		[Property]
		public virtual Segment Segment { get; set; }

		[Nested]
		public virtual RegistrationInfo Registration { get; set;}

		[BelongsTo(Cascade = CascadeEnum.All)]
		public virtual Payer Payer { get; set; }

		[BelongsTo("ContactGroupOwnerId", Cascade = CascadeEnum.All)]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }

		[HasMany(Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<Price> Prices { get; set; }

		[HasMany(Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<RegionalData> RegionalData { get; set; }

		[HasMany]
		public virtual IList<OrderSendRules> OrderRules { get; set; }

		public virtual IList<User> Users
		{
			get
			{
				if (Id > 0)
					return User.Queryable.Where(u => u.RootService == this).ToList();

				return Enumerable.Empty<User>().ToList();
			}
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

		public virtual void AddComment(string comment)
		{
			if (String.IsNullOrEmpty(comment))
				return;

			Payer.AddComment(comment);
			new ClientInfoLogEntity(comment, this).Save();
		}

		public virtual void CreateDirs()
		{
			CreateDirs(ConfigurationManager.AppSettings["OptBox"]);
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
	}

	[ActiveRecord("RegionalData", Schema = "Usersettings")]
	public class RegionalData
	{
		[PrimaryKey("RowId")]
		public uint Id { get; set; }

		[BelongsTo("RegionCode")]
		public Region Region { get; set; }

		[BelongsTo("FirmCode")]
		public Supplier Supplier { get; set; }
	}
}