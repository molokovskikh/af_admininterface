using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Layout("billing"), 
		Helper(typeof (BindingHelper)), 
		Helper(typeof (ViewHelper)),
		Secure(PermissionType.Billing, ExecutionOrder = 0),
		Secure(PermissionType.ViewSuppliers, PermissionType.ViewDrugstore, Required = Required.AnyOf, ExecutionOrder = 1)
	]
	public class BillingController : ARSmartDispatcherController
	{
		[AccessibleThrough(Verb.Get)]
		public void Edit(uint billingCode, string tab, uint currentJuridicalOrganizationId)
		{
			Edit(billingCode, 0, 0, 0, false, false, false, tab, null, null, currentJuridicalOrganizationId);
		}

		public void Edit(uint billingCode,
			uint clientCode,
			uint userId,
			uint addressId,
			bool showClients,
			bool showAddresses,
			bool showUsers,
			string tab,
			DateTime? paymentsFrom,
			DateTime? paymentsTo,
			uint currentJuridicalOrganizationId)
		{
			Client client;

			if (billingCode == 0)
				client = Client.Find(clientCode);
			else
				client = Payer.Find(billingCode).Clients.First();

			var user = (userId != 0) ? User.Find(userId) : client.Users.FirstOrDefault();
			var address = (addressId != 0) ? Address.Find(addressId) : client.Addresses.FirstOrDefault();

			var payer = client.BillingInstance;
			var usersMessages = new List<ClientMessage>();
			var usersLogs = new List<UserLogRecord>();
			var addressesLogs = new List<AddressLogRecord>();
			var countUsersWithMessages = 0;
			foreach (var item in payer.GetAllUsers())
			{
				var message = ClientMessage.FindUserMessage(item.Id);
				if ((message != null) && message.IsContainsNotShowedMessage())
					countUsersWithMessages++;
				usersMessages.Add(message);
				usersLogs.AddRange(UserLogRecord.GetUserEnabledLogRecords(item));
			}
			foreach (var item in payer.GetAllAddresses())
				addressesLogs.AddRange(AddressLogRecord.GetAddressLogRecords(item));
			PropertyBag["CountUsersWithMessages"] = countUsersWithMessages;
			PropertyBag["UsersMessages"] = usersMessages;
			PropertyBag["ShowClients"] = showClients;

			if (String.IsNullOrEmpty(tab))
				tab = "payments";
			
			PropertyBag["tab"] = tab;
			PropertyBag["paymentsFrom"] = paymentsFrom ?? payer.DefaultBeginPeriod();
			PropertyBag["paymentsTo"] = paymentsTo ?? payer.DefaultEndPeriod();
			var clientLogs = ClientLogRecord.GetClientLogRecords(client);
			var userLogs = usersLogs.OrderByDescending(logRecord => logRecord.LogTime).ToList();
			var addressLogs = addressesLogs.OrderByDescending(logRecord => logRecord.LogTime).ToList();
			PropertyBag["LogRecords"] = SwitchLogRecord.GetUnionLogs(clientLogs, userLogs, addressLogs);

			PropertyBag["Client"] = client;
			PropertyBag["Instance"] = payer;
			PropertyBag["User"] = user;
			PropertyBag["Address"] = address;
			PropertyBag["Tariffs"] = Tariff.FindAll();
			PropertyBag["Payments"] = Payment.FindCharges(payer, paymentsFrom, paymentsTo);
			PropertyBag["MailSentHistory"] = MailSentEntity.GetHistory(payer.PayerID);
			PropertyBag["Today"] = DateTime.Today;
			PropertyBag["TotalSum"] = payer.TotalSum;
			PropertyBag["Recipients"] = Recipient.Queryable.OrderBy(r => r.Name).ToList();

			PropertyBag["Users"] = payer.GetAllUsers();
			PropertyBag["Addresses"] = payer.GetAllAddresses();

			if (currentJuridicalOrganizationId > 0)
				PropertyBag["currentJuridicalOrganizationId"] = currentJuridicalOrganizationId;
		}

		public void Update(
			[ARDataBind("Instance", AutoLoadBehavior.Always)] Payer billingInstance,
			uint clientCode,
			string tab)
		{
			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging<User>(SecurityContext.Administrator.UserName,
					HttpContext.Current.Request.UserHostAddress);
				billingInstance.UpdateAndFlush();
				Flash.Add("Message", new Message("Изменения сохранены"));
				Redirect("Billing", "Edit", new {clientCode, tab});
			}
		}

		public void SendMessage([DataBind("NewClientMessage")] ClientMessage clientMessage, uint clientId,
			bool sendMessageToClientEmails, string subjectForEmailToClient)
		{
			uint clientCode = 0;
			try
			{
				Client client = null;
				using (var scope = new TransactionScope())
				{
					DbLogHelper.SetupParametersForTriggerLogging<User>(SecurityContext.Administrator.UserName,
						HttpContext.Current.Request.UserHostAddress);
					if (clientMessage.ClientCode == 0)
						foreach (var user in Client.Find(clientId).Users)
						{
							SendMessageToUser(user, clientMessage);
							clientCode = user.Client.Id;
							client = user.Client;
						}
					else
					{
						var user = User.Find(clientMessage.ClientCode);
						SendMessageToUser(user, clientMessage);
						clientCode = user.Client.Id;
						client = user.Client;
					}
				}
				if (sendMessageToClientEmails)
					Mailer.SendMessageFromBillingToClient(client, clientMessage.Message, subjectForEmailToClient);
				Flash.Add("Message", new Message("Сообщение сохранено"));
			}
			catch (ValidationException exception)
			{
				Flash.Add("SendError", exception.ValidationErrorMessages[0]);
			}

			Redirect("Billing", "Edit", new {clientCode, tab = "payments"});
		}

		private void SendMessageToUser(User user, ClientMessage clientMessage)
		{
			DbLogHelper.SetupParametersForTriggerLogging<ClientMessage>(SecurityContext.Administrator.UserName,
																		Request.UserHostAddress);
			var message = ClientMessage.Find(user.Id);
			message.Message = clientMessage.Message;
			message.ShowMessageCount = clientMessage.ShowMessageCount;
			message.Update();
		}

		public void UpdateClientStatus(uint clientId, bool enabled)
		{
			using(new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging<ClientWithStatus>(SecurityContext.Administrator.UserName,
																			   Request.UserHostAddress);
				var newStatus = enabled ? ClientStatus.On : ClientStatus.Off;
				var client = Client.Find(clientId);
				if (newStatus == ClientStatus.On && client.Status == ClientStatus.Off)
					Mailer.ClientBackToWork(client);
				if (client.Status != newStatus)
					ClientInfoLogEntity.StatusChange(newStatus, client.Id).Save();
				client.Status = newStatus;
				client.UpdateAndFlush();
			}
			CancelView();
			CancelLayout();
		}

		public void SetUserStatus(uint userId, bool enabled, bool free)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging<User>(SecurityContext.Administrator.UserName,
					HttpContext.Current.Request.UserHostAddress);
				var user = User.Find(userId);
				var oldStatus = user.Enabled;
				user.Enabled = enabled;
				user.IsFree = free;
				user.UpdateAndFlush();
				if (enabled && !oldStatus)
					Mailer.UserBackToWork(user);
				if (!enabled)
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
				user.Client.UpdateBeAccounted();
				user.Client.Save();
				scope.VoteCommit();
			}
			CancelView();
			CancelLayout();
		}

		public void UserAccounting(uint userId, bool accounted)
		{
			var user = User.Find(userId);
			if (accounted)
				user.RegisterInBilling();
			else
				user.UnregisterInBilling();
			CancelView();
			CancelLayout();
		}

		public void AddressAccounting(uint addressId, bool accounted)
		{
			var address = Address.Find(addressId);
			if (accounted)
				address.RegisterInBilling();
			else
				address.UnregisterInBilling();
			CancelView();
			CancelLayout();
		}

		public void SetAddressStatus(uint addressId, bool enabled, bool free)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var address = Address.Find(addressId);
				var oldStatus = address.Enabled;
				if (enabled && !oldStatus)
					Mailer.AddressBackToWork(address);
				address.Enabled = enabled;
				address.FreeFlag = free;
				address.Client.UpdateBeAccounted();
				address.Client.Save();

				scope.VoteCommit();
			}
			CancelView();
			CancelLayout();
		}

		public void Search()
		{
			var billingSearchProperties = new BillingSearchProperties {
				ClientStatus = SearchClientStatus.Enabled
			};
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["regions"] = GetRegions();
			PropertyBag["FindBy"] = billingSearchProperties;
		}

		public void SearchBy([DataBind("SearchBy")] BillingSearchProperties searchProperties)
		{
			var searchResults = BillingSearchItem.FindBy(searchProperties);

			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["regions"] = GetRegions();
			PropertyBag["sortColumnName"] = "ShortName";
			PropertyBag["sortDirection"] = "Ascending";
			PropertyBag["FindBy"] = searchProperties;
			PropertyBag["searchResults"] = searchResults;
		}

		public void OrderBy(string columnName, 
							string sortDirection,
							string searchText,
							ulong regionId,
							uint recipientId,
							PayerStateFilter payerState,
							SearchSegment segment,
							SearchClientType clientType,
							SearchClientStatus clientStatus,
							SearchBy searchBy)
		{
			var searchProperties = new BillingSearchProperties
			{
				PayerState = payerState,
				SearchText = searchText,
				RegionId = regionId,
				RecipientId = recipientId,
				Segment = segment,
				ClientStatus = clientStatus,
				ClientType = clientType,
				SearchBy = searchBy
			};
			var direction = sortDirection == "Ascending" ? SortDirection.Ascending : SortDirection.Descending;

			var searchResults = (List<BillingSearchItem>)BillingSearchItem.FindBy(searchProperties);

			searchResults.Sort(new PropertyComparer<BillingSearchItem>(direction, columnName));

			PropertyBag["FindBy"] = searchProperties;
			PropertyBag["regions"] = GetRegions();
			PropertyBag["searchResults"] = searchResults;
			PropertyBag["sortColumnName"] = columnName;
			PropertyBag["sortDirection"] = sortDirection;
			RenderView("SearchBy");
		}

		public void Save([DataBind("SearchBy")] BillingSearchProperties searchProperties,
			[DataBind("PaymentInstances")] PaymentInstance[] paymentInstances)
		{
			using (new TransactionScope())
				foreach (var instance in paymentInstances)
					instance.Save();
			SearchBy(searchProperties);
			RenderView("SearchBy");
		}

		public void SentMail(uint clientCode, [DataBind("MailSentEntity")] MailSentEntity sentEntity)
		{
			try
			{
				sentEntity.UserName = SecurityContext.Administrator.UserName;
				sentEntity.SaveAndFlush();
				Flash["Message"] = new Message("Cохранено");
			}
			catch (ValidationException ex)
			{
				Flash["SendMailError"] = ex.ValidationErrorMessages[0];
			}
			Redirect("Billing", "Edit", new {clientCode, tab = "mail"});
		}

		public void DeleteMail(uint id)
		{
			var mailSend = MailSentEntity.Find(id);
			mailSend.IsDeleted = true;
			CancelView();
		}

		public void ShowMessageForUser(uint userId)
		{
			var message = ClientMessage.FindUserMessage(userId);
			PropertyBag["Message"] = message;
			PropertyBag["user"] = User.Find(message.ClientCode);
		}

		public void CancelMessage(uint userId)
		{
			using (new TransactionScope())
			{
				var message = ClientMessage.Find(userId);
				message.ShowMessageCount = 0;
				message.Update();
			}
			CancelView();
		}

		public void AddPayment(uint clientCode, [DataBind("Payment")] Payment payment)
		{
			payment.PaymentType = PaymentType.Charge;
			payment.Name = "Оплата";
			payment.Save();
			Flash["Message"] = new Message("Сохранено");
			Redirect("Billing", "Edit", new {clientCode, tab = "payments"});
		}

		private static IList<Region> GetRegions()
		{
			return Region.GetRegionsForClient(HttpContext.Current.User.Identity.Name);
		}

		public void AdditionalUserInfo(uint userId, string cssClassName)
		{
			CancelLayout();
			PropertyBag["user"] = User.Find(userId);
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
				address.Client.UpdateBeAccounted();
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
				client.UpdateBeAccounted();
				client.Save();

				scope.VoteCommit();
			}
			CancelView();
			CancelLayout();
		}

		public void TotalSum(uint payerId)
		{
			Response.Output.Write(Payer.Find(payerId).TotalSum.ToString());
			CancelView();
			CancelLayout();
		}

		public void Accounting([DataBind("SearchBy")] AccountingSearchProperties SearchBy, string tab, uint? pageSize, uint? currentPage, uint? rowsCount)
		{
			if ((SearchBy.BeginDate == null) && (SearchBy.EndDate == null) && (SearchBy.SearchText == null))
				SearchBy = new AccountingSearchProperties {
					BeginDate = DateTime.Today.AddDays(-1),
					EndDate = DateTime.Today,
					SearchText = String.Empty,
				};
			if (!pageSize.HasValue)
				pageSize = 30;
			if (!currentPage.HasValue)
				currentPage = 0;

			if (String.IsNullOrEmpty(tab))
				tab = "unregistredItems";

			if (tab.Equals("unregistredItems", StringComparison.CurrentCultureIgnoreCase))
			{
				var unaccountedItems = AccountingItem.GetUnaccountedObjects(currentPage.Value, pageSize.Value, rowsCount.HasValue);
				PropertyBag["unaccountedItems"] = unaccountedItems;
				PropertyBag["rowsCount"] = rowsCount.HasValue ? rowsCount : (uint)unaccountedItems.Count;
				PropertyBag["currentPage"] = currentPage;
			}
			if (tab.Equals("AccountingHistory", StringComparison.CurrentCultureIgnoreCase))
			{
				var historyItems = AccountingItem
					.SearchBy(SearchBy, currentPage.Value, pageSize.Value, rowsCount.HasValue)
					.OrderByDescending(item => item.WriteTime)
					.ToList();
				PropertyBag["accountingHistoryItems"] = historyItems;
				PropertyBag["currentPage"] = currentPage;
				PropertyBag["rowsCount"] = rowsCount.HasValue ? rowsCount : (uint)historyItems.Count;
			}
			PropertyBag["pageSize"] = pageSize;
			PropertyBag["tab"] = tab;
			PropertyBag["FindBy"] = SearchBy;
		}

		public void JuridicalOrganizations(uint payerId, uint currentJuridicalOrganizationId)
		{
			var payer = Payer.Find(payerId);
			PropertyBag["Payer"] = payer;
			PropertyBag["tab"] = "juridicalOrganization";
			PropertyBag["Addresses"] = payer.GetAllAddresses();
			if (currentJuridicalOrganizationId > 0)
				PropertyBag["currentJuridicalOrganization"] = JuridicalOrganization.Find(currentJuridicalOrganizationId);
		}

		public void UpdateJuridicalOrganizationInfo([ARDataBind("juridicalOrganization", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] JuridicalOrganization juridicalOrganization)
		{
			CancelLayout();
			CancelView();
			using (var scope = new TransactionScope())
			{
				var organization = JuridicalOrganization.Find(juridicalOrganization.Id);
				organization.Name = juridicalOrganization.Name;
				organization.FullName = juridicalOrganization.FullName;
				organization.Inn = juridicalOrganization.Inn;
				organization.Kpp = juridicalOrganization.Kpp;
				organization.ReceiverAddress = juridicalOrganization.ReceiverAddress;
				organization.Address = juridicalOrganization.Address;

				organization.Update();
				scope.VoteCommit();
				Flash["Message"] = new Message("Сохранено");
				var billingCode = organization.Payer.PayerID;
				Redirect("Billing", "Edit", new { billingCode, tab = "juridicalOrganization", currentJuridicalOrganizationId = organization.Id });
			}
		}

		public void AddJuridicalOrganization([ARDataBind("juridicalOrganization", AutoLoad = AutoLoadBehavior.NewRootInstanceIfInvalidKey)] JuridicalOrganization juridicalOrganization, uint payerId)
		{
			CancelLayout();
			CancelView();

			using (var scope = new TransactionScope())
			{
				var payer = Payer.Find(payerId);

				juridicalOrganization.Payer = payer;
				juridicalOrganization.CreateAndFlush();

				scope.VoteCommit();

				Flash["Message"] = new Message("Юридическое лицо создано");
			}
			Redirect("Billing", "Edit", new { billingCode = payerId, tab = "juridicalOrganization", currentJuridicalOrganizationId = juridicalOrganization.Id });
		}
	}
}
