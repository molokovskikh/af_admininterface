using System;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.NHibernateExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using NHibernate;

namespace AdminInterface.Controllers
{
	[Secure(PermissionType.Billing)]
	public class AccountsController : AdminInterfaceController
	{
		public void Index([DataBind("SearchBy")] AccountFilter searchBy, string tab, uint? currentPage)
		{
			if (searchBy.BeginDate == null && searchBy.EndDate == null && searchBy.SearchText == null)
				searchBy = new AccountFilter();

			if (String.IsNullOrEmpty(tab))
				tab = "unregistredItems";

			var pager = new Pager((int?)currentPage, 30);
			if (tab.Equals("unregistredItems", StringComparison.CurrentCultureIgnoreCase))
			{
				PropertyBag["unaccountedItems"] = Models.Billing.Account.GetReadyForAccounting(pager);
			}
			if (tab.Equals("AccountingHistory", StringComparison.CurrentCultureIgnoreCase))
			{
				var historyItems = searchBy.Find(pager)
					.OrderByDescending(item => item.WriteTime)
					.ToList();
				PropertyBag["accountingHistoryItems"] = historyItems;
			}
			PropertyBag["currentPage"] = pager.Page;
			PropertyBag["totalPages"] = pager.TotalPages;

			PropertyBag["tab"] = tab;
			PropertyBag["FindBy"] = searchBy;
		}

		public void Update(uint id, bool? status, bool? free, bool? accounted, decimal? payment)
		{
			var account = Account.TryFind(id);
			UpdateAccounting(account.Id, accounted, payment, free);
			if (status != null)
			{
				NHibernateUtil.Initialize(account);
				if (account is UserAccount)
				{
					var user = ((UserAccount)account).User;
					SetUserStatus(user.Id, status);
				}
				else if (account is AddressAccount)
				{
					var address = ((AddressAccount)account).Address;
					SetAddressStatus(address.Id, status);
				}
				else
				{
					account.Status = status.Value;
				}
			}
			CancelView();
		}

		public void SetUserStatus(uint userId, bool? enabled)
		{
			var user = User.Find(userId);
			var oldStatus = user.Enabled;
			if (enabled.HasValue)
				user.Enabled = enabled.Value;
			user.UpdateAndFlush();
			if (enabled != oldStatus)
				this.Mailer().EnableChanged(user).Send();
			if (enabled.HasValue && !enabled.Value)
			{
				// Если это отключение, то проходим по адресам и
				// отключаем адрес, который доступен только отключенным пользователям
				foreach (var address in user.AvaliableAddresses)
				{
					if (address.AvaliableForEnabledUsers)
						continue;
					address.Enabled = false;
					address.Update();
				}
			}
			ActiveRecordMediator.Save(user.RootService);
			CancelView();
		}

		public void UpdateAccounting(uint accountId, bool? accounted, decimal? payment, bool? isFree)
		{
			var account = Account.Find(accountId);
			if (accounted.HasValue)
			{
				if (accounted.Value)
					account.Accounted();
				else
					account.BeAccounted = false;
			}

			if (isFree.HasValue)
			{
				account.IsFree = isFree.Value;
			}

			if (payment.HasValue)
			{
				Admin.CheckPermisions(PermissionType.ChangePayment);
				account.Payment = payment.Value;
			}

			if (account.IsChanged(a => a.Payment))
				this.Mailer().AccountingChanged(account).Send();

			account.Update();
			CancelView();
		}

		public void SetAddressStatus(uint addressId, bool? enabled)
		{
			var address = Address.Find(addressId);
			var oldStatus = address.Enabled;
			if (enabled.HasValue)
				address.Enabled = enabled.Value;
			if (enabled != oldStatus)
				this.Mailer().EnableChanged(address).Send();
			address.Client.Save();
			CancelView();
		}

		public void Edit(uint id)
		{
			var account = Account.Find(id);
			if (IsPost)
			{
				BindObjectInstance(account, "account");
				if (IsValid(account))
				{
					account.Save();
					Notify("Сохранено");
					RedirectToReferrer();
					return;
				}
			}
			PropertyBag["account"] = account;
		}
	}
}