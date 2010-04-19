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

	public class AccountingObject
	{
		public uint PayerId { get; set; }
		public string PayerName { get; set; }
		public string Type { get; set; }
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

		public static IList<AccountingItem> GetUnaccounted()
		{
			var userIds = ArHelper.WithSession(session => session.CreateSQLQuery(@"
SELECT
	Users.Id
FROM
	future.Users
WHERE Users.Enabled = 1 and Users.Free = 0 and NOT EXISTS (
    SELECT * FROM Billing.Accounting
    WHERE Users.Id = Accounting.AccountId and Type = :Type
)")
								.SetParameter("Type", AccountingItemType.User)
								.List());
		    var accountingItems = new List<AccountingItem>();
            foreach (uint id in userIds)
            {
                accountingItems.Add(new AccountingItem() {
                    Type = AccountingItemType.User,
                    AccountId = id,
                });
            }
		    var addressIds = ArHelper.WithSession(session => session.CreateSQLQuery(@"
SELECT 
    Addresses.Id
FROM
    future.Addresses
WHERE Addresses.Enabled = 1 and Addresses.Free = 0 and Addresses.BeAccounted = 1 and NOT EXISTS (
    SELECT * FROM Billing.Accounting
    WHERE Addresses.Id = Accounting.AccountId and Type = :Type
)")
                                .SetParameter("Type", AccountingItemType.Address)
                                .List());            
            foreach (uint id in addressIds)
            {
                var address = Address.Find(id);
                if (address.IsFree)
                    continue;
                accountingItems.Add(new AccountingItem() {
                    Type = AccountingItemType.Address,
                    AccountId = id,
                });
            }
			return accountingItems;
		}

		public static IList<AccountingObject> GetUnaccountedObjects()
		{
			var items = GetUnaccounted();
			var objects = new List<AccountingObject>();

			foreach (var item in items)
			{
				if (item.Type.Equals(AccountingItemType.Address))
					objects.Add(new AccountingObject {
						Type = "Адрес",
						Name = item.Address.Value,
						PayerId = item.Address.Client.BillingInstance.PayerID,
						PayerName = item.Address.Client.BillingInstance.ShortName,
					});
				if (item.Type.Equals(AccountingItemType.User))
					objects.Add(new AccountingObject {
						Type = "Пользователь",
						Name = item.User.GetLoginWithName(),
						PayerId = item.User.Client.BillingInstance.PayerID,
						PayerName = item.User.Client.BillingInstance.ShortName,
					});
			}
			return objects;
		}

		public static IList<AccountingItem> SearchByPeriod(DateTime beginDate, DateTime endDate)
		{
			return FindAll().Where(item => item.WriteTime.HasValue &&
                item.WriteTime.Value.CompareTo(beginDate) >= 0 &&
                item.WriteTime.Value.CompareTo(endDate) < 0).ToList();
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
