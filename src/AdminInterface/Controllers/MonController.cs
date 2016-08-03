using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Web.Ui.Controllers;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class RequestStat
	{
		public int Count { get; set; }
		public string Version { get; set; }
	}

	public class MonController : Controller
	{
		protected ISession DbSession;

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			DbSession = sessionHolder.CreateSession(typeof(ActiveRecordBase));
		}

		protected override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			var sessionHolder = ActiveRecordMediator.GetSessionFactoryHolder();
			sessionHolder.ReleaseSession(DbSession);
		}

		public ActionResult Index()
		{
			var begin = DateTime.Today;
			var end = DateTime.Today.AddDays(1);
			var versions = new string[0];
			if (Request.HttpMethod == "POST") {
				begin = DateTime.Parse(Request.Form["begin"]);
				end = DateTime.Parse(Request.Form["end"]);
				versions = Request.Form.GetValues("version") ?? new string[0];
			}
			var query = DbSession.Query<RequestLog>()
				.Where(x => x.CreatedOn > begin && x.CreatedOn < end);
			ViewBag.Versions = query.GroupBy(x => x.Version).Select(x => x.Key).ToArray()
				.Select(x => new SelectListItem {
					Text = x,
					Value = x,
					Selected = versions.Contains(x)
				});

			if (versions.Length > 0) {
				query = query.Where(x => versions.Contains(x.Version));
			}

			var items = query
				.GroupBy(x => x.Version)
				.OrderByDescending(x => x.Count())
				.Select(x => new RequestStat { Count = x.Count(), Version = x.Key })
				.ToList();
			ViewBag.Begin = begin;
			ViewBag.End = end;
			ViewBag.Items = items;
			return View();
		}

		public ActionResult Users(string version, DateTime begin, DateTime end)
		{
			var items = DbSession.Query<RequestLog>()
				.Where(x => x.CreatedOn > begin && x.CreatedOn < end && x.Version == version)
				.OrderByDescending(x => x.CreatedOn)
				.ToList();
			return View(items);
		}
	}
}