using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Mailers;
using AdminInterface.ManagerReportsFilters;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Controllers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate.Linq;
using AppHelper = AdminInterface.Helpers.AppHelper;

namespace AdminInterface.MonoRailExtentions
{
	public class AdminInterfaceController : BaseController
	{
		private List<MonorailMailer> mailers = new List<MonorailMailer>();

		public AdminInterfaceController()
		{
			BeforeAction += (action, context, controller, controllerContext) => {
				controllerContext.PropertyBag["admin"] = Admin;
			};
			AfterAction += (action, context, controller, controllerContext) => {
				SendMails();
			};
		}

		public bool IsProduction
		{
			get
			{
				return Context.UnderlyingContext != null
					&& Context.UnderlyingContext.Application != null
					&& ((WebApplication)Context.UnderlyingContext.ApplicationInstance).Environment == "production";
			}
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

		public MonorailMailer Mail()
		{
			var m = this.Mailer();
			mailers.Add(m);
			return m;
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

		public void SendMails()
		{
			if (Context.LastException == null) {
				foreach (var mailer in mailers) {
					try {
						mailer.Send();
					}
					catch (Exception e) {
						if (!IsProduction)
							throw;
						Logger.Error("Ошибка при отправке уведомления", e);
					}
				}
			}
		}
	}
}