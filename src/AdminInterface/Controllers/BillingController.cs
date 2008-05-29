using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
using AdminInterface.Filters;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Layout("billing"), Helper(typeof(BindingHelper)), Helper(typeof(ViewHelper))]
	[Filter(ExecuteWhen.BeforeAction, typeof(AuthorizeFilter))]
	[RequiredPermission(PermissionType.Billing)]
	public class BillingController : ARSmartDispatcherController
	{
		public void Edit(uint clientCode, bool showClients)
		{
			var client = Client.Find(clientCode);
			var clientMessage = ClientMessage.TryFind(clientCode);

			if (clientMessage != null)
				PropertyBag.Add("ClientMessage", clientMessage);

			if (showClients)
				PropertyBag["ShowClients"] = showClients;

			PropertyBag["LogRecords"] = ClientLogRecord.GetClientLogRecords(clientCode);
			PropertyBag["Client"] = client;
			PropertyBag["Instance"] = client.BillingInstance;
			PropertyBag["ContactGroups"] = client.BillingInstance.ContactGroupOwner.ContactGroups;
			PropertyBag["MailSentHistory"] = MailSentEntity.GetHistory(client.BillingInstance.PayerID);
		}

		public void Update([ARDataBind("Instance", AutoLoadBehavior.Always)] Payer billingInstance, 
						   uint clientCode)
		{
			billingInstance.UpdateAndFlush();
			Flash.Add("Message", new Message("Изменения сохранены"));
			RedirectToAction("Edit", "clientCode=" + clientCode);
		}

		public void SendMessage([DataBind("NewClientMessage")] ClientMessage clientMessage)
		{
			try
			{
				clientMessage.UpdateAndFlush();
				Flash.Add("Message", new Message("Сообщение сохранено"));
			}
			catch (ValidationException exception)
			{
				Flash.Add("SendError", exception.ValidationErrorMessages[0]);
			}

			RedirectToAction("Edit", "clientCode=" + clientMessage.ClientCode);
		}

		public void UpdateClientsStatus(uint clientCode, 
										[DataBind("Status")] ClientWithStatus[] clients)
		{
			using(var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SavePersistentWithLogParams(SecurityContext.Administrator.UserName,
				                                        HttpContext.Current.Request.UserHostAddress,
				                                        ActiveRecordMediator.GetSessionFactoryHolder().CreateSession(typeof(ClientWithStatus)));
				foreach (var client in clients)
					client.Update();
				scope.VoteCommit();
			}
			RedirectToAction("Edit", "clientCode=" + clientCode, "showClients=true");
		}

		public void Search()
		{
			var billingSearchProperties = new BillingSearchProperties
			                              	{
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
			using (var scope = new TransactionScope())
			{
				foreach (var instance in paymentInstances)
					instance.Save();				
				scope.Flush();
			}		
			SearchBy(searchProperties);
			RenderView("SearchBy");
		}

		public void SentMail(uint clientCode, uint payerId, string comment)
		{
			if (!String.IsNullOrEmpty(comment))
			{
				var mailSentEntity = new MailSentEntity
				                     	{
											PayerId =  payerId,
				                     		Comment = comment,
				                     		UserName = ((Administrator) Session["Admin"]).UserName
				                     	};
				mailSentEntity.SaveAndFlush();
			}
			RedirectToAction("Edit", "clientCode=" + clientCode);
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

		private static IList<Region> GetRegions()
		{
			return Region.GetRegionsForClient(HttpContext.Current.User.Identity.Name);
		}
	}

	public class RequiredPermissionAttribute : Attribute
	{
		public RequiredPermissionAttribute(PermissionType permissionType)
		{
			throw new NotImplementedException();
		}
	}
}
