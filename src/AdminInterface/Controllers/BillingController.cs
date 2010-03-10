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
using NHibernate.Criterion;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[
		Layout("billing"), 
		Helper(typeof (BindingHelper)), 
		Helper(typeof (ViewHelper)),
		Secure(PermissionType.Billing, ExecutionOrder = 0),
		Secure(PermissionType.ViewSuppliers, PermissionType.ViewDrugstore, Required = Required.AnyOf, ExecutionOrder = 1),
		Filter(ExecuteWhen.BeforeAction, typeof (PayerFilterActivationFilter), ExecutionOrder = 2)
	]
	public class BillingController : ARSmartDispatcherController
	{
		public void Edit(uint billingCode,
			uint clientCode,
			uint userId,
			uint addressId,
			bool showClients,
			bool showAddresses,
			bool showUsers,
			string tab,
			DateTime? paymentsFrom,
			DateTime? paymentsTo)
		{
			Client client;
			User user;
			Address address;
			if (billingCode == 0)
				client = Client.Find(clientCode);
			else
				client = Payer.Find(billingCode).Clients.First();
			user = (userId != 0) ? User.Find(userId) : client.Users.First();
			address = (addressId != 0) ? Address.Find(addressId) : client.Addresses.First();

			SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
			SecurityContext.Administrator.CheckClientType(client.Type);

			var clientMessage = ClientMessage.FindClientMessage(client.Id);

			if (clientMessage != null)
				PropertyBag["ClientMessage"] = clientMessage;

			PropertyBag["ShowClients"] = showClients;
			PropertyBag["ShowUsers"] = showUsers;
			PropertyBag["ShowAddresses"] = showAddresses;

			if (String.IsNullOrEmpty(tab))
				tab = "payments";

			var payer = client.BillingInstance;
			PropertyBag["tab"] = tab;
			PropertyBag["paymentsFrom"] = paymentsFrom ?? payer.DefaultBeginPeriod();
			PropertyBag["paymentsTo"] = paymentsTo ?? payer.DefaultEndPeriod();
			PropertyBag["LogRecords"] = ClientLogRecord.GetClientLogRecords(client);
			PropertyBag["Client"] = client;
			PropertyBag["Instance"] = payer;
			PropertyBag["User"] = user;
			PropertyBag["Address"] = address;
			PropertyBag["recivers"] = Reciver.FindAll(Order.Asc("Name"));
			PropertyBag["Tariffs"] = Tariff.FindAll();
			PropertyBag["Payments"] = Payment.FindCharges(payer, paymentsFrom, paymentsTo);
			PropertyBag["MailSentHistory"] = MailSentEntity.GetHistory(payer.PayerID);
			//PropertyBag["ContactGroups"] = payer.ContactGroupOwner.ContactGroups;
			PropertyBag["Today"] = DateTime.Today;
			PropertyBag["TotalSum"] = payer.TotalSum;

			PropertyBag["Users"] = payer.GetAllUsers();
			PropertyBag["Addresses"] = payer.GetAllAddresses();
		}

		public void Update([ARDataBind("Instance", AutoLoadBehavior.Always)] Payer billingInstance, 
						   uint clientCode,
						   string tab)
		{
			billingInstance.UpdateAndFlush();
			Flash.Add("Message", new Message("Изменения сохранены"));
			Redirect("Billing", "Edit", new {clientCode, tab});
		}

		public void SendMessage([DataBind("NewClientMessage")] ClientMessage clientMessage)
		{
			try
			{
				using (new TransactionScope())
				{
					foreach (var user in Client.Find(clientMessage.ClientCode).Users)
					{
						DbLogHelper.SetupParametersForTriggerLogging<ClientMessage>(SecurityContext.Administrator.UserName,
						                                                            Request.UserHostAddress);
						var message = ClientMessage.Find(user.Id);
						message.Message = clientMessage.Message;
						message.ShowMessageCount = clientMessage.ShowMessageCount;
						message.Update();
					}
				}
				Flash.Add("Message", new Message("Сообщение сохранено"));
			}
			catch (ValidationException exception)
			{
				Flash.Add("SendError", exception.ValidationErrorMessages[0]);
			}

			Redirect("Billing", "Edit", new {clientMessage.ClientCode, tab = "payments"});
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

				// Если нужно, отключаем пользователей и адреса
				if (!enabled)
				{					
					foreach (var user in client.Users)
						SetUserStatus(user.Id, false, user.IsFree);
					foreach (var addr in client.Addresses)
						SetAddressStatus(addr.Id, false);
				}
			}
			CancelView();
			CancelLayout();
		}

		public void SetUserStatus(uint userId, bool enabled, bool free)
		{
			using (new TransactionScope())
			{
				var user = User.Find(userId);
				user.Enabled = enabled;
				user.IsFree = free;
				user.UpdateAndFlush();
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
			}
			CancelView();
            CancelLayout();
		}

		public void SetAddressStatus(uint addressId, bool enabled)
		{
			using (new TransactionScope())
			{
				var address = Address.Find(addressId);
				address.Enabled = enabled;
				address.UpdateAndFlush();
			}
			CancelView();
            CancelLayout();			
		}

		public void Search()
		{
			var billingSearchProperties = new BillingSearchProperties
			                              	{
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

		public void ShowMessageForClient(uint clientCode)
		{
			var message = ClientMessage.FindClientMessage(clientCode);
			PropertyBag["Message"] = message;
		}

		public void CancelMessage(uint clientCode)
		{
			
			using (new TransactionScope())
			{
				var rootUser = User.Find(clientCode);
				foreach (var user in rootUser.Client.Users /*Client.Find(clientCode).Users*/)
				{
					var message = ClientMessage.Find(user.Id);
					message.ShowMessageCount = 0;
					message.Update();
				}				
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
                (user.Name.ToLower().Contains(searchText.ToLower()) || user.Login.ToLower().Contains(searchText.ToLower())) &&
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
			using (new TransactionScope())
			{
				var user = User.Find(userId);
				var address = Address.Find(addressId);
				address.AvaliableForUsers.Add(user);
				address.Update();
			}
			CancelView();
			CancelLayout();
		}

		public void DisconnectUserFromAddress(uint userId, uint addressId)
		{
			using (new TransactionScope())
			{
				var user = User.Find(userId);
				var address = Address.Find(addressId);
				address.AvaliableForUsers.Remove(user);
				address.Update();
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
	}
}
