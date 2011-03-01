using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using AdminInterface.Models.Security;
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

		public virtual Client Client
		{
			get
			{
				if (User != null)
					return User.Client;
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
			var limitExpression = String.Empty;
			if (pager.ShouldPage)
				limitExpression = String.Format(" LIMIT {0}, {1} ", pager.Page * pager.PageSize, pager.PageSize);

			var filter = "(date(Accounting.WriteTime) >= :BeginDate and date(Accounting.WriteTime) <= :EndDate)  and Accounting.BeAccounted = 1";

			switch (searchProperties.SearchBy)
			{
				case AccountingSearchBy.Auto:
					return AutoSearch(searchProperties, limitExpression, filter);
				case AccountingSearchBy.ByAddress:
					return SearchByAddress(searchProperties, limitExpression, filter);
				case AccountingSearchBy.ByClient:
					return SearchByClient(searchProperties, limitExpression, filter);
				case AccountingSearchBy.ByPayer:
					return SearchByPayer(searchProperties, limitExpression, filter);
				case AccountingSearchBy.ByUser:
					return SearchByUser(searchProperties, limitExpression, filter);
			}

			return null;
		}

		public static IList<AccountingItem> AutoSearch(AccountingSearchProperties searchProperties, string limitExpression, string filter)
		{
			var result = new List<AccountingItem>();

			result.AddRange(SearchByAddress(searchProperties, limitExpression, filter));
			result.AddRange(SearchByUser(searchProperties, limitExpression, filter));
			result.AddRange(SearchByPayer(searchProperties, limitExpression, filter));
			return result;
		}

		public static IList<AccountingItem> SearchByAddress(AccountingSearchProperties searchProperties, string limitExpression, string filter)
		{
			var searchText = String.Format("%{0}%", Utils.StringToMySqlString(searchProperties.SearchText));
			var searchNumber = 0;
			Int32.TryParse(searchProperties.SearchText, out searchNumber);

			return ArHelper.WithSession(session => session.CreateSQLQuery(String.Format(@"
SELECT
	Accounting.Id AS {{AccountingItem.Id}},
	Accounting.WriteTime AS {{AccountingItem.WriteTime}},
	Accounting.Type AS {{AccountingItem.Type}},
	Accounting.Operator AS {{AccountingItem.Operator}}
FROM Billing.Accounting
	JOIN Future.Addresses ON Addresses.AccountingId = Accounting.Id AND Accounting.Type = 1 AND
		(Addresses.Address LIKE :SearchText OR Addresses.Id = :SearchNumber)
WHERE {1}
ORDER BY {{AccountingItem.WriteTime}} DESC
{0}
", limitExpression, filter))
					.AddEntity(typeof(AccountingItem))
					.SetParameter("BeginDate", searchProperties.BeginDate.Value)
					.SetParameter("EndDate", searchProperties.EndDate.Value)
					.SetParameter("SearchText", searchText)
					.SetParameter("SearchNumber", searchNumber)
					.List<AccountingItem>());
		}

		public static IList<AccountingItem> SearchByUser(AccountingSearchProperties searchProperties, string limitExpression, string filter)
		{
			var searchText = String.Format("%{0}%", Utils.StringToMySqlString(searchProperties.SearchText));
			var searchNumber = 0;
			Int32.TryParse(searchProperties.SearchText, out searchNumber);

			return ArHelper.WithSession(session => session.CreateSQLQuery(String.Format(@"
SELECT
	Accounting.Id AS {{AccountingItem.Id}},
	Accounting.WriteTime AS {{AccountingItem.WriteTime}},
	Accounting.Type AS {{AccountingItem.Type}},
	Accounting.Operator AS {{AccountingItem.Operator}}
FROM Billing.Accounting
	JOIN Future.Users ON Users.AccountingId = Accounting.Id AND Accounting.Type = 0 AND
		(Users.Name LIKE :SearchText OR Users.Id = :SearchNumber)
WHERE {1}
ORDER BY {{AccountingItem.WriteTime}} DESC
{0}
", limitExpression, filter))
					.AddEntity(typeof(AccountingItem))
					.SetParameter("BeginDate", searchProperties.BeginDate.Value)
					.SetParameter("EndDate", searchProperties.EndDate.Value)
					.SetParameter("SearchText", searchText)
					.SetParameter("SearchNumber", searchNumber)
					.List<AccountingItem>());
		}

		public static IList<AccountingItem> SearchByClient(AccountingSearchProperties searchProperties, string limitExpression, string filter)
		{
			var searchText = String.Format("%{0}%", Utils.StringToMySqlString(searchProperties.SearchText));
			var searchNumber = 0;
			Int32.TryParse(searchProperties.SearchText, out searchNumber);

			return ArHelper.WithSession(session => session.CreateSQLQuery(String.Format(@"
SELECT
	Accounting.Id AS {{AccountingItem.Id}},
	Accounting.WriteTime AS {{AccountingItem.WriteTime}},
	Accounting.Type AS {{AccountingItem.Type}},
	Accounting.Operator AS {{AccountingItem.Operator}}
FROM Billing.Accounting
	JOIN Future.Users ON Users.AccountingId = Accounting.Id AND Accounting.Type = 0
	JOIN Future.Clients ON Clients.Id = Users.ClientId AND
		(Clients.Name LIKE :SearchText OR Clients.FullName LIKE :SearchText OR Clients.Id = :SearchNumber)
WHERE {1}
ORDER BY {{AccountingItem.WriteTime}} DESC
{0}
", limitExpression, filter))
				.AddEntity(typeof(AccountingItem))
				.SetParameter("BeginDate", searchProperties.BeginDate.Value)
				.SetParameter("EndDate", searchProperties.EndDate.Value)
				.SetParameter("SearchText", searchText)
				.SetParameter("SearchNumber", searchNumber)
				.List<AccountingItem>());
		}

		public static IList<AccountingItem> SearchByPayer(AccountingSearchProperties searchProperties, string limitExpression, string filter)
		{
			var searchText = String.Format("%{0}%", Utils.StringToMySqlString(searchProperties.SearchText));
			var searchNumber = 0;
			Int32.TryParse(searchProperties.SearchText, out searchNumber);

			return ArHelper.WithSession(session => session.CreateSQLQuery(String.Format(@"
SELECT
	Accounting.Id AS {{AccountingItem.Id}},
	Accounting.WriteTime AS {{AccountingItem.WriteTime}},
	Accounting.Type AS {{AccountingItem.Type}},
	Accounting.Operator AS {{AccountingItem.Operator}}
FROM Billing.Accounting
	JOIN Future.Users ON Users.AccountingId = Accounting.Id AND Accounting.Type = 0
	JOIN Future.Clients ON Clients.Id = Users.ClientId
	JOIN Billing.Payers ON Clients.PayerId = Payers.PayerId AND
		(Payers.PayerId = :SearchNumber OR Payers.ShortName LIKE :SearchText OR Payers.JuridicalName LIKE :SearchText)
WHERE {1}
ORDER BY {{AccountingItem.WriteTime}} DESC
{0}
", limitExpression, filter))
				.AddEntity(typeof(AccountingItem))
				.SetParameter("BeginDate", searchProperties.BeginDate.Value)
				.SetParameter("EndDate", searchProperties.EndDate.Value)
				.SetParameter("SearchText", searchText)
				.SetParameter("SearchNumber", searchNumber)
				.List<AccountingItem>());
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

		public override bool ShouldPay()
		{
			return User.Client.Enabled && User.Enabled && !User.IsFree && (ReadyForAcounting || BeAccounted);
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

		public AddressAccounting() { }

		public override bool ShouldPay()
		{
			return Address.Client.Enabled && Address.Enabled && !Address.FreeFlag && (ReadyForAcounting || BeAccounted);
		}
	}

	[ActiveRecord("Accounting", DiscriminatorColumn = "Type", DiscriminatorValue = "3", Schema = "Billing", Lazy = true)]
	public abstract class Accounting : ActiveRecordLinqBase<Accounting>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime? WriteTime { get; set; }

		[Property]
		public virtual string Operator { get; set; }

		[Property]
		public virtual decimal Payment { get; set; }

		[Property(NotNull = true, Default = "0")]
		public virtual bool BeAccounted { get; set;  }

		[Property(NotNull = true, Default = "0")]
		public virtual bool ReadyForAcounting { get; set; }

		public virtual uint PayerId
		{
			get
			{
				if (this is UserAccounting)
					return ((UserAccounting) this).User.Payer.Id;
				return ((AddressAccounting) this).Address.Payer.Id;
			}
		}

		public virtual string Name
		{
			get
			{
				if (this is UserAccounting)
					return ((UserAccounting) this).User.GetLoginOrName();
				return ((AddressAccounting) this).Address.Value;
			}
		}

		public virtual Payer Payer
		{
			get
			{
				if (this is UserAccounting)
					return ((UserAccounting) this).User.Payer;
				return ((AddressAccounting) this).Address.Payer;
			}
		}

		public virtual void Accounted()
		{
			Operator = SecurityContext.Administrator.UserName;
			WriteTime = DateTime.Now;
			BeAccounted = true;
		}

		public abstract bool ShouldPay();
		public abstract string Type { get; }

		public static IList<Accounting> SearchBy(AccountingSearchProperties filter, Pager pager)
		{
			return filter.Apply(Queryable).Page(pager).ToList();
		}

		public static IEnumerable<Accounting> GetReadyForAccounting(Pager pager)
		{
			var readyForAccounting = Queryable.Where(a => a.ReadyForAcounting && !a.BeAccounted);

			pager.Total = readyForAccounting.Count();

			var accounts = readyForAccounting
				.Page(pager)
				.ToList();
			return accounts;
		}
	}

	public class Pager
	{
		public int Page;
		public int PageSize = 30;

		public int Total;

		public bool ShouldPage;

		public Pager()
		{}

		public Pager(uint? page, uint? pageSize, bool shouldPage)
		{
			Page = (int) (page ?? 0);
			PageSize = (int) (pageSize ?? 30);
			ShouldPage = shouldPage;
		}
	}

	public static class PagerExtentions
	{
		public static IEnumerable<T> Page<T>(this IEnumerable<T> enumerable, Pager pager)
		{
			return enumerable
				.Skip(pager.Page*pager.PageSize)
				.Take(pager.PageSize);
		}
	}
}
