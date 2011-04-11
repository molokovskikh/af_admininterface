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
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Castle.Components.Validator;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using SortDirection = Common.Tools.SortDirection;

namespace AdminInterface.Controllers
{
	[
		Layout("GeneralWithJQueryOnly"),
		Helper(typeof (BindingHelper)),
		Helper(typeof (ViewHelper)),
		Secure(PermissionType.Billing),
	]
	public class BillingController : ARController
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
			Client client = null;
			var payer = Payer.Find(billingCode);
			if (clientCode != 0)
				client = Client.Find(clientCode);

			var user = (userId != 0) ? User.Find(userId) : payer.Users.FirstOrDefault();
			var address = (addressId != 0) ? Address.Find(addressId) : payer.Addresses.FirstOrDefault();

			var usersMessages = new List<UserMessage>();
			var usersLogs = new List<UserLogRecord>();
			var addressesLogs = new List<AddressLogRecord>();
			var clients = payer.Users.Select(u => u.Client).Distinct().ToList();
			var countUsersWithMessages = 0;
			foreach (var item in payer.Users)
			{
				var message = UserMessage.FindUserMessage(item.Id);
				if (message != null && message.IsContainsNotShowedMessage())
					countUsersWithMessages++;
				usersMessages.Add(message);
				usersLogs.AddRange(UserLogRecord.GetUserEnabledLogRecords(item));
			}
			foreach (var item in payer.Addresses)
				addressesLogs.AddRange(AddressLogRecord.GetAddressLogRecords(item));
			PropertyBag["CountUsersWithMessages"] = countUsersWithMessages;
			PropertyBag["UsersMessages"] = usersMessages;
			PropertyBag["ShowClients"] = showClients;

			if (String.IsNullOrEmpty(tab))
				tab = "payments";
			
			PropertyBag["tab"] = tab;
			PropertyBag["paymentsFrom"] = paymentsFrom ?? payer.DefaultBeginPeriod();
			PropertyBag["paymentsTo"] = paymentsTo ?? payer.DefaultEndPeriod();

			var clientLogs = ClientLogRecord.GetClientLogRecords(clients);
			var userLogs = usersLogs.OrderByDescending(logRecord => logRecord.LogTime).ToList();
			var addressLogs = addressesLogs.OrderByDescending(logRecord => logRecord.LogTime).ToList();
			PropertyBag["LogRecords"] = SwitchLogRecord.GetUnionLogs(clientLogs, userLogs, addressLogs);

			if (client != null)
				PropertyBag["Client"] = client;

			PropertyBag["Clients"] = clients;
			PropertyBag["Instance"] = payer;
			PropertyBag["payer"] = payer;
			PropertyBag["User"] = user;
			PropertyBag["Address"] = address;
			PropertyBag["MailSentHistory"] = MailSentEntity.GetHistory(payer.PayerID);
			PropertyBag["Today"] = DateTime.Today;
			PropertyBag["TotalSum"] = payer.TotalSum;
			PropertyBag["Recipients"] = Recipient.Queryable.OrderBy(r => r.Name).ToList();

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
			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				payer.UpdateAndFlush();
				Flash.Add("Message", new Message("Изменения сохранены"));
			}
			RedirectToReferrer();
		}

		public void SendMessage([DataBind("NewClientMessage")] UserMessage message,
			uint clientId,
			uint payerId,
			bool sendMessageToClientEmails,
			string subjectForEmailToClient)
		{
			try
			{
				Client client = null;
				using (var scope = new TransactionScope(OnDispose.Rollback))
				{
					DbLogHelper.SetupParametersForTriggerLogging();
					if (message.ClientCode != 0)
					{
						var user = User.Find(message.ClientCode);
						client = user.Client;
						SendMessageToUser(user, message);
					}
					else
					{
						var payer = Payer.Find(payerId);
						foreach(var user in payer.Users)
						{
							client = user.Client;
							SendMessageToUser(user, message);
						}
					}
					scope.VoteCommit();
				}
				if (sendMessageToClientEmails)
					Mailer.SendMessageFromBillingToClient(client, message.Message, subjectForEmailToClient);
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
			DbLogHelper.SetupParametersForTriggerLogging();
			var message = UserMessage.Find(user.Id);
			message.Message = clientMessage.Message;
			message.ShowMessageCount = clientMessage.ShowMessageCount;
			message.Update();
		}

		public void UpdateClientStatus(uint clientId, bool enabled)
		{
			using(new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var client = Client.Find(clientId);
				var oldEnabled = client.Enabled;
				client.Enabled = enabled;
				if (oldEnabled != client.Enabled)
				{
					this.Mail().EnableChanged(client, enabled).Send();
					ClientInfoLogEntity.StatusChange(client.Status, client).Save();
				}
				client.Save();
			}
			CancelView();
			CancelLayout();
		}

		public void Search()
		{
			var billingSearchProperties = new BillingSearchProperties {
				ClientStatus = SearchClientStatus.Enabled
			};
			PropertyBag["regions"] = GetRegions();
			PropertyBag["FindBy"] = billingSearchProperties;
		}

		public void SearchBy([DataBind("SearchBy")] BillingSearchProperties searchProperties)
		{
			var searchResults = BillingSearchItem.FindBy(searchProperties);

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
			var direction = sortDirection == "Ascending" ? SortDirection.Asc : SortDirection.Desc;

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
				sentEntity.UserName = Administrator.UserName;
				sentEntity.SaveAndFlush();
				Flash["Message"] = new Message("Cохранено");
			}
			catch (ValidationException ex)
			{
				Flash["SendMailError"] = ex.ValidationErrorMessages[0];
			}
			Redirect("Billing", "Edit", new {clientCode, billingCode = sentEntity.PayerId, tab = "mail"});
		}

		public void DeleteMail(uint id)
		{
			var mailSend = MailSentEntity.Find(id);
			mailSend.IsDeleted = true;
			CancelView();
		}

		public void ShowMessageForUser(uint userId)
		{
			CancelLayout();
			var message = UserMessage.FindUserMessage(userId);
			PropertyBag["Message"] = message;
			PropertyBag["user"] = User.Find(message.ClientCode);
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

		private static IList<Region> GetRegions()
		{
			var regions = RegionHelper.GetAllRegions();
			regions.First(r => r.Name == "Все").Id = ulong.MaxValue;
			return regions;
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
			Response.Output.Write(Payer.Find(payerId).TotalSum.ToString("#.#"));
			CancelView();
			CancelLayout();
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
