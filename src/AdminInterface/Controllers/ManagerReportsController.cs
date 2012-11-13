using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
					updatedAndDidNotDoOrdersField.SetUrlHelper(urlHelper);
				}
				PropertyBag["Clients"] = result.Cast<BaseItemForTable>().ToList();
			}
			else {
				PropertyBag["Clients"] = new List<BaseItemForTable>();
			}
		}

		public void AnalysisOfWorkDrugstores()
		{
			var urlHelper = new UrlHelper(Context);
			var filter = BindFilter<AnalysisOfWorkDrugstoresFilter, BaseItemForTable>();
			filter.SetDefaultRegion();
			FindFilter(filter);
			foreach (var item in (IList)PropertyBag["Items"]) {
				((AnalysisOfWorkFiled)item).SetUrlHelper(urlHelper);
				((AnalysisOfWorkFiled)item).ReportType = AnalysisReportType.Client;
			}
		}

		[return: JSONReturnBinder]
		public object GetAnalysUserInfo(uint clientId)
		{
			CancelLayout();
			var urlHelper = new UrlHelper(Context);
			var filter = BindFilter<AnalysisOfWorkDrugstoresFilter, BaseItemForTable>();
			PropertyBag["filter"] = filter;
			var thisClient = DbSession.Get<Client>(clientId);
			var html = string.Format("<tr class=\"toggled\" id=\"toggledRow{0}\"><td colspan=9><div class=\"userInfoDivTable\">", clientId);
			html += string.Format("<b>Клиент: {0} </b><br/>", thisClient.Name);
			html += "<div>";
			foreach (var user in thisClient.Users) {
				filter.ObjectId = user.Id;
				filter.Type = AnalysisReportType.User;
				filter.RenderHead = false;
				var result = filter.Find();
				result.Cast<AnalysisOfWorkFiled>().Each(r => { r.ReportType = AnalysisReportType.User; r.SetUrlHelper(urlHelper); });
				PropertyBag["Items"] = result;
				TextWriter writer = new StringWriter();
				BaseMailer.ViewEngineManager.Process("ManagerReports/SimpleTable", writer, Context, this, ControllerContext);
				html += string.Format("<div class=\"padding5\"><b>{0} - ({1})</b> {2}</div>", user.Name, user.Id, writer);
				html += "<div class=\"padding10\">";
				foreach (var avaliableAddress in user.AvaliableAddresses) {
					filter.ObjectId = avaliableAddress.Id;
					filter.Type = AnalysisReportType.Address;
					filter.RenderHead = false;
					var resultAddress = filter.Find();
					resultAddress.Cast<AnalysisOfWorkFiled>().Each(r => { r.ReportType = AnalysisReportType.Address; r.SetUrlHelper(urlHelper); });
					PropertyBag["Items"] = resultAddress;
					TextWriter writerAddress = new StringWriter();
					BaseMailer.ViewEngineManager.Process("ManagerReports/SimpleTable", writerAddress, Context, this, ControllerContext);
					bool zeroOrderFlag = ((AnalysisOfWorkFiled)resultAddress.First()).CurWeekZak == 0;
					html += string.Format("<span class=\"{2}\"><b>{0}</b></span></br> {1} </br>", avaliableAddress.Name, writerAddress, zeroOrderFlag ? "adressNoOrder" : string.Empty);
				}
				html += "</div>";
			}
			html += "</div></div></td></tr>";
			return html;
		}

		public void ClientConditionsMonitoring()
		{
			var filter = BindFilter<ClientConditionsMonitoringFilter, MonitoringItem>();
			FindFilter(filter);
			PropertyBag["CiUrl"] = Properties.Settings.Default.ClientInterfaceUrl;
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
			uint id = 0;
			uint.TryParse(term, out id);
			return DbSession.Query<Client>().Where(c =>
				(c.Name.Contains(term) || c.Id == id) &&
					c.Status == ClientStatus.On &&
					c.Settings.InvisibleOnFirm == DrugstoreType.Standart)
				.ToList()
				.Select(c => new { id = c.Id, label = c.Name })
				.ToList();
		}
	}
}