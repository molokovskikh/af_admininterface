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
using AdminInterface.Security;
using Castle.Components.Binder;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Common.Web.Ui.Controllers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.MonoRailExtentions;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(PaginatorHelper), "paginator"),
		Helper(typeof(TableHelper), "tableHelper"),
		Secure(PermissionType.ManagerReport),
	]
	public class ManagerReportsController : BaseController
	{
		public void UsersAndAdresses()
		{
			var userFilter = new UserFinderFilter();
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
			BindObjectInstance(userFilter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			PropertyBag["filter"] = userFilter;
			PropertyBag["Users"] = userFilter.Find();
		}

		public void GetUsersAndAdresses([DataBind("filter")] UserFinderFilter userFilter)
		{
			this.RenderFile("Пользовалети_и_адреса.xls", ExportModel.GetUserOrAdressesInformation(userFilter));
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
			var filter = new UpdatedAndDidNotDoOrdersFilter();
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
			BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter", AutoLoadBehavior.NullIfInvalidKey);
			PropertyBag["filter"] = filter;
			var result = filter.Find(DbSession);
			foreach (var updatedAndDidNotDoOrdersField in result) {
				updatedAndDidNotDoOrdersField.UrlHelper = urlHelper;
			}
			PropertyBag["Clients"] = result.Cast<BaseItemForTable>().ToList();
		}

		public void AnalysisOfWorkDrugstores()
		{
			MakeFilterAction<AnalysisOfWorkDrugstoresFilter, AnalysisOfWorkFiled>(f => f.SetDefaultRegion());
		}

		public void ClientConditionsMonitoring()
		{
			MakeFilterAction<ClientConditionsMonitoringFilter, MonitoringItem>();
		}

		protected void MakeFilterAction<TFilter, TItem>(Action<TFilter> action = null) where TFilter : IFind<TItem>
		{
			SetSmartBinder(AutoLoadBehavior.OnlyNested);
			var filter = (TFilter)BindObject(IsPost ? ParamStore.Form : ParamStore.QueryString, typeof(TFilter), "filter", AutoLoadBehavior.OnlyNested);
			filter.Session = DbSession;
			if (action != null)
				action(filter);
			PropertyBag["filter"] = filter;
			if (Request.ObtainParamsNode(ParamStore.Params).GetChildNode("filter") != null)
				PropertyBag["Items"] = filter.Find();
			else
				PropertyBag["Items"] = new List<TItem>();
		}
	}
}