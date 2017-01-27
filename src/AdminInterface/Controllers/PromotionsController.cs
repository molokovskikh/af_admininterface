using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Binder;
using Castle.Components.Validator;
using Castle.Core.Smtp;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using log4net;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers
{
	public enum PromotionStatus
	{
		[Description("Все")] All,
		[Description("Включенные")] Enabled,
		[Description("Отключенные")] Disabled,
	}

	public class PromotionFilter : Sortable
	{
		[Description("Статус:")]
		public PromotionStatus PromotionStatus { get; set; }

		[Description("Наименование:")]
		public string SearchText { get; set; }

		[Description("Поставщик:")]
		public string SearchSupplier { get; set; }

		public PromotionFilter()
		{
			PromotionStatus = PromotionStatus.Enabled;
			SortKeyMap = new Dictionary<string, string> {
				{ "Id", "Id" },
				{ "Enabled", "Enabled" },
				{ "Moderated", "Moderated" },
				{ "AgencyDisabled", "AgencyDisabled" },
				{ "Name", "Name" },
				{ "Begin", "Begin" },
				{ "End", "End" },
				{ "SupplierName", "s.Name" }
			};
			SortDirection = "Asc";
			SortBy = "SupplierName";
		}

		public List<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("PromotionOwnerSupplier", "s", JoinType.InnerJoin);

			if (PromotionStatus == Controllers.PromotionStatus.Enabled)
				criteria.Add(Expression.Eq("Status", true));
			else if (PromotionStatus == Controllers.PromotionStatus.Disabled)
				criteria.Add(Expression.Eq("Status", false));

			criteria.Add(Expression.Or(Expression.Eq("Moderated", true), Expression.And(Expression.IsNotNull("Moderator"), Expression.Eq("Moderated", false))));

			if (!String.IsNullOrEmpty(SearchSupplier))
				criteria.Add(Expression.Like("s.Name", SearchSupplier, MatchMode.Anywhere));

			if (!String.IsNullOrEmpty(SearchText))
				criteria.Add(Expression.Like("Name", SearchText, MatchMode.Anywhere));

			ApplySort(criteria);

			return ArHelper.WithSession(s => criteria
				.GetExecutableCriteria(s).List<T>())
				.ToList()
				.GroupBy(i => ((dynamic)i).Id)
				.Select(g => g.First())
				.ToList();
		}

		public string[] ToUrl()
		{
			return new[] {
				String.Format("filter.SearchSupplier={0}", SearchSupplier),
				String.Format("filter.PromotionStatus={0}", (int)PromotionStatus),
				String.Format("filter.SearchText={0}", SearchText),
			};
		}

		public string ToUrlQuery()
		{
			return string.Join("&", ToUrl());
		}

		public string GetUri()
		{
			var result = new StringBuilder();
			result.Append("filter.PromotionStatus=" + (int)PromotionStatus);
			String.Format("filter.SearchSupplier={0}", SearchSupplier);
			if (!String.IsNullOrEmpty(SearchText)) {
				result.Append("&filter.SearchText=" + SearchText);
			}
			return result.ToString();
		}
	}

	public class CatalogFilter : IPaginable
	{
		public string SearchText { get; set; }

		public SupplierPromotion Promotion;

		private int _lastRowsCount;

		public List<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>();

			criteria.Add(Expression.Eq("Hidden", false));

			if (!String.IsNullOrEmpty(SearchText))
				criteria.Add(Expression.Like("Name", SearchText, MatchMode.Anywhere));

			DetachedCriteria countSubquery = NHibernate.CriteriaTransformer.TransformToRowCount(criteria);
			_lastRowsCount = ArHelper.WithSession<int>(
				s => countSubquery.GetExecutableCriteria(s).UniqueResult<int>());

			criteria.AddOrder(Order.Asc("Name"));

			if (CurrentPage > 0)
				criteria.SetFirstResult(CurrentPage * PageSize);

			criteria.SetMaxResults(PageSize);

			var list = ArHelper.WithSession(
				s => criteria.GetExecutableCriteria(s).List<T>())
				.ToList();
			return list;
		}

		public string[] ToUrl()
		{
			return new[] {
				String.Format("Id={0}", Promotion.Id),
				String.Format("filter.SearchText={0}", SearchText)
			};
		}

		public string ToUrlQuery()
		{
			return string.Join("&", ToUrl());
		}

		public string GetUri()
		{
			return ToUrlQuery();
		}

		public int RowsCount
		{
			get { return _lastRowsCount; }
		}

		public int PageSize
		{
			get { return 30; }
		}

		public int CurrentPage { get; set; }
	}

	public class SupplierFilter : IPaginable
	{
		public string SearchText { get; set; }

		private int _lastRowsCount;

		public List<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("HomeRegion", "hr");

			if (!String.IsNullOrEmpty(SearchText))
				criteria.Add(Expression.Like("Name", SearchText, MatchMode.Anywhere));

			DetachedCriteria countSubquery = NHibernate.CriteriaTransformer.TransformToRowCount(criteria);
			_lastRowsCount = ArHelper.WithSession<int>(
				s => countSubquery.GetExecutableCriteria(s).UniqueResult<int>());

			criteria.AddOrder(Order.Asc("Name"));
			criteria.AddOrder(Order.Asc("hr.Name"));

			if (CurrentPage > 0)
				criteria.SetFirstResult(CurrentPage * PageSize);

			criteria.SetMaxResults(PageSize);

			var list = ArHelper.WithSession(
				s => criteria.GetExecutableCriteria(s).List<T>())
				.ToList();
			return list;
		}

		public string[] ToUrl()
		{
			return new[] {
				String.Format("filter.SearchText={0}", SearchText),
			};
		}

		public string ToUrlQuery()
		{
			return string.Join("&", ToUrl());
		}

		public string GetUri()
		{
			return ToUrlQuery();
		}

		public int RowsCount
		{
			get { return _lastRowsCount; }
		}

		public int PageSize
		{
			get { return 30; }
		}

		public int CurrentPage { get; set; }
	}

	[
		Helper(typeof(BindingHelper)),
		Helper(typeof(ViewHelper)),
		Helper(typeof(ADHelper)),
		Helper(typeof(PaginatorHelper), "paginator"),
	]
	public class PromotionsController : AdminInterfaceController
	{
		private Dictionary<string, string> _allowedExtentions;

		private static ILog log = LogManager.GetLogger(typeof(PromotionsController));

		public PromotionsController()
		{
			_allowedExtentions = new Dictionary<string, string> {
				{ ".jpg", "image/JPEG" },
				{ ".jpeg", "image/JPEG" },
				{ ".txt", "text/plain" },
			};
		}

		private bool IsAllowedExtention(string extention)
		{
			return _allowedExtentions.ContainsKey(extention.ToLower());
		}

		private string GetAllowedExtentions()
		{
			return _allowedExtentions.Keys.Implode();
		}

		public void Index([DataBind("filter")] PromotionFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["systemTime"] = SystemTime.Now().AddDays(3).Date;
			PropertyBag["promotions"] = filter.Find<SupplierPromotion>();
			PropertyBag["promotionsPremoderated"] = DbSession.Query<SupplierPromotion>().Where(s => s.Moderator == null && s.Moderated == false).OrderBy(d => d.Begin).ToList();
			PropertyBag["SortBy"] = Request["SortBy"];
			PropertyBag["Direction"] = Request["Direction"];
		}

		public void Delete(uint id)
		{
			var promotion = DbSession.Load<SupplierPromotion>(id);
			DbSession.Delete(promotion);
			Notify("Удалено");

			RedirectToReferrer();
		}

		public void Edit(uint id, [DataBind("PromoRegions")] ulong[] promoRegions, bool deleteOnSave = false)
		{
			PropertyBag["allowedExtentions"] = GetAllowedExtentions();
			Binder.Validator = Validator;

			var promotion = DbSession.Load<SupplierPromotion>(id);

			PropertyBag["promotion"] = promotion;
			PropertyBag["isOverdued"] = promotion.End < SystemTime.Now();
			ActiveRecordMediator.Evict(promotion);
			PropertyBag["AllowRegions"] = Region.GetRegionsByMask(DbSession, promotion.PromotionOwnerSupplier.MaskRegion).OrderBy(reg => reg.Name);


			if (IsPost) {
				promotion.RegionMask = promoRegions.Aggregate(0UL, (v, a) => a + v);

				BindObjectInstance(promotion, "promotion");
				promotion.UpdateStatus();

				if (IsValid(promotion)) {
					if (deleteOnSave) {
						var oldLocalPromoFile = GetPromoFile(promotion);
						if (File.Exists(oldLocalPromoFile))
							File.Delete(oldLocalPromoFile);
						promotion.PromoFile = null;
					} else {
						var file = Request.Files["inputfile"] as HttpPostedFile;
						if (file != null && file.ContentLength > 0) {
							if (!IsAllowedExtention(Path.GetExtension(file.FileName))) {
								Error("Выбранный файл имеет недопустимый формат.");
								return;
							}

							var oldLocalPromoFile = GetPromoFile(promotion);
							if (File.Exists(oldLocalPromoFile))
								File.Delete(oldLocalPromoFile);

							promotion.PromoFile = file.FileName;

							var newLocalPromoFile = GetPromoFile(promotion);
							using (var newFile = File.Create(newLocalPromoFile)) {
								file.InputStream.CopyTo(newFile);
							}
						}
					}

					promotion.UpdateStatus();
					DbSession.Update(promotion);
					SendNotificationAboutUpdatePromotion(promotion);
					RedirectToAction("Index");
				}
				else
					ActiveRecordMediator.Evict(promotion);
			}
			else
				ActiveRecordMediator.Evict(promotion);
		}
		/// <summary>
		//отправка уведомления о новой промо-акции
		/// </summary>
		/// <param name="promotion"></param>
		void SendNotificationAboutNewPromotion(SupplierPromotion promotion)
		{
			try {
				var mailFrom = ConfigurationManager.AppSettings["NewPromotionNotifier"];
				var sender = new DefaultSmtpSender(ConfigurationManager.AppSettings["SmtpServer"]);
				var message = new MailMessage();
				message.Subject = "Новая промо-акция";
				message.From = new MailAddress(mailFrom);
				message.To.Add(new MailAddress(mailFrom));
				message.BodyEncoding = Encoding.UTF8;
				message.HeadersEncoding = Encoding.UTF8;
				message.Body =
					string.Format(@"Добавлена новая промо-акция.
Поставщик : {0}
Акция     : '{1}' ({2})
Период    : с {3} по {4}
Время     : {5}
Ip-адрес  : {6}", promotion.PromotionOwnerSupplier.Name, promotion.Name, promotion.Id,
						promotion.Begin.ToShortDateString(), promotion.End.ToShortDateString(),
						DateTime.Now.ToString("dd.MM.yyyy HH:mm"), HttpContext.Current?.Request?.UserHostAddress);
#if !DEBUG
				sender.Send(message);
#endif
			} catch (Exception e) {
#if DEBUG
				throw;
#endif
				log.Error(String.Format("Ошибка при отправке уведомления о новой промо-акции {0}", promotion.Id), e);
			}
		}

		/// <summary>
		//отправка уведомления о новой промо-акции
		/// </summary>
		/// <param name="promotion"></param>
		void SendNotificationAboutUpdatePromotion(SupplierPromotion promotion)
		{
			try {
				var mailFrom = ConfigurationManager.AppSettings["NewPromotionNotifier"];
				var sender = new DefaultSmtpSender(ConfigurationManager.AppSettings["SmtpServer"]);
				var message = new MailMessage();
				message.Subject = "Обновлена промо-акция";
				message.From = new MailAddress(mailFrom);
				message.To.Add(new MailAddress(mailFrom));
				message.BodyEncoding = Encoding.UTF8;
				message.HeadersEncoding = Encoding.UTF8;
				message.Body =
					string.Format(@"Обновлена промо-акция.
Поставщик : {0}
Акция     : '{1}' ({2})
Период    : с {3} по {4}
Время     : {5}
Ip-адрес  : {6}", promotion.PromotionOwnerSupplier.Name, promotion.Name, promotion.Id,
						promotion.Begin.ToShortDateString(), promotion.End.ToShortDateString(),
						DateTime.Now.ToString("dd.MM.yyyy HH:mm"), HttpContext.Current?.Request?.UserHostAddress);
#if !DEBUG
				sender.Send(message);
#endif
			} catch (Exception e) {
#if DEBUG
				throw;
#endif
				log.Error(String.Format("Ошибка при отправке уведомления об обновлении промо-акции {0}", promotion.Id), e);
			}
		}

		void SendMailFromModerator(List<string> contacts, string subject, string body)
		{
			try {
				var sender = new DefaultSmtpSender(ConfigurationManager.AppSettings["SmtpServer"]);
				var message = new MailMessage();
				message.Subject = subject;
				message.From = new MailAddress(ConfigurationManager.AppSettings["ModeratorMailFrom"]);
				message.To.Add(string.Join(",", contacts));
				message.BodyEncoding = Encoding.UTF8;
				message.HeadersEncoding = Encoding.UTF8;
				message.IsBodyHtml = true;
				message.Body = body;
				sender.Send(message);
			}
			catch (Exception e) {
#if DEBUG
				throw;
#endif
				log.Error(String.Format("Ошибка при отправке письма от модератора {0}", subject), e);
			}
		}

		public void ChangeModeration(uint id, string buttonText, string reason)
		{
			var moderationState = buttonText == "Подтвердить" ? 0 : buttonText == "Отказать" ? 1 : 2;

			if (moderationState != 0 && string.IsNullOrEmpty(reason)) {
				Error($"Необходимо указать причину {(moderationState == 1 ? "отказа" : "отмены")}!");
				RedirectToAction("Edit", new { id });
				return;
			}
			var promotion = DbSession.Load<SupplierPromotion>(id);
			if (moderationState == 0 && promotion.End < SystemTime.Now()) {
				Error($"Акция является просроченной, ее нельзя подтвердить.");
				RedirectToAction("Edit", new { id });
				return;
			}
			promotion.Moderated = moderationState == 0;
			promotion.ModerationChanged = SystemTime.Now();
			promotion.Moderator = Admin.Name;
			promotion.UpdateStatus();
			DbSession.Save(promotion);
			var defaultSettings = DbSession.Query<DefaultValues>().FirstOrDefault();
			if (defaultSettings != null) {
				var subject = moderationState == 0 ? defaultSettings.PromotionModerationAllowedSubject
					: moderationState == 1 ? defaultSettings.PromotionModerationDeniedSubject : defaultSettings.PromotionModerationEscapeSubject;
				var body = moderationState == 0 ? defaultSettings.PromotionModerationAllowedBody
					: moderationState == 1 ? defaultSettings.PromotionModerationDeniedBody : defaultSettings.PromotionModerationEscapeBody;

				body = string.Format(body ?? "", reason, promotion.Id, promotion.Name, Admin.Name, "<br/>");

				var contacts = promotion.PromotionOwnerSupplier.ContactGroupOwner.GetEmails(ContactGroupType.ClientManagers).ToList();
				if (contacts.Count > 0) {
#if !DEBUG
					SendMailFromModerator(contacts, subject, body);
#endif
				}
			}
			Notify("Сохранено");
			RedirectToAction("Edit", new { id });
		}


		public void ChangeState(uint id, [DataBind("filter")] PromotionFilter filter)
		{
			var promotion = DbSession.Load<SupplierPromotion>(id);
			promotion.Enabled = !promotion.Enabled;
			Notify("Сохранено");
			promotion.UpdateStatus();
			DbSession.Save(promotion);
			RedirectToAction("Index", filter.ToUrl());
		}

		public void ChangeDisabled(uint id, [DataBind("filter")] PromotionFilter filter)
		{
			var promotion = DbSession.Load<SupplierPromotion>(id);
			promotion.AgencyDisabled = !promotion.AgencyDisabled;
			Notify("Сохранено");
			promotion.UpdateStatus();
			DbSession.Save(promotion);
			RedirectToAction("Index", filter.ToUrl());
		}

		public void New(uint supplierId,
			[DataBind("PromoRegions")] ulong[] promoRegions)
		{
			RenderView("/Promotions/Edit");
			PropertyBag["allowedExtentions"] = GetAllowedExtentions();
			Binder.Validator = Validator;
			PropertyBag["IsNew"] = true;

			var client = DbSession.Load<PromotionOwnerSupplier>(supplierId);

			var promotion = new SupplierPromotion {
				Enabled = true,
				Catalogs = new List<Catalog>(),
				PromotionOwnerSupplier = client,
				RegionMask = client.HomeRegion.Id,
				Begin = DateTime.Now.Date,
				End = DateTime.Now.AddDays(6).Date,
			};
			PropertyBag["promotion"] = promotion;
			PropertyBag["AllowRegions"] = Region.GetRegionsByMask(DbSession, promotion.PromotionOwnerSupplier.MaskRegion);

			if (IsPost) {
				promotion.RegionMask = promoRegions.Aggregate(0UL, (v, a) => a + v);

				BindObjectInstance(promotion, "promotion");
				if (IsValid(promotion)) {
					var file = Request.Files["inputfile"] as HttpPostedFile;
					if (file != null && file.ContentLength > 0) {
						if (!IsAllowedExtention(Path.GetExtension(file.FileName))) {
							Error("Выбранный файл имеет недопустимый формат.");
							return;
						}

						promotion.PromoFile = file.FileName;
					}

					promotion.UpdateStatus();
					DbSession.Save(promotion);
					//отправка уведомления о новой промо-акции
					SendNotificationAboutNewPromotion(promotion);

					if (file != null && file.ContentLength > 0) {
						var newLocalPromoFile = GetPromoFile(promotion);
						using (var newFile = File.Create(newLocalPromoFile)) {
							file.InputStream.CopyTo(newFile);
						}
					}

					RedirectToAction("EditCatalogs", new[] { "id=" + promotion.Id });
				}
			}
		}

		private string GetPromoFile(SupplierPromotion promotion)
		{
			return Path.Combine(Config.PromotionsPath, promotion.Id + Path.GetExtension(promotion.PromoFile));
		}

		public void GetPromoFile(uint id)
		{
			var promotion = DbSession.Load<SupplierPromotion>(id);
			var fileName = GetPromoFile(promotion);
			this.RenderFile(fileName, promotion.PromoFile);
		}

		public void SelectSupplier([DataBind("filter")] SupplierFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["suppliers"] = filter.Find<PromotionOwnerSupplier>();
		}

		[return: JSONReturnBinder]
		public object SearchCatalog(string term)
		{
			return ActiveRecordLinq
				.AsQueryable<Catalog>()
				.Where(c => !c.Hidden && c.Name.Contains(term))
				.Take(20)
				.ToList()
				.Select(c => new {
					id = c.Id,
					label = c.Name
				});
		}

		public void EditCatalogs(
			uint id,
			[DataBind("filter")] CatalogFilter filter)
		{
			var promotion = DbSession.Load<SupplierPromotion>(id);
			PropertyBag["promotion"] = promotion;

			filter.Promotion = promotion;
			PropertyBag["filter"] = filter;
			PropertyBag["catalogNames"] = filter.Find<Catalog>();

			if (IsPost) {
				if (Request.Params["delBtn"] != null) {
					foreach (string key in Request.Params.AllKeys) {
						if (key.StartsWith("chd")) {
							var catalog = DbSession.Load<Catalog>(Convert.ToUInt32(Request.Params[key]));
							var index = promotion.Catalogs.IndexOf(c => c.Id == catalog.Id);
							if (index >= 0)
								promotion.Catalogs.RemoveAt(index);
						}
					}
				}

				if (Request.Params["addBtn"] != null) {
					foreach (string key in Request.Params.AllKeys) {
						if (key.StartsWith("cha")) {
							var catalog = DbSession.Load<Catalog>(Convert.ToUInt32(Request.Params[key]));

							if (promotion.Catalogs.FirstOrDefault(c => c.Id == catalog.Id) == null)
								promotion.Catalogs.Add(catalog);
						}
					}
				}

				ActiveRecordMediator.Evict(promotion);
				if (Validator.IsValid(promotion) && promotion.Catalogs.Count > 0) {
					Notify("Сохранено");
					promotion.UpdateStatus();
					DbSession.Update(promotion);
					RedirectToAction("EditCatalogs", filter.ToUrl());
				}
				else {
					var summary = String.Empty;
					if (Validator.GetErrorSummary(promotion) != null)
						summary = String.Join(Environment.NewLine, Validator.GetErrorSummary(promotion).ErrorMessages);

					if (promotion.Catalogs.Count == 0)
						summary = String.Join(Environment.NewLine, summary, "Список препаратов не может быть пустым.");

					Error(summary);
					ActiveRecordMediator.Evict(promotion);
					var discardedPromotion = DbSession.Load<SupplierPromotion>(id);
					PropertyBag["promotion"] = discardedPromotion;
				}
			}
		}
	}
}