﻿using System;
using System.Linq;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Controllers;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class NewsController : AdminInterfaceController
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
					Mail().RegisterOrDeleteNews(news, "Зарегистрирована новость");
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
			PropertyBag["news"] = news;
			DbSession.Save(news);
			Mail().RegisterOrDeleteNews(news, "Скрыта новость");
			Notify("Удалено");
			RedirectToReferrer();
		}

		public void Open(uint id)
		{
			var news = DbSession.Load<News>(id);
			news.PublicationDate = DateTime.Now;
			// Для того, чтобы не рассылать повторные уведомления
			DbSession.Save(news);
			DbSession.Flush();
			news.Deleted = false;
			PropertyBag["news"] = news;
			DbSession.Save(news);
			Mail().RegisterOrDeleteNews(news, "Восстановлена новость");
			Notify("Восстановлено");
			RedirectToReferrer();
		}
	}
}