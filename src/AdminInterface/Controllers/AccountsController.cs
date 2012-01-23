using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.NHibernateExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using NHibernate;
using Common.Tools;

namespace AdminInterface.Controllers
{
	[Secure(PermissionType.Billing)]
	public class AccountsController : AdminInterfaceController
	{
		public void Index([DataBind("filter")] AccountFilter filter, string tab, uint? currentPage)
		{
			if (String.IsNullOrEmpty(tab))
				tab = "unregistredItems";

			var pager = new Pager((int?)currentPage, 30);
			if (tab.Equals("unregistredItems", StringComparison.CurrentCultureIgnoreCase))
				PropertyBag["unaccountedItems"] = Account.GetReadyForAccounting(pager);
			else if (tab.Equals("AccountingHistory", StringComparison.CurrentCultureIgnoreCase))
				PropertyBag["accountingHistoryItems"] = filter.Find(pager);

			PropertyBag["currentPage"] = pager.Page;
			PropertyBag["totalPages"] = pager.TotalPages;

			PropertyBag["tab"] = tab;
			PropertyBag["filter"] = filter;
		}

		[return: JSONReturnBinder]
		public object Update(uint id, bool? status, bool? free, bool? accounted, decimal? payment)
		{
			var account = Account.TryFind(id);
			var result = UpdateAccounting(account.Id, accounted, payment, free);
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
				else if (account is SupplierAccount)
				{
					SetSupplierStatus(((SupplierAccount)account).Supplier, status.Value);
				}
				else
				{
					account.Status = status.Value;
				}
			}
			return result;
		}

		private void SetSupplierStatus(Supplier supplier, bool status)
		{
			supplier.Disabled = !status;
			if (supplier.IsChanged(s => s.Disabled))
			{
				this.Mailer().EnableChanged(supplier).Send();
				ClientInfoLogEntity.StatusChange(supplier).Save();
			}
			ActiveRecordMediator.Save(supplier);
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
		}

		private object UpdateAccounting(uint accountId, bool? accounted, decimal? payment, bool? isFree)
		{
			object result = null;
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
				IEnumerable<Address> addresses = null;
				if (account is UserAccount)
				{
					addresses = ((UserAccount) account).User
						.AvaliableAddresses
						.Where(a => a.Accounting.IsFree)
						.ToArray();
				}

				account.IsFree = isFree.Value;
				
				if (addresses != null && !account.IsFree && addresses.Count() > 0)
				{
					foreach (var address in addresses)
						address.Accounting.IsFree = isFree.Value;
					result = new {
						accounts = addresses.Select(a => new {
							id = a.Accounting.Id,
							free = a.Accounting.IsFree
						}).ToArray(),
						message = String.Format("Следующие адреса доставки стали платными: {0}", addresses.Implode(a => a.Value))
					};
				}
			}

			if (payment.HasValue)
			{
				Admin.CheckPermisions(PermissionType.ChangePayment);
				account.Payment = payment.Value;
			}

			if (account.IsChanged(a => a.Payment))
				this.Mailer().AccountingChanged(account).Send();

			account.Update();
			return result;
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