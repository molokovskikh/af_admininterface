using System.Collections.Generic;
using System.Linq;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Controllers;
using Common.Web.Ui.Helpers;
using NHibernate.Linq;
using AppHelper = AdminInterface.Helpers.AppHelper;

namespace AdminInterface.MonoRailExtentions
{
	public class AdminInterfaceController : BaseController
	{
		public AdminInterfaceController()
		{
			BeforeAction += (action, context, controller, controllerContext) => { controllerContext.PropertyBag["admin"] = Admin; };
		}

		public DefaultValues Defaults
		{
			get { return DbSession.Query<DefaultValues>().First(); }
		}

		protected Administrator Admin
		{
			get { return SecurityContext.Administrator; }
		}

		protected IUserStorage Storage
		{
			get { return ADHelper.Storage; }
		}

		protected AppConfig Config
		{
			get { return Global.Config; }
		}

		public void RedirectTo(object entity, string action = "Show")
		{
			var controller = AppHelper.GetControllerName(entity);
			var id = ((dynamic)entity).Id;
			RedirectUsingRoute(controller, action, new { id });
		}

		public TFilter BindFilter<TFilter, TItem>() where TFilter : IFiltrable<TItem>
		{
			SetSmartBinder(AutoLoadBehavior.OnlyNested);
			var filter = (TFilter)BindObject(IsPost ? ParamStore.Form : ParamStore.QueryString, typeof(TFilter), "filter", AutoLoadBehavior.OnlyNested);
			filter.Session = DbSession;
			return filter;
		}

		public void FindFilter<TItem>(IFiltrable<TItem> filter)
		{
			PropertyBag["filter"] = filter;
			if (Request.ObtainParamsNode(ParamStore.Params).GetChildNode("filter") != null || filter.LoadDefault)
				PropertyBag["Items"] = filter.Find();
			else
				PropertyBag["Items"] = new List<TItem>();
		}
	}
}