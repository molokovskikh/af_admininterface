using System;
using System.Linq;
using AdminInterface.Mailers;
using AdminInterface.Models;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Controllers;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class NewsController : BaseController
	{
		public void Index()
		{
			PropertyBag["newses"] = DbSession.Query<News>()
				.OrderByDescending(n => n.PublicationDate)
				.ToList();
		}

		public void New()
		{
			var news = new News();
			PropertyBag["news"] = news;
			if (IsPost) {
				BindObjectInstance(news, "news");
				if (IsValid(news)) {
					DbSession.Save(news);
					new MonorailMailer().RegisterNews(news, "AFNews@subscribe.analit.net").Send();
					Notify("Сохранено");
					RedirectToAction("Index");
					return;
				}
			}
			RenderView("Edit");
		}

		public void Edit(uint id)
		{
			var news = DbSession.Load<News>(id);
			PropertyBag["news"] = news;
			if (IsPost) {
				BindObjectInstance(news, "news");
				if (IsValid(news)) {
					DbSession.Save(news);
					Notify("Сохранено");
					RedirectToAction("Index");
				}
			}
		}

		public void Delete(uint id)
		{
			var news = DbSession.Load<News>(id);
			news.Deleted = true;
			DbSession.Save(news);
			Notify("Удалено");
			RedirectToReferrer();
		}
	}
}