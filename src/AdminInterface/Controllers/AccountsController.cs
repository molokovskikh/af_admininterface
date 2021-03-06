﻿using System;
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
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using Common.Tools;
using NHibernate.Linq;

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
				PropertyBag["unaccountedItems"] = Account.GetReadyForAccounting(pager, DbSession);
			else if (tab.Equals("AccountingHistory", StringComparison.CurrentCultureIgnoreCase))
				PropertyBag["accountingHistoryItems"] = filter.Find(DbSession, pager);

			PropertyBag["currentPage"] = pager.Page;
			PropertyBag["totalPages"] = pager.TotalPages;

			PropertyBag["tab"] = tab;
			PropertyBag["filter"] = filter;
		}

		[return: JSONReturnBinder]
		public object Update(uint id, bool? status, bool? free, bool? accounted, decimal? payment, DateTime? freePeriodEnd, string addComment)
		{
			var account = DbSession.Load<Account>(id);
			//хак будь бдителен, что бы получить объект реального типа и работали проверки
			//account is UserAccount нужно спросить тип объекта у хибера и загружать объект с учетом правильного типа
			account = (Account)DbSession.Load(NHibernateUtil.GetClass(account), id);
			account.Comment = addComment;
			var result = UpdateAccounting(account.Id, accounted, payment, free, freePeriodEnd);
			if (status != null) {
				if (account is UserAccount) {
					var user = ((UserAccount)account).User;
					SetUserStatus(user.Id, status, addComment);
				}
				else if (account is AddressAccount) {
					var address = ((AddressAccount)account).Address;
					SetAddressStatus(address.Id, status, addComment);
				}
				else if (account is SupplierAccount) {
					SetSupplierStatus(((SupplierAccount)account).Supplier, status.Value, addComment);
				}
				else {
					account.Status = status.Value;
				}
			}
			DbSession.Save(account);
			if(freePeriodEnd.HasValue)
				result = new { data = freePeriodEnd.Value.ToShortDateString() };
			return result;
		}

		private void SetSupplierStatus(Supplier supplier, bool status, string comment)
		{
			var oldStatus = supplier.Disabled;
			supplier.Disabled = !status;
			supplier.EditComment = comment;
			DbSession.Save(supplier);
			DbSession.Flush();
			if (oldStatus != !status) {
				Mail().EnableChanged(supplier, comment);
			}
		}

		public void SetUserStatus(uint userId, bool? enabled, string comment)
		{
			var user = DbSession.Load<User>(userId);
			var oldStatus = user.Enabled;
			if (enabled.HasValue)
				user.Enabled = enabled.Value;
			user.EditComment = comment;
			DbSession.Save(user);
			DbSession.Flush();
			if (enabled != oldStatus)
				Mail().EnableChanged(user, comment);
			if (enabled.HasValue && !enabled.Value) {
				// Если это отключение, то проходим по адресам и
				// отключаем адрес, который доступен только отключенным пользователям
				foreach (var address in user.AvaliableAddresses) {
					if (address.AvaliableForEnabledUsers)
						continue;
					address.Enabled = false;
					DbSession.Save(address);
				}
			}
		}

		private object UpdateAccounting(uint accountId, bool? accounted, decimal? payment, bool? isFree, DateTime? freePeriodEnd)
		{
			object result = null;
			var account = DbSession.Load<Account>(accountId);
			//хак будь бдителен, что бы получить объект реального типа и работали проверки
			//account is UserAccount нужно спросить тип объекта у хибера и загружать объект с учетом правильного типа
			account = (Account)DbSession.Load(NHibernateUtil.GetClass(account), accountId);

			if (freePeriodEnd.HasValue)
				account.FreePeriodEnd = freePeriodEnd.Value;

			if (accounted.HasValue) {
				if (accounted.Value)
					account.Accounted();
				else
					account.BeAccounted = false;
			}

			if (isFree.HasValue) {
				IEnumerable<Address> addresses = null;
				if (account is UserAccount) {
					addresses = ((UserAccount)account).User
						.AvaliableAddresses
						.Where(a => a.Accounting.IsFree)
						.ToArray();
				}

				account.IsFree = isFree.Value;

				if (addresses != null && !account.IsFree && addresses.Any()) {
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

			if (payment.HasValue) {
				Admin.CheckPermisions(PermissionType.ChangePayment);
				account.Payment = payment.Value;
			}

			if (account.IsChanged(a => a.Payment))
				Mail().AccountChanged(account);

			DbSession.Save(account);

			return result;
		}

		public void SetAddressStatus(uint addressId, bool? enabled, string comment)
		{
			var address = DbSession.Load<Address>(addressId);
			var oldStatus = address.Enabled;
			if (enabled.HasValue)
				address.Enabled = enabled.Value;
			DbSession.SaveOrUpdate(address.Client);
			DbSession.Flush();
			if (enabled != oldStatus)
				Mail().EnableChanged(address, comment);
			CancelView();
		}

		public void Edit(uint id)
		{
			var account = DbSession.Load<Account>(id);
			if (IsPost) {
				BindObjectInstance(account, "account");
				if (IsValid(account)) {
					DbSession.Save(account);
					Notify("Сохранено");
					RedirectToReferrer();
					return;
				}
			}
			PropertyBag["account"] = account;
		}
	}
}