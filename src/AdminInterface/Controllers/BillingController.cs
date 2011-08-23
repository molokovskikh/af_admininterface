using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Castle.Components.Validator;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate;
using SortDirection = Common.Tools.SortDirection;

namespace AdminInterface.Controllers
{
	public class BillingFilter
	{
		private string _tab;

		public string Tab
		{
			get
			{
				return _tab;
			}
			set
			{
				if (String.IsNullOrEmpty(value))
					return;

				_tab = value;
			}
		}

		public uint PayerId { get; set; }
		public uint ServiceId { get; set; }
		public string ActiveTab { get; set; }

		private Service service;
		private Payer payer;

		public BillingFilter()
		{
			Tab = "payments";
		}

		public Payer Payer
		{
			get
			{
				if (payer == null)
					payer = Payer.Find(PayerId);
				return payer;
			}
		}

		public Service Service
		{
			get
			{
				if (service == null && ServiceId != 0)
					service = ActiveRecordMediator<Service>.FindByPrimaryKey(ServiceId);
				return service;
			}
		}

		public string FilterRow(dynamic log)
		{
			var result = new StringBuilder();
			var filterValue = (string)(log.LogType.ToString() + log.ObjectId.ToString());
			result.AppendFormat("data-filter=\"{0}\" ", filterValue);
			if (IsFiltred)
			{
				var currentFilterValue = GetFilterValue(Service);

				if (filterValue.ToLower() != currentFilterValue.ToLower())
					result.AppendFormat("style=\"display:none\"");
			}
			return result.ToString();
		}

		public string GetFilterValue(Service service)
		{
			var currentFilterValue = "Log" + service.Id;
			if (service is Client)
				currentFilterValue = "Client" + currentFilterValue;
			else
				currentFilterValue = "Supplier" + currentFilterValue;
			return currentFilterValue;
		}

		private bool IsFiltred
		{
			get { return Service != null; }
		}

		public Dictionary<string, string> Parts()
		{
			var result = new Dictionary<string, string> {
				{"BillingCode", PayerId.ToString()}
			};
			if (ServiceId != 0)
				result.Add("ClientCode", ServiceId.ToString());
			if (!String.IsNullOrEmpty(ActiveTab))
				result.Add("Tab", ActiveTab);
			return result;
		}
	}

	[
		Helper(typeof (BindingHelper)),
		Helper(typeof (ViewHelper)),
		Secure(PermissionType.Billing),
	]
	public class BillingController : ARController
	{
		public void Edit(uint billingCode,
			uint clientCode,
			string tab,
			uint currentJuridicalOrganizationId)
		{
			var filter = new BillingFilter {
				ServiceId = clientCode,
				PayerId = billingCode,
				Tab = tab
			};
			var payer = filter.Payer;

			var userIds = payer.Users.Select(u => u.Id).ToArray();
			PropertyBag["UsersMessages"] = ActiveRecordLinqBase<UserMessage>.Queryable
				.Where(m => userIds.Contains(m.Id) && m.ShowMessageCount > 0)
				.ToList();

			PropertyBag["filter"] = filter;
			PropertyBag["LogRecords"] = SwitchLogRecord.GetLogs(payer);
			PropertyBag["Instance"] = payer;
			PropertyBag["payer"] = payer;
			PropertyBag["MailSentHistory"] = MailSentEntity.GetHistory(payer);
			PropertyBag["Today"] = DateTime.Today;
			PropertyBag["Recipients"] = Recipient.Queryable.OrderBy(r => r.Name).ToList();

			PropertyBag["suppliers"] = Supplier.Queryable.Where(s => s.Payer == payer).OrderBy(s => s.Name).ToList();
			PropertyBag["clients"] = payer.Clients.OrderBy(c => c.Name).ToList();
			PropertyBag["Users"] = payer.Users;
			PropertyBag["Addresses"] = payer.Addresses;
			PropertyBag["Reports"] = payer.Reports;

			if (currentJuridicalOrganizationId > 0)
				PropertyBag["currentJuridicalOrganizationId"] = currentJuridicalOrganizationId;
		}

		public void Update(
			[ARDataBind("Instance", AutoLoadBehavior.Always)] Payer payer,
			uint clientCode,
			string tab)
		{
			payer.Update();
			Flash.Add("Message", new Message("Изменения сохранены"));
			RedirectToReferrer();
		}

		public void SendMessage([DataBind("NewClientMessage")] UserMessage message,
			uint clientCode,
			uint billingCode,
			string tab,
			bool sendMessageToClientEmails,
			string subjectForEmailToClient)
		{
			try
			{
				User notificationUser = null;
				using (var scope = new TransactionScope(OnDispose.Rollback))
				{
					DbLogHelper.SetupParametersForTriggerLogging();
					if (message.Id != 0)
					{
						var user = User.Find(message.Id);
						notificationUser = user;
						SendMessageToUser(user, message);
					}
					else
					{
						var payer = Payer.Find(billingCode);
						foreach (var user in payer.Users)
						{
							notificationUser = user;
							SendMessageToUser(user, message);
						}
					}
					scope.VoteCommit();
				}
				if (sendMessageToClientEmails)
					Mailer.SendMessageFromBillingToClient(notificationUser, message.Message, subjectForEmailToClient);
				Flash["Message"] = new Message("Сообщение сохранено");
			}
			catch (ValidationException exception)
			{
				Flash["SendError"] = exception.ValidationErrorMessages[0];
			}

			RedirectToReferrer();
		}

		private void SendMessageToUser(User user, UserMessage clientMessage)
		{
			var message = UserMessage.FindUserMessage(user.Id);
			if (message == null)
				return;
			message.Message = clientMessage.Message;
			message.ShowMessageCount = clientMessage.ShowMessageCount;
			message.Update();
		}

		public void UpdateClientStatus(uint clientId, bool enabled)
		{
			using(new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var service = ActiveRecordMediator<Service>.FindByPrimaryKey(clientId);
				var oldDisabled = service.Disabled;
				service.Disabled = !enabled;
				if (oldDisabled != service.Disabled)
				{
					this.Mail().EnableChanged(service).Send();
					ClientInfoLogEntity.StatusChange(service).Save();
				}
				ActiveRecordMediator<Service>.Save(service);
			}
			CancelView();
		}

		public void Search()
		{
			var filter = new PayerFilter();
			if (IsPost || Request.QueryString.Keys.Cast<string>().Any(k => k.StartsWith("filter.")))
			{
				BindObjectInstance(filter, "filter", AutoLoadBehavior.NullIfInvalidKey);
				PropertyBag["searchResults"] = filter.Find();
			}
			PropertyBag["filter"] = filter;
		}

		public void Save([DataBind("filter")] PayerFilter filter,
			[DataBind("PaymentInstances")] PaymentInstance[] paymentInstances)
		{
			foreach (var instance in paymentInstances)
				instance.Save();

			RedirectToAction("Search", filter.GetQueryString());
		}

		public void SentMail(uint clientCode, string tab, [DataBind("MailSentEntity")] MailSentEntity sentEntity)
		{
			try
			{
				sentEntity.UserName = Administrator.UserName;
				sentEntity.SaveAndFlush();
				Flash["Message"] = new Message("Cохранено");
			}
			catch (ValidationException ex)
			{
				Flash["SendMailError"] = ex.ValidationErrorMessages[0];
			}
			Redirect("Billing", "Edit", new {clientCode, billingCode = sentEntity.PayerId, tab});
		}

		public void DeleteMail(uint id)
		{
			var mailSend = MailSentEntity.Find(id);
			mailSend.Delete();
			CancelView();
		}

		public void ShowMessageForUser(uint userId)
		{
			CancelLayout();
			var message = UserMessage.FindUserMessage(userId);
			PropertyBag["Message"] = message;
			PropertyBag["user"] = User.Find(message.Id);
		}

		public void CancelMessage(uint userId)
		{
			using (new TransactionScope())
			{
				var message = UserMessage.Find(userId);
				message.ShowMessageCount = 0;
				message.Update();
			}
			CancelView();
		}

		public void AdditionalUserInfo(uint userId, string cssClassName)
		{
			CancelLayout();
			var user = User.Find(userId);
			PropertyBag["user"] = user;
		}

		public void AdditionalAddressInfo(uint addressId, string cssClassName)
		{
			CancelLayout();
			PropertyBag["address"] = Address.Find(addressId);
		}

		public void SearchUsersForAddress(uint addressId, string searchText)
		{
			if (String.IsNullOrEmpty(searchText))
				searchText = String.Empty;
			var address = Address.Find(addressId);
			var users = address.Client.Users.Where(user =>
				((!String.IsNullOrEmpty(user.Name) && user.Name.ToLower().Contains(searchText.ToLower())) || (user.Login.ToLower().Contains(searchText.ToLower()))) &&
				(!address.AvaliableFor(user)));
			PropertyBag["Users"] = users;
			CancelLayout();
		}

		public void SearchAddressesForUser(uint userId, string searchText)
		{
			if (String.IsNullOrEmpty(searchText))
				searchText = String.Empty;
			var user = User.Find(userId);
			var addresses = user.Client.Addresses.Where(address => 
				address.Value.ToLower().Contains(searchText.ToLower()) &&
				!address.AvaliableFor(user));
			PropertyBag["Addresses"] = addresses;
			CancelLayout();
		}

		public void ConnectUserToAddress(uint userId, uint addressId)
		{
			using (var scope = new TransactionScope())
			{
				var user = User.Find(userId);
				var address = Address.Find(addressId);
				address.AvaliableForUsers.Add(user);
				address.Client.Save();

				scope.VoteCommit();
			}
			CancelView();
			CancelLayout();
		}

		public void DisconnectUserFromAddress(uint userId, uint addressId)
		{
			using (var scope = new TransactionScope(OnDispose.Commit))
			{
				var user = User.Find(userId);
				var address = Address.Find(addressId);
				var client = user.Client;

				address.AvaliableForUsers.Remove(user);
				client.Save();

				scope.VoteCommit();
			}
			CancelView();
			CancelLayout();
		}

		[return: JSONReturnBinder]
		public string TotalSum(uint payerId)
		{
			return Payer.Find(payerId).TotalSum.ToString("C");
		}

		public void Accounting([DataBind("SearchBy")] AccountingSearchProperties searchBy, string tab, uint? pageSize, uint? currentPage, uint? rowsCount)
		{
			if (searchBy.BeginDate == null && searchBy.EndDate == null && searchBy.SearchText == null)
				searchBy = new AccountingSearchProperties();

			if (String.IsNullOrEmpty(tab))
				tab = "unregistredItems";

			var pager = new Pager(currentPage, pageSize, rowsCount.HasValue);
			if (tab.Equals("unregistredItems", StringComparison.CurrentCultureIgnoreCase))
			{
				PropertyBag["unaccountedItems"] = Models.Billing.Accounting.GetReadyForAccounting(pager);
			}
			if (tab.Equals("AccountingHistory", StringComparison.CurrentCultureIgnoreCase))
			{
				var historyItems = AccountingItem
					.SearchBy(searchBy, pager)
					.OrderByDescending(item => item.WriteTime)
					.ToList();
				PropertyBag["accountingHistoryItems"] = historyItems;
			}
			PropertyBag["pageSize"] = pager.PageSize;
			PropertyBag["currentPage"] = pager.Page;
			PropertyBag["rowsCount"] = pager.Total;

			PropertyBag["tab"] = tab;
			PropertyBag["FindBy"] = searchBy;
		}

		public void JuridicalOrganizations(uint payerId, uint currentJuridicalOrganizationId)
		{
			var payer = Payer.Find(payerId);
			PropertyBag["Payer"] = payer;
			PropertyBag["tab"] = "juridicalOrganization";
			PropertyBag["Addresses"] = payer.Addresses;
			if (currentJuridicalOrganizationId > 0)
				PropertyBag["currentJuridicalOrganization"] = LegalEntity.Find(currentJuridicalOrganizationId);
		}

		public void UpdateJuridicalOrganizationInfo([ARDataBind("juridicalOrganization", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] LegalEntity juridicalOrganization)
		{
			using (var scope = new TransactionScope())
			{
				var organization = LegalEntity.Find(juridicalOrganization.Id);
				organization.Name = juridicalOrganization.Name;
				organization.FullName = juridicalOrganization.FullName;

				organization.Update();
				scope.VoteCommit();
				Flash["Message"] = new Message("Сохранено");
				var billingCode = organization.Payer.PayerID;
				Redirect("Billing", "Edit", new { billingCode, tab = "juridicalOrganization", currentJuridicalOrganizationId = organization.Id });
			}
		}

		public void AddJuridicalOrganization([ARDataBind("juridicalOrganization", AutoLoad = AutoLoadBehavior.NewRootInstanceIfInvalidKey)] LegalEntity legalEntity, uint payerId)
		{
			using (var scope = new TransactionScope())
			{
				var payer = Payer.Find(payerId);

				legalEntity.Payer = payer;
				legalEntity.CreateAndFlush();

				Maintainer.LegalEntityCreated(legalEntity);

				scope.VoteCommit();

				Flash["Message"] = new Message("Юридическое лицо создано");
			}
			Redirect("Billing", "Edit", new { billingCode = payerId, tab = "juridicalOrganization", currentJuridicalOrganizationId = legalEntity.Id });
		}

		public void UpdateReport(uint id, bool allow)
		{
			var report = Report.Find(id);
			report.Allow = allow;
			report.Update();
			CancelView();
		}
	}
}
