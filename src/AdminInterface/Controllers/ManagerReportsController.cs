using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using AdminInterface.Services;
using Castle.Components.Binder;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Common.Tools;
using Common.Web.Ui.Controllers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(PaginatorHelper), "paginator"),
		Helper(typeof(TableHelper), "tableHelper"),
		Helper(typeof(UrlHelper), "urlHelper"),
		Secure(PermissionType.ManagerReport),
	]
	public class ManagerReportsController : AdminInterfaceController
	{
		public void UsersAndAdresses()
		{
			var userFilter = new UserFinderFilter();
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
			BindObjectInstance(userFilter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			PropertyBag["filter"] = userFilter;
			PropertyBag["Users"] = userFilter.Find();
		}

		[return: JSONReturnBinder]
		public object GetUserInfo(uint userId)
		{
			var thisUser = DbSession.Get<User>(userId);
			var html = "<div class=\"userInfoDiv\">";
			html += string.Format("Клиент: {0} <br/>", thisUser.Client.Name);
			html += "<div>";
			foreach (var user in thisUser.Client.Users) {
				html += string.Format("<div class=\"padding5\">{0} - ({1})</div>", user.Name, user.Id);
				html += "<div class=\"padding10\">";
				foreach (var avaliableAddress in user.AvaliableAddresses) {
					html += string.Format("{0} </br>", avaliableAddress.Name);
				}
				html += "</div>";
			}
			html += "</div> </div>";
			return html;
		}

		public void GetUsersAndAdresses([DataBind("filter")] UserFinderFilter userFilter)
		{
			this.RenderFile("Пользоватети_и_адреса.xls", ExportModel.GetUserOrAdressesInformation(userFilter));
		}

		public void ClientAddressesMonitor()
		{
			var filter = new ClientAddressFilter();
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			PropertyBag["Clients"] = filter.Find();
			PropertyBag["filter"] = filter;
		}

		public void SwitchOffClients()
		{
			var filter = new SwitchOffClientsFilter();
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			PropertyBag["filter"] = filter;
			PropertyBag["Clients"] = filter.Find(DbSession);
		}

		public void WhoWasNotUpdated()
		{
			var filter = new WhoWasNotUpdatedFilter();
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			PropertyBag["filter"] = filter;
			if (Request.ObtainParamsNode(ParamStore.Params).GetChildNode("filter") != null)
				PropertyBag["Clients"] = filter.SqlQuery2(DbSession);
			else
				PropertyBag["Clients"] = new List<WhoWasNotUpdatedField>();
		}

		public void UpdatedAndDidNotDoOrders()
		{
			var urlHelper = new UrlHelper(Context);
			var filter = BindFilter<UpdatedAndDidNotDoOrdersFilter, UpdatedAndDidNotDoOrdersField>();
			PropertyBag["filter"] = filter;
			if (Request.ObtainParamsNode(ParamStore.Params).GetChildNode("filter") != null || filter.LoadDefault) {
				var result = filter.Find();
				foreach (var updatedAndDidNotDoOrdersField in result) {
					updatedAndDidNotDoOrdersField.UrlHelper = urlHelper;
				}
				PropertyBag["Clients"] = result.Cast<BaseItemForTable>().ToList();
			}
			else {
				PropertyBag["Clients"] = new List<BaseItemForTable>();
			}
		}

		public void AnalysisOfWorkDrugstores()
		{
			var filter = BindFilter<AnalysisOfWorkDrugstoresFilter, BaseItemForTable>();
			filter.SetDefaultRegion();
			FindFilter(filter);
		}

		public void ClientConditionsMonitoring()
		{
			var filter = BindFilter<ClientConditionsMonitoringFilter, MonitoringItem>();
			FindFilter(filter);
		}

		public void SendSupplierNotification(uint clientCode, uint supplierCode)
		{
			var client = DbSession.Get<Client>(clientCode);
			var service = new NotificationService(Defaults);
			if (supplierCode > 0) {
				var supplier = DbSession.Get<Supplier>(supplierCode);
				var contacts = supplier.ContactGroupOwner.GetEmails(ContactGroupType.ClientManagers).ToList();
				service.NotifySupplierAboutDrugstoreRegistration(client, contacts);
			}
			else {
				service.NotifySupplierAboutDrugstoreRegistration(client, true);
			}
			Flash["Message"] = Message.Notify("Уведомления отправлены");
			RedirectToReferrer();
		}

		[return: JSONReturnBinder]
		public object GetClientForAutoComplite(string term)
		{
			return DbSession.Query<Client>().Where(c => c.Name.Contains(term) && c.Status == ClientStatus.On && !c.Settings.IsHiddenFromSupplier)
				.ToList()
				.Select(c => new { id = c.Id, label = c.Name })
				.ToList();
		}
	}
}