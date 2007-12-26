using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Model;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Layout("billing"), Helper(typeof(BindingHelper)), Helper(typeof(ViewHelper))]
	public class BillingController : ARSmartDispatcherController
	{
		public void Edit(uint clientCode, bool showClients)
		{
			Client client = Client.Find(clientCode);
			ClientMessage clientMessage = ClientMessage.TryFind(clientCode);

			if (clientMessage != null)
				PropertyBag.Add("ClientMessage", clientMessage);

			if (showClients)
				PropertyBag["ShowClients"] = showClients;

			PropertyBag["LogRecords"] = ClientLogRecord.GetClientLogRecords(clientCode);
			PropertyBag["Client"] = client;
			PropertyBag["Instance"] = client.BillingInstance;
			PropertyBag["ContactGroups"] = client.BillingInstance.ContactGroupOwner.ContactGroups;
		}

		public void Update([ARDataBind("Instance", AutoLoadBehavior.Always)] BillingInstance billingInstance, 
						   uint clientCode)
		{
			billingInstance.UpdateAndFlush();
			Flash.Add("UpdateMessage", "Изменения сохранены");
			RedirectToAction("Edit", "clientCode=" + clientCode);
		}

		public void SendMessage([DataBind("NewClientMessage")] ClientMessage clientMessage)
		{
			try
			{
				clientMessage.UpdateAndFlush();
				Flash.Add("SendMessage", "Сообщение отправленно");
			}
			catch (ValidationException exception)
			{
				Flash.Add("SendError", exception.ValidationErrorMessages[0]);
			}

			RedirectToAction("Edit", "clientCode=" + clientMessage.ClientCode);
		}

		public void UpdateClientsStatus(uint clientCode, [DataBind("Status")] ClientWithStatus[] clients)
		{
			using(TransactionScope scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SavePersistentWithLogParams(Session["UserName"].ToString(),
				                                        HttpContext.Current.Request.UserHostAddress,
				                                        ActiveRecordMediator.GetSessionFactoryHolder().CreateSession(typeof(ClientWithStatus)));
				foreach (ClientWithStatus client in clients)
					client.Update();
				scope.VoteCommit();
			}
			RedirectToAction("Edit", "clientCode=" + clientCode, "showClients=true");
		}

		public void Search()
		{
			BillingSearchProperties billingSearchProperties = new BillingSearchProperties();
			billingSearchProperties.ClientStatus = SearchClientStatus.Enabled;
			PropertyBag["regions"] = GetRegions();
			PropertyBag["FindBy"] = billingSearchProperties;
		}

		public void SearchBy([DataBind("SearchBy")] BillingSearchProperties searchProperties)
		{
			IList<BillingSearchItem> searchResults = BillingSearchItem.FindBy(searchProperties);

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
			BillingSearchProperties searchProperties = new BillingSearchProperties();
			searchProperties.PayerState = payerState;
			searchProperties.SearchText = searchText;
			searchProperties.RegionId = regionId;
			searchProperties.Segment = segment;
			searchProperties.ClientStatus = clientStatus;
			searchProperties.ClientType = clientType;
			searchProperties.SearchBy = searchBy;
			SortDirection direction = sortDirection == "Ascending" ? SortDirection.Ascending : SortDirection.Descending;
			List<BillingSearchItem> searchResults;

			searchResults = (List<BillingSearchItem>)BillingSearchItem.FindBy(searchProperties);

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
			using (TransactionScope scope = new TransactionScope())
			{
				foreach (PaymentInstance instance in paymentInstances)
					instance.Save();				
				scope.Flush();
			}		
			SearchBy(searchProperties);
			RenderView("SearchBy");
		}

		private IList<Region> GetRegions()
		{
			return Region.GetRegionsForClient(HttpContext.Current.User.Identity.Name);
		}

		public string GetChangeStatusButtonText(Client client)
		{
			if (client.Status == ClientStatus.On)
				return "Отключить клиента";
			else
				return "Включить клиента";
		}
	}
}
