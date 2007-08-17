using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AdminInterface.Helpers;
using AdminInterface.Model;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Common.Web.Ui.Models;
using NHibernate;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[Layout("billing"), Helper(typeof(BindingHelper))]
	public class BillingController : ARSmartDispatcherController
	{
		public void Edit(uint clientCode)
		{
			Client client = Client.Find(clientCode);
			ClientMessage clientMessage = ClientMessage.TryFind(clientCode);

			if (clientMessage != null)
				PropertyBag.Add("ClientMessage", clientMessage);

			PropertyBag["LogRecords"] = ClientLogRecord.GetClientLogRecords(clientCode);
			PropertyBag["Client"] = client;
			PropertyBag["Instance"] = client.BillingInstance;
			PropertyBag["ContactGroups"] = client.BillingInstance.ContactGroupOwner.ContactGroups;

			SetTitle(client.BillingInstance);
		}

		public void Update([ARDataBind("Instance", AutoLoadBehavior.Always)] BillingInstance billingInstance, uint clientCode)
		{
			billingInstance.UpdateAndFlush();
			Flash.Add("UpdateMessage", "��������� ���������");
			RedirectToAction("Edit", "clientCode=" + clientCode);
		}

		public void SendMessage([DataBind("NewClientMessage")] ClientMessage clientMessage)
		{
			try
			{
				clientMessage.UpdateAndFlush();
				Flash.Add("SendMessage", "��������� �����������");
			}
			catch(ValidationException exception)
			{
				Flash.Add("SendError", exception.ValidationErrorMessages[0]);
			}
			RedirectToAction("Edit", "clientCode=" + clientMessage.ClientCode);
		}

		public void ChangeClientState(uint clientCode)
		{
			Client client = Client.Find(clientCode);
			if (client.Status == ClientStatus.On)
				client.Status = ClientStatus.Off;
			else
				client.Status = ClientStatus.On;
			DbLogHelper.SavePersistentWithLogParams(Session["UserName"].ToString(), HttpContext.Current.Request.UserHostAddress, client);
			RedirectToAction("Edit", "clientCode=" + clientCode);
		}

		public void Search()
		{
			PropertyBag["regions"] = GetRegions();
		}

		public void Search([DataBind("SearchBy")] BillingSearchProperties searchProperties)
		{
			IList<BillingSearchItem> searchResults = BillingSearchItem.FindBy(searchProperties);
			Context.Cache.Store("searchResults", searchResults);

			PropertyBag["regions"] = GetRegions();
			PropertyBag["sortColumnName"] = "ShortName";
			PropertyBag["sortDirection"] = "Ascending";
			PropertyBag["FindBy"] = searchProperties;
			PropertyBag["searchResults"] = searchResults;
		}

		public void OrderBy(string columnName, 
							string sortDirection,
							string shortName,
							ulong regionId,
							PayerStateFilter payerState)
		{
			BillingSearchProperties searchProperties = new BillingSearchProperties();
			searchProperties.PayerState = payerState;
			searchProperties.ShortName = shortName;
			searchProperties.RegionId = regionId;
			SortDirection direction = sortDirection == "Ascending" ? SortDirection.Ascending : SortDirection.Descending;
			List<BillingSearchItem> searchResults;
			if (Context.Cache.HasKey("searchResults"))
				searchResults = (List<BillingSearchItem>)Context.Cache.Get("searchResults");
			else
				searchResults = (List<BillingSearchItem>)BillingSearchItem.FindBy(searchProperties);

			searchResults.Sort(new PropertyComparer<BillingSearchItem>(direction, columnName));

			PropertyBag["FindBy"] = searchProperties;
			PropertyBag["regions"] = GetRegions();
			PropertyBag["searchResults"] = searchResults;
			PropertyBag["sortColumnName"] = columnName;
			PropertyBag["sortDirection"] = sortDirection.ToString();
			RenderView("search");
		}

		public bool IsDebitor(BillingSearchItem item)
		{
			return DateTime.Now - item.PayDate > TimeSpan.FromDays(1);
		}

		private IList<Region> GetRegions()
		{
			if (!Context.Cache.HasKey("clientRegions"))
				Context.Cache.Store("clientRegions", Region.GetRegionsForClient(HttpContext.Current.User.Identity.Name));
			return (IList<Region>)Context.Cache.Get("clientRegions");
		}

		public string GetChangeStatusButtonText(Client client)
		{
			if (client.Status == ClientStatus.On)
				return "��������� �������";
			else
				return "�������� �������";
		}

		public bool IsClientActive(Client client)
		{
			return client.Status == ClientStatus.On;
		}

		private void SetTitle(BillingInstance billingInstance)
		{
			PropertyBag.Add("Title", String.Format("��������� ���������� � ����������� {0}",
												   billingInstance.ShortName));
		}

		public bool IsContainsNotShowedMessage()
		{
			return ((ClientMessage) PropertyBag["ClientMessage"]).ShowMessageCount > 0;
		}
	}
}
