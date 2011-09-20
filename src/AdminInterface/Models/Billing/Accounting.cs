using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Common.MySql;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord("Accounting", Schema = "Billing", Lazy = true)]
	public class AccountingItem : ActiveRecordLinqBase<AccountingItem>
	{
		private User _user;
		private Address _address;

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime? WriteTime { get; set; }

		[Property]
		public virtual AccountingItemType Type { get; set; }

		[Property]
		public virtual string Operator { get; set; }

		public virtual Service RootService
		{
			get
			{
				if (User != null)
					return User.RootService;
				if (Address != null)
					return Address.Client;
				return null;
			}
		}

		public virtual Payer Payer
		{
			get
			{
				if (User != null)
					return User.Payer;
				return Address.Payer;
			}
		}

		public virtual User User
		{
			get
			{
				if (Type.Equals(AccountingItemType.User))
				{
					if (_user == null)
						_user = User.Queryable.First(u => u.Accounting.Id == Id);
					return _user;
				}
				return null;
			}
		}

		public virtual Address Address
		{
			get
			{
				if (Type.Equals(AccountingItemType.Address))
				{
					if (_address == null)
						_address = Address.Queryable.First(a => a.Accounting.Id == Id);
					return _address;
				}
				return null;
			}
		}

		public virtual string AccountingName
		{
			get
			{
				if (User != null)
					return User.GetLoginOrName();
				return Address.Value;
			}
		}

		public static IList<AccountingItem> SearchBy(AccountingSearchProperties searchProperties, Pager pager)
		{
			var searchText = String.Format("%{0}%", Utils.StringToMySqlString(searchProperties.SearchText));
			var searchNumber = 0;
			Int32.TryParse(searchProperties.SearchText, out searchNumber);

			var filter = "(date(c.WriteTime) >= :BeginDate and date(c.WriteTime) <= :EndDate) and c.BeAccounted = 1";
			var from = "";
			var where = "";
			var order = "ORDER BY c.WriteTime DESC";
			var limit = String.Format(" LIMIT {0}, {1} ", pager.Page * pager.PageSize, pager.PageSize);
			switch (searchProperties.SearchBy)
			{
				case AccountingSearchBy.ByAddress:
					from = @"
from Billing.Accounting c
	join Future.Addresses a ON a.AccountingId = c.Id AND c.Type = 1";
					where = @"
(a.Address LIKE :SearchText OR a.Id = :SearchNumber)";
					break;
				case AccountingSearchBy.ByClient:
					from = @"
from Billing.Accounting c
	join Future.Users u ON u.AccountingId = c.Id AND c.Type = 0
		join Future.Clients cl ON cl.Id = u.ClientId";
					where = "cl.Name LIKE :SearchText OR cl.FullName LIKE :SearchText OR cl.Id = :SearchNumber";
					break;
				case AccountingSearchBy.ByPayer:
					from = @"
from Billing.Accounting c
	join Future.Users u ON u.AccountingId = c.Id AND c.Type = 0
		join Future.Clients cl ON cl.Id = u.ClientId
			join Billing.PayerClients pc on pc.ClientId = cl.Id
				join Billing.Payers p ON pc.PayerId = p.PayerId";
					where = "p.PayerId = :SearchNumber OR p.ShortName LIKE :SearchText OR p.JuridicalName LIKE :SearchText";
					break;
				case AccountingSearchBy.ByUser:
					from = @"
from Billing.Accounting c
	join Future.Users u ON u.AccountingId = c.Id AND c.Type = 0";
					where = @"
(ifnull(u.Name, '') LIKE :SearchText OR u.Id = :SearchNumber)";
					break;
				case AccountingSearchBy.Auto:
					from = @"
from Billing.Accounting c
	left join Future.Users u ON u.AccountingId = c.Id AND c.Type = 0
		left join Future.Clients  cl ON cl.Id = u.ClientId
			left join Billing.PayerClients pc on pc.ClientId = cl.Id
				left join Billing.Payers p ON pc.PayerId = p.PayerId
	left join Future.Addresses a ON a.AccountingId = c.Id AND c.Type = 1";
					where = @"(
	(u.Id is not null and (ifnull(u.Name, '') LIKE :SearchText OR u.Id = :SearchNumber)) or 
	(a.Address LIKE :SearchText OR a.Id = :SearchNumber) or
	(p.PayerId = :SearchNumber OR p.ShortName LIKE :SearchText OR p.JuridicalName LIKE :SearchText)
)";
					break;
				default:
					return null;
			}

			return ArHelper.WithSession(session => {
				pager.Total = Convert.ToInt32(session.CreateSQLQuery(String.Format(@"
SELECT count(*)
{0}
WHERE {1} and {2}
", from, where, filter))
					.SetParameter("BeginDate", searchProperties.BeginDate.Value)
					.SetParameter("EndDate", searchProperties.EndDate.Value)
					.SetParameter("SearchText", searchText)
					.SetParameter("SearchNumber", searchNumber)
					.UniqueResult());

				return session.CreateSQLQuery(String.Format(@"
SELECT
	c.Id AS {{AccountingItem.Id}},
	c.WriteTime AS {{AccountingItem.WriteTime}},
	c.Type AS {{AccountingItem.Type}},
	c.Operator AS {{AccountingItem.Operator}}
{0}
WHERE {1} and {2}
{3}
{4}
", from, where, filter, order, limit))
					.AddEntity(typeof(AccountingItem))
					.SetParameter("BeginDate", searchProperties.BeginDate.Value)
					.SetParameter("EndDate", searchProperties.EndDate.Value)
					.SetParameter("SearchText", searchText)
					.SetParameter("SearchNumber", searchNumber)
					.List<AccountingItem>();
			});
		}
	}

	public enum AccountingItemType
	{
		[Description("Пользователь")] User = 0,
		[Description("Адрес")] Address = 1,
	}

	[ActiveRecord(DiscriminatorValue = "0")]
	public class UserAccounting : Accounting
	{
		[OneToOne(PropertyRef = "Accounting")]
		public virtual User User { get; set; }

		public override string Type
		{
			get { return "Пользователь"; }
		}

		public UserAccounting(User user)
		{
			Payment = 800;
			User = user;
		}

		public UserAccounting() {}

		public override Service Service
		{
			get
			{
				return User.RootService;
			}
		}

		public override Payer Payer
		{
			get { return User.Payer; }
		}

		public override string Name
		{
			get { return User.Name; }
		}

		public override bool ShouldPay()
		{
			return !User.RootService.Disabled && User.Enabled && base.ShouldPay();
		}
	}

	[ActiveRecord(DiscriminatorValue = "1")]
	public class AddressAccounting : Accounting
	{
		[OneToOne(PropertyRef = "Accounting")]
		public virtual Address Address { get; set; }

		public AddressAccounting(Address address)
		{
			Address = address;
			Payment = 200;
		}

		public override string Type
		{
			get { return "Адрес"; }
		}

		public AddressAccounting() {}

		public override Service Service
		{
			get
			{
				return Address.Client;
			}
		}

		public override Payer Payer
		{
			get { return Address.Payer; }
		}

		public override string Name
		{
			get { return Address.Name; }
		}

		public virtual bool HasPaidUsers
		{
			get
			{
				// Кол-во пользователей, которым доступен этот адрес и которые включены работают НЕ бесплатно, должно быть НЕ нулевым
				return (Address.AvaliableForUsers.Where(user => !user.Accounting.IsFree && user.Enabled).Count() > 0);
			}
		}

		public override bool ShouldPay()
		{
			return Address.Client.Enabled && Address.Enabled && HasPaidUsers && base.ShouldPay();
		}
	}

	[ActiveRecord(DiscriminatorValue = "2")]
	public class ReportAccounting : Accounting
	{
		[BelongsTo("ObjectId")]
		public virtual Report Report { get; set; }

		public override string Type
		{
			get { return "Отчет"; }
		}

		public ReportAccounting() {}

		public override Payer Payer
		{
			get { return Report.Payer; }
		}

		public override string Name
		{
			get { return Report.Comment; }
		}

		public override bool ShouldPay()
		{
			return Report.Allow && base.ShouldPay();
		}
	}

	[ActiveRecord("Accounting", DiscriminatorColumn = "Type", DiscriminatorValue = "10000", Schema = "Billing", Lazy = true), Auditable]
	public abstract class Accounting : ActiveRecordLinqBase<Accounting>, IAuditable
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime? WriteTime { get; set; }

		[Property]
		public virtual string Operator { get; set; }

		[Property, Auditable("Платеж")]
		public virtual decimal Payment { get; set; }

		[Property]
		public virtual bool BeAccounted { get; set;  }

		[Property]
		public virtual bool ReadyForAcounting { get; set; }

		[Property]
		public virtual int InvoiceGroup { get; set; }

		[Property]
		public virtual bool IsFree { get; set; }

		public virtual uint PayerId
		{
			get
			{
				return Payer.Id;
			}
		}

		public abstract Payer Payer { get; }

		public abstract string Name { get;}

		public abstract string Type { get; }

		public virtual Service Service
		{
			get { return null; }
		}

		public virtual void Accounted()
		{
			Operator = SecurityContext.Administrator.UserName;
			WriteTime = DateTime.Now;
			BeAccounted = true;
		}

		public virtual bool ShouldPay()
		{
			return (BeAccounted || ReadyForAcounting) && !IsFree;
		}

		public static IList<Accounting> SearchBy(AccountingSearchProperties filter, Pager pager)
		{
			return pager.DoPage(filter.Apply(Queryable)).ToList();
		}

		public static IEnumerable<Accounting> GetReadyForAccounting(Pager pager)
		{
			var readyForAccounting = Queryable.Where(a => a.ReadyForAcounting && !a.BeAccounted);

			pager.Total = readyForAccounting.Count();
			return pager.DoPage(readyForAccounting).ToList();
		}

		public virtual IAuditRecord GetAuditRecord()
		{
			return new PayerAuditRecord(Payer, this);
		}
	}

	public class Pager
	{
		public int Page;
		public int PageSize = 30;

		public int Total;

		public int TotalPages
		{
			get
			{
				var result = Total / PageSize;
				if (Total % PageSize > 0)
					result++;
				return result;
			}
		}

		public Pager()
		{}

		public Pager(int? page, int pageSize)
		{
			if (page != null)
				Page = page.Value;
			PageSize = pageSize;
		}

		public IEnumerable<T> DoPage<T>(IEnumerable<T> enumerable)
		{
			return enumerable
				.Skip(Page*PageSize)
				.Take(PageSize);
		}
	}
}
