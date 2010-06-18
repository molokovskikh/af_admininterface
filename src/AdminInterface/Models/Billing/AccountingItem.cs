using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Billing
{
    public enum AccountingItemType
    {
        [Description("Пользователь")] User = 0,
        [Description("Адрес")] Address = 1,
    }

	[ActiveRecord]
	public class AccountingObject : ActiveRecordBase
	{
		[PrimaryKey]
		public virtual uint Id { get; set;}

		[Property]
		public uint PayerId { get; set; }

		[Property]
		public string PayerName { get; set; }

		[Property]
		public string Type { get; set; }

		[Property]
		public string Name { get; set; }
	}

	[ActiveRecord("Accounting", Schema = "Billing", Lazy = true)]
    public class AccountingItem : ActiveRecordLinqBase<AccountingItem>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
        public virtual DateTime? WriteTime { get; set; }

		[Property]
        public virtual AccountingItemType Type { get; set; }

		[Property]
		public virtual uint AccountId { get; set; }

		[Property]
		public virtual string Operator { get; set; }

		public virtual User User 
		{
			get
			{
				if (Type.Equals(AccountingItemType.User))
					return User.Find(AccountId);
				return null;
			}
		}

		public virtual Address Address
		{
			get
			{
				if (Type.Equals(AccountingItemType.Address))
					return Address.Find(AccountId);
				return null;
			}
		}

		private static IList<AccountingObject> GetUnaccountedUsers(uint page, uint pageSize, bool usePaging)
		{
			var limitExpression = String.Empty;
			if (usePaging)
				limitExpression = String.Format(" LIMIT {0},{1} ", page * (pageSize), pageSize);
			var userIds = ArHelper.WithSession(session => session.CreateSQLQuery(String.Format(@"
SELECT
	Users.Id AS {{AccountingObject.Id}},
	Payers.PayerId AS {{AccountingObject.PayerId}},
	Payers.ShortName AS {{AccountingObject.PayerName}},
	'Пользователь' AS {{AccountingObject.Type}},
	concat(Users.Login, ' (', Users.Name, ')') AS {{AccountingObject.Name}}
FROM
	future.Users
JOIN Future.Clients ON Clients.Id = Users.ClientId
JOIN Billing.Payers ON Payers.PayerID = Clients.PayerId
WHERE Users.Enabled = 1 and Users.Free = 0 and NOT EXISTS (
	SELECT * FROM Billing.Accounting
	WHERE Users.Id = Accounting.AccountId and Type = :Type
)
ORDER BY {{AccountingObject.PayerId}} DESC
{0} ", limitExpression))
								.AddEntity(typeof(AccountingObject))
								.SetParameter("Type", AccountingItemType.User)
								.List<AccountingObject>());
			return userIds;
		}

		private static IList<AccountingObject> GetUnaccountedAddresses(uint page, uint pageSize, bool usePaging)
		{
			var limitExpression = String.Empty;
			if (usePaging)
				limitExpression = String.Format(" LIMIT {0},{1} ", page * (pageSize), pageSize);
			var addressIds = ArHelper.WithSession(session => session.CreateSQLQuery(String.Format(@"
SELECT 
	Addresses.Id AS {{AccountingObject.Id}},
	Payers.PayerId AS {{AccountingObject.PayerId}},
	Payers.ShortName AS {{AccountingObject.PayerName}},
	'Адрес' AS {{AccountingObject.Type}},
	Addresses.Address AS {{AccountingObject.Name}}
FROM
	future.Addresses
JOIN Future.Clients ON Clients.Id = Addresses.ClientId
JOIN Billing.Payers ON Payers.PayerID = Clients.PayerId
WHERE Addresses.Enabled = 1 and Addresses.Free = 0 and Addresses.BeAccounted = 1 and NOT EXISTS (
	SELECT * FROM Billing.Accounting
	WHERE Addresses.Id = Accounting.AccountId and Type = :Type
)
ORDER BY {{AccountingObject.PayerId}} DESC
{0} ", limitExpression))
					.AddEntity(typeof(AccountingObject))
					.SetParameter("Type", AccountingItemType.Address)
					.List<AccountingObject>());
			return addressIds;
		}

		public static IList<AccountingObject> GetUnaccountedObjects(uint page, uint pageSize, bool usePaging)
		{
			var objects = new List<AccountingObject>();
			
			// Делим на 2 потому что отдавать будем список в 2 раза длиннее (адреса + пользователи)
			var count = pageSize / 2;
			var addresses = GetUnaccountedAddresses(page, count, usePaging);

			// Если выбрали меньше чем нужно было, остальное будем добирать пользователями
			if (addresses.Count < count)
				count = pageSize / 2 + (uint)(pageSize / 2 - addresses.Count);

			var users = GetUnaccountedUsers(page, count, usePaging);

			objects.AddRange(addresses);
			objects.AddRange(users);

			return objects;
		}

		public static IList<AccountingItem> SearchByPeriod(DateTime beginDate, DateTime endDate, uint page, uint pageSize, bool usePaging)
		{
			var limitExpression = String.Empty;
			if (usePaging)
				limitExpression = String.Format(" LIMIT {0}, {1} ", page * pageSize, pageSize);
			return ArHelper.WithSession(session => session.CreateSQLQuery(String.Format(@"
SELECT
	Accounting.Id AS {{AccountingItem.Id}},
	Accounting.WriteTime AS {{AccountingItem.WriteTime}},
	Accounting.Type AS {{AccountingItem.Type}},
	Accounting.AccountId AS {{AccountingItem.AccountId}},
	Accounting.Operator AS {{AccountingItem.Operator}}
FROM Billing.Accounting
JOIN Future.Users ON Users.Id = Accounting.AccountId AND Accounting.Type = 0
WHERE Accounting.WriteTime > :BeginDate AND Accounting.WriteTime < :EndDate
UNION
SELECT
	Accounting.Id AS {{AccountingItem.Id}},
	Accounting.WriteTime AS {{AccountingItem.WriteTime}},
	Accounting.Type AS {{AccountingItem.Type}},
	Accounting.AccountId AS {{AccountingItem.AccountId}},
	Accounting.Operator AS {{AccountingItem.Operator}}
FROM Billing.Accounting
JOIN Future.Addresses ON Addresses.Id = Accounting.AccountId AND Accounting.Type = 1
WHERE Accounting.WriteTime > :BeginDate AND Accounting.WriteTime < :EndDate
ORDER BY {{AccountingItem.WriteTime}} DESC
{0}
", limitExpression))
				.AddEntity(typeof(AccountingItem))
				.SetParameter("BeginDate", beginDate)
				.SetParameter("EndDate", endDate)
				.List<AccountingItem>());
		}

		public static IList<AccountingItem> Union(IList<User> users, IList<Address> addresses)
		{
			var unionList = new List<AccountingItem>();

			unionList.AddRange(users.Select(user => new AccountingItem {
                Type= AccountingItemType.User,
                AccountId = user.Id,
			}));

			unionList.AddRange(addresses.Select(address => new AccountingItem {
                Type = AccountingItemType.Address,
				AccountId = address.Id,
			}));

			return unionList;
		}

        public static AccountingItem GetByUser(User user)
        {
            var id = ArHelper.WithSession(session => session.CreateSQLQuery(@"
SELECT Id FROM Billing.Accounting WHERE AccountId = :Id AND Type = :Type
").SetParameter("Id", user.Id).SetParameter("Type", AccountingItemType.User).UniqueResult());

            return TryFind(Convert.ToUInt32(id));
        }

        public static AccountingItem GetByAddress(Address address)
        {
            var id = ArHelper.WithSession(session => session.CreateSQLQuery(@"
SELECT Id FROM Billing.Accounting WHERE AccountId = :Id AND Type = :Type
").SetParameter("Id", address.Id).SetParameter("Type", AccountingItemType.Address).UniqueResult());

            return TryFind(Convert.ToUInt32(id));
        }
    }
}
