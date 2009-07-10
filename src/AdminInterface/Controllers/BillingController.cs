using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
		public void Edit(uint clientCode, 
						bool showClients, 
						string tab, 
						DateTime? paymentsFrom, 
						DateTime? paymentsTo)
		{
			var client = Client.Find(clientCode);

			SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
			SecurityContext.Administrator.CheckClientType(client.Type);

			var clientMessage = ClientMessage.TryFind(clientCode);

			if (clientMessage != null)
				PropertyBag["ClientMessage"] = clientMessage;

			if (showClients)
				PropertyBag["ShowClients"] = showClients;

			if (String.IsNullOrEmpty(tab))
				tab = "payments";

			var payer = client.BillingInstance;
			PropertyBag["tab"] = tab;
			PropertyBag["paymentsFrom"] = paymentsFrom ?? payer.DefaultBeginPeriod();
			PropertyBag["paymentsTo"] = paymentsTo ?? payer.DefaultEndPeriod();
			PropertyBag["LogRecords"] = ClientLogRecord.GetClientLogRecords(clientCode);
			PropertyBag["Client"] = client;
			PropertyBag["Instance"] = payer;
			PropertyBag["recivers"] = Reciver.FindAll(Order.Asc("Name"));
			PropertyBag["Tariffs"] = Tariff.FindAll();
			PropertyBag["Payments"] = Payment.FindCharges(payer, paymentsFrom, paymentsTo);
			PropertyBag["ContactGroups"] = payer.ContactGroupOwner.ContactGroups;
			PropertyBag["MailSentHistory"] = MailSentEntity.GetHistory(payer.PayerID);
			PropertyBag["Today"] = DateTime.Today;
		}

		public void Update([ARDataBind("Instance", AutoLoadBehavior.Always)] Payer billingInstance, 
						   uint clientCode,
						   string tab)
		{
			billingInstance.UpdateAndFlush();
			Flash.Add("Message", new Message("Изменения сохранены"));
			RedirectToAction("Edit", new {clientCode, tab});
		}

		public void SendMessage([DataBind("NewClientMessage")] ClientMessage clientMessage)
		{
			try
			{
				using (new TransactionScope())
				{
					DbLogHelper.SetupParametersForTriggerLogging<ClientMessage>(SecurityContext.Administrator.UserName,
					                                                            Request.UserHostAddress);
					clientMessage.UpdateAndFlush();
				}
				Flash.Add("Message", new Message("Сообщение сохранено"));
			}
			catch (ValidationException exception)
			{
				Flash.Add("SendError", exception.ValidationErrorMessages[0]);
			}

			RedirectToAction("Edit", new {clientMessage.ClientCode, tab = "payments"});
		}

		public void UpdateClientsStatus(uint clientCode, 
										[DataBind("Status")] ClientWithStatus[] clients)
		{
			using(new TransactionScope())
			{
			    DbLogHelper.SetupParametersForTriggerLogging<ClientWithStatus>(SecurityContext.Administrator.UserName,
			                                                                   Request.UserHostAddress);
				foreach (var client in clients)
				{
					var oldClient = Client.Find(client.FirmCode);
					if (client.Status == ClientStatus.On && oldClient.Status == ClientStatus.Off)
						Mailer.ClientBackToWork(oldClient);
					if (oldClient.Status != client.Status)
						ClientInfoLogEntity.StatusChange(client.Status, client.FirmCode).Save();
					client.Update();
				}
			}
			RedirectToAction("Edit", new {clientCode, tab = "payments"});
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
		    RedirectToAction("Edit", new {clientCode, tab = "mail"});
		}

		public void DeleteMail(uint id)
		{
			var mailSend = MailSentEntity.Find(id);
			mailSend.IsDeleted = true;
			CancelView();
		}

		public void ShowMessageForClient(uint clientCode)
		{
			var message = ClientMessage.Find(clientCode);
			PropertyBag["Message"] = message;
		}

		public void CancelMessage(uint clientCode)
		{
			var message = ClientMessage.Find(clientCode);
			message.Message = null;
			message.ShowMessageCount = 0;
			message.Save();
			CancelView();
		}

		public void AddPayment(uint clientCode, [DataBind("Payment")] Payment payment)
		{
			payment.PaymentType = PaymentType.Charge;
			payment.Name = "Оплата";
			payment.Save();
			Flash["Message"] = new Message("Сохранено");
			RedirectToAction("Edit", new {clientCode, tab = "payments"});
		}

		private static IList<Region> GetRegions()
		{
			return Region.GetRegionsForClient(HttpContext.Current.User.Identity.Name);
		}
	}
}
