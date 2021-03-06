﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Controllers.Filters;
using AdminInterface.Helpers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
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
		public ManagerReportsController()
		{
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
		}

		public void SynonymStat([SmartBinder] SynonymStat filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["items"] = filter.Find(DbSession);
		}

		public void UsersAndAdresses()
		{
			var userFilter = new UserFinderFilter();
			BindObjectInstance(userFilter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			PropertyBag["filter"] = userFilter;
			PropertyBag["Users"] = userFilter.Find(DbSession);
		}

		[return: JSONReturnBinder]
		public object GetUserInfo(uint userId)
		{
			var thisUser = DbSession.Get<User>(userId);
			var html = string.Format("<tr class=\"toggled\" id=\"toggledRow{0}\"><td colspan=9>", userId);
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
			html += "</div></td></tr>";
			return html;
		}

		public void GetUsersAndAdresses([DataBind("filter")] UserFinderFilter userFilter)
		{
			this.RenderFile("Пользователи_и_адреса.xls", ExportModel.GetUserOrAdressesInformation(userFilter));
		}

		public void ClientAddressesMonitor()
		{
			var filter = new ClientAddressFilter();
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			if(filter.Session == null)
				filter.Session = DbSession;
			PropertyBag["Clients"] = filter.Find();
			PropertyBag["filter"] = filter;
		}

		public void ClientAddressesMonitorToExcel([DataBind("filter")] ClientAddressFilter filter)
		{
			CancelLayout();
			CancelView();
			if(filter.Session == null)
				filter.Session = DbSession;
			var result = ExportModel.GetClientAddressesMonitor(filter);
			this.RenderFile("Клиенты и адреса в регионе, по которым не принимаются накладные.xls", result);
		}

		public void SwitchOffClients()
		{
			var filter = new SwitchOffClientsFilter();
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			PropertyBag["filter"] = filter;
			PropertyBag["Clients"] = filter.Find(DbSession);
		}

		public void ExcelSwitchOffClients()
		{
			var filter = new SwitchOffClientsFilter();
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			this.RenderFile("Список_отключенных_клиентов.xls", ExportModel.ExcelSwitchOffClients(filter));
		}

		public void ExcelWhoWasNotUpdated()
		{
			var filter = new WhoWasNotUpdatedFilter();
			filter.Session = DbSession;
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			this.RenderFile("Кто_не_обновлялся_с_опред._даты.xls", ExportModel.ExcelWhoWasNotUpdated(filter));
		}

		public void ExcelUpdatedAndDidNotDoOrders()
		{
			var filter = new UpdatedAndDidNotDoOrdersFilter {
				Session = DbSession
			};
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			this.RenderFile("Кто_обновлялся_и_не_делал_заказы.xls", filter.Excel());
		}

		public void ExcelAnalysisOfWorkDrugstores()
		{
			var filter = new AnalysisOfWorkDrugstoresFilter();
			filter.Session = DbSession;
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			this.RenderFile("Сравнительный_анализ_работы_аптек.xls", ExportModel.ExcelAnalysisOfWorkDrugstores(filter));
		}

		public void ExcelClientConditionsMonitoring()
		{
			var filter = new ClientConditionsMonitoringFilter();
			filter.Session = DbSession;
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			this.RenderFile("Мониторинг_выставления_условий_клиенту.xls", ExportModel.GetClientConditionsMonitoring(filter));
		}

		public void WhoWasNotUpdated()
		{
			var filter = BindFilter<WhoWasNotUpdatedFilter, WhoWasNotUpdatedField>();
			PropertyBag["filter"] = filter;
			PropertyBag["AllRegions"] = Region.All();
			if (Request.ObtainParamsNode(ParamStore.Params).GetChildNode("filter") != null)
				PropertyBag["Clients"] = filter.Find();
			else
				PropertyBag["Clients"] = new List<WhoWasNotUpdatedField>();
		}

		public void UpdatedAndDidNotDoOrders()
		{
			var filter = BindFilter<UpdatedAndDidNotDoOrdersFilter, UpdatedAndDidNotDoOrdersField>();
			PropertyBag["filter"] = filter;
			PropertyBag["SupplierDisabled"] = new List<Tuple<bool, string>>() {
				new Tuple<bool, string>(false, "Только по включенным поставщикам"),
				new Tuple<bool, string>(true, "Только по отключенным поставщикам")
			};
			PropertyBag["AllRegions"] = Region.All();
			PropertyBag["AllSuppliers"] = DbSession.Query<Supplier>()
				.Where(s => !s.Disabled && !s.Name.Contains("ассортимент"))
				.Select(x => Tuple.Create(x.Id, x.Name + " - " + x.HomeRegion.Name))
				.ToList()
				.OrderBy(x => x.Item2);

			if (Request.ObtainParamsNode(ParamStore.Params).GetChildNode("filter") != null || filter.LoadDefault) {
				var result = filter.Find();
				PropertyBag["Clients"] = result.Cast<BaseItemForTable>().ToList();
			}
			else {
				PropertyBag["Clients"] = new List<BaseItemForTable>();
			}
		}

		public void AnalysisOfWorkDrugstores()
		{
			var filter = BindFilter<AnalysisOfWorkDrugstoresFilter, BaseItemForTable>();
			FindFilter(filter);
			PropertyBag["AllRegions"] = Region.All();
			PropertyBag["AutoOrder"] = BindingHelper.GetDescriptionsDictionary(typeof(AutoOrderStatus))
				.Select(p => new Tuple<string, string>(p.Key.ToString(), p.Value))
				.ToList();
			foreach (var item in (IList)PropertyBag["Items"]) {
				((AnalysisOfWorkFiled)item).ReportType = AnalysisReportType.Client;
			}
		}

		[return: JSONReturnBinder]
		public object GetAnalysUserInfo(uint clientId)
		{
			CancelLayout();
			var filter = BindFilter<AnalysisOfWorkDrugstoresFilter, BaseItemForTable>();
			PropertyBag["filter"] = filter;
			var thisClient = DbSession.Get<Client>(clientId);
			var html = string.Format("<tr class=\"toggled\" id=\"toggledRow{0}\"><td colspan=11><div class=\"userInfoDivTable\">", clientId);
			html += string.Format("<b>Клиент: {0} </b><br/>", thisClient.Name);
			html += "<div>";
			foreach (var user in thisClient.Users.Where(x => x.Enabled)) {
				filter.ObjectId = user.Id;
				filter.Type = AnalysisReportType.User;
				filter.RenderHead = false;
				var result = filter.Find();
				result.Cast<AnalysisOfWorkFiled>().Each(r => r.ReportType = AnalysisReportType.User);
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
					resultAddress.Cast<AnalysisOfWorkFiled>().Each(r => r.ReportType = AnalysisReportType.Address);
					PropertyBag["Items"] = resultAddress;
					TextWriter writerAddress = new StringWriter();
					BaseMailer.ViewEngineManager.Process("ManagerReports/SimpleTable", writerAddress, Context, this, ControllerContext);
					bool zeroOrderFlag = ((AnalysisOfWorkFiled)resultAddress.First()).CurWeekSum == 0;
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
		}

		public void SendSupplierNotification(uint clientCode, uint supplierCode)
		{
			var client = DbSession.Get<Client>(clientCode);
			var service = new NotificationService(DbSession, Defaults);
			if (supplierCode > 0) {
				var supplier = DbSession.Get<Supplier>(supplierCode);
				var contacts = supplier.ContactGroupOwner.GetEmails(ContactGroupType.ClientManagers).ToList();
				service.NotifySupplierAboutDrugstoreRegistration(client, contacts);
			}
			else {
				service.NotifySupplierAboutDrugstoreRegistration(client, true);
			}
			Notify("Уведомления отправлены");
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
				.Select(c => new { id = c.Id, label = c.Name, region = c.HomeRegion.Name })
				.ToList();
		}

		public void FormPosition()
		{
			var filter = BindFilter<FormPositionFilter, BaseItemForTable>();
			FindFilter(filter);
		}

		public void FormPositionToExcel([DataBind("filter")] FormPositionFilter filter)
		{
			if(filter.Session == null)
				filter.Session = DbSession;
			var result = ExportModel.GetFormPosition(filter);
			this.RenderFile("Отчет о состоянии формализуемых полей.xls", result);
		}

		public void NotParcedWaybills()
		{
			var filter = new DocumentFilter();
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			var documents = filter.FindStat(DbSession);
			PropertyBag["filter"] = filter;
			PropertyBag["documents"] = documents;
		}

		public void NotParcedWaybillsToExcel([DataBind("filter")] DocumentFilter filter)
		{
			var result = ExportModel.GetNotParcedWaybills(DbSession, filter);
			this.RenderFile("Отчет о состоянии неформализованных накладных.xls", result);
		}

		public void ParsedWaybills()
		{
			var filter = BindFilter<ParsedWaybillsFilter, BaseItemForTable>();
			FindFilter(filter);
			if(!String.IsNullOrEmpty(filter.ClientName) && filter.ClientId == null)
				Warning("Поиск выполнен без учета клиента!" +
					" Для включения клиента в поиск необходимо выбрать полное наименование клиента" +
					" из выпадающего списка, появляющегося при вводе части наименования клиента в поле 'Клиент' ");
		}

		public void ParsedWaybillsToExcel([DataBind("filter")] ParsedWaybillsFilter filter)
		{
			if(filter.Session == null)
				filter.Session = DbSession;
			var result = ExportModel.GetParcedWaybills(filter);
			this.RenderFile("Отчет о состоянии формализованных накладных.xls", result);
		}
	}
}