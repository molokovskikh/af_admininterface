using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AdminInterface.Controllers
{
	public enum PromotionStatus
	{
		[Description("Все")]All,
		[Description("Включенные")]Enabled,
		[Description("Отключенные")]Disabled,
	}

	public class PromotionFilter : SortableContributor
	{
		[Description("Статус:")]
		public PromotionStatus PromotionStatus { get; set; }
		public string SearchText { get; set; }

		public PromotionFilter()
		{
			PromotionStatus = PromotionStatus.Enabled;
		}

		public List<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("Supplier", "s", JoinType.InnerJoin)
				.CreateAlias("Catalog", "c", JoinType.InnerJoin);

			if (PromotionStatus == Controllers.PromotionStatus.Enabled)
				criteria.Add(Expression.Eq("Enabled", true));
			else
				if (PromotionStatus == Controllers.PromotionStatus.Disabled)
					criteria.Add(Expression.Eq("Enabled", false));

			if (!String.IsNullOrEmpty(SearchText))
				criteria.Add(Expression.Like("c.Name", SearchText, MatchMode.Anywhere));

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
			if (!String.IsNullOrEmpty(SearchText))
			{
				result.Append("&filter.SearchText=" + SearchText);
			}
			return result.ToString();
		}
	}

	public class CatalogFilter : IPaginable
	{
		public string SearchText { get; set; }

		public uint SupplierId { get; set; }

		private int _lastRowsCount;

		public List<T> Find<T>()
		{
			var criteria = DetachedCriteria.For<T>()
				.CreateAlias("Promotions", "sp", JoinType.LeftOuterJoin, Expression.Eq("sp.Supplier.Id", SupplierId))
				.CreateAlias("sp.Supplier", "s", JoinType.LeftOuterJoin);

			criteria.Add(Expression.Eq("Hidden", false));
			criteria.Add(Expression.IsNull("s.Id"));

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
				String.Format("filter.SearchText={0}", SearchText),
				String.Format("filter.SupplierId={0}", SupplierId)
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
		Layout("GeneralWithJQuery"),
		Helper(typeof(BindingHelper)), 
		Helper(typeof(ViewHelper)),
		Helper(typeof(ADHelper)),
		Helper(typeof(LinkHelper)),
		Helper(typeof(PaginatorHelper)),
	]
	public class PromotionsController : SmartDispatcherController
	{
		private Dictionary<string, string> _allowedExtentions;

		public PromotionsController()
		{
			_allowedExtentions = new Dictionary<string, string> 
			{
				{".htm", "text/HTML"},
				{".jpg", "image/JPEG"},
				{".txt", "text/plain"},
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
			PropertyBag["promotions"] = filter.Find<SupplierPromotion>().SortBy(Request["SortBy"], Request["Direction"] == "desc");
			PropertyBag["SortBy"] = Request["SortBy"];
			PropertyBag["Direction"] = Request["Direction"];
		}

		public void Delete(uint id)
		{
			var promotion = SupplierPromotion.Find(id);
			promotion.Delete();
			Flash["Message"] = "Удалено";

			RedirectToReferrer();
		}

		public void Edit(uint id)
		{
			PropertyBag["allowedExtentions"] = GetAllowedExtentions();
			Binder.Validator = Validator;

			var promotion = SupplierPromotion.Find(id);
			PropertyBag["promotion"] = promotion;

			if (IsPost)
			{
				BindObjectInstance(promotion, "promotion");
				if (!HasValidationError(promotion))
				{

					var file = Request.Files["inputfile"] as HttpPostedFile;
					if (file != null && file.ContentLength > 0)
					{
						if (!IsAllowedExtention(Path.GetExtension(file.FileName)))
						{
							PropertyBag["Message"] = Message.Error("Выбранный файл имеет недопустимый формат.");
							return;
						}

						var oldLocalPromoFile = GetPromoFile(promotion);
						if (File.Exists(oldLocalPromoFile))
							File.Delete(oldLocalPromoFile);

						promotion.PromoFile = file.FileName;

						var newLocalPromoFile = GetPromoFile(promotion);
						using (var newFile = File.Create(newLocalPromoFile))
						{
							file.InputStream.CopyTo(newFile);
						}
					}

					promotion.Save();
					RedirectToAction("Index");
				}
				else
					ActiveRecordMediator.Evict(promotion);
			}
		}

		public void ChangeState(uint id, [DataBind("filter")] PromotionFilter filter)
		{
			var promotion = SupplierPromotion.Find(id);
			promotion.Enabled = !promotion.Enabled;
			Flash["Message"] = "Сохранено";
			promotion.Save();
			RedirectToAction("Index", filter.ToUrl());
		}

		public void ChangeDisabled(uint id, [DataBind("filter")] PromotionFilter filter)
		{
			var promotion = SupplierPromotion.Find(id);
			promotion.AgencyDisabled = !promotion.AgencyDisabled;
			Flash["Message"] = "Сохранено";
			promotion.Save();
			RedirectToAction("Index", filter.ToUrl());
		}

		public void SelectName([DataBind("filter")] CatalogFilter filter)
		{
			PropertyBag["supplier"] = Supplier.Find(filter.SupplierId);
			PropertyBag["filter"] = filter;
			PropertyBag["catalogNames"] = filter.Find<Catalog>();
		}

		public void New(uint catalogId, uint supplierId)
		{
			RenderView("/Promotions/Edit");
			PropertyBag["allowedExtentions"] = GetAllowedExtentions();
			Binder.Validator = Validator;

			var client = Supplier.Find(supplierId);

			var catalog = Catalog.Find(catalogId);
			var promotion = new SupplierPromotion
			{
				Enabled = true,
				Catalog = catalog,
				Supplier = client
			};

			PropertyBag["promotion"] = promotion;

			if (IsPost)
			{
				BindObjectInstance(promotion, "promotion");
				if (!HasValidationError(promotion))
				{
					var file = Request.Files["inputfile"] as HttpPostedFile;
					if (file != null && file.ContentLength > 0)
					{
						if (!IsAllowedExtention(Path.GetExtension(file.FileName)))
						{
							PropertyBag["Message"] = Message.Error("Выбранный файл имеет недопустимый формат.");
							return;
						}

						promotion.PromoFile = file.FileName;
					}

					promotion.Save();

					if (file != null && file.ContentLength > 0)
					{
						var newLocalPromoFile = GetPromoFile(promotion);
						using (var newFile = File.Create(newLocalPromoFile))
						{
							file.InputStream.CopyTo(newFile);
						}
					}

					RedirectToAction("Index");
				}
				else
					ActiveRecordMediator.Evict(promotion);
			}
		}

		private string GetPromoFile(SupplierPromotion promotion)
		{
			return Path.Combine(CustomSettings.PromotionsPath(), promotion.Id + Path.GetExtension(promotion.PromoFile));
		}

		public void GetPromoFile(uint id)
		{
			CancelLayout();
			CancelView();

			var promotion = SupplierPromotion.Find(id);
			var fileName = GetPromoFile(promotion);

			if (File.Exists(fileName) && !String.IsNullOrEmpty(promotion.PromoFile))
				using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
				{
					Response.Clear();

					var extention = Path.GetExtension(promotion.PromoFile);
					if (_allowedExtentions.ContainsKey(extention))
						Response.ContentType = _allowedExtentions[extention];
					else
						Response.AppendHeader("Content-Disposition",
							String.Format("attachment; filename=\"{0}\"", Uri.EscapeDataString(promotion.PromoFile)));
					fileStream.CopyTo(Response.OutputStream);
				}
			else
				throw new Exception(String.Format("Для акции №{0} не существует файл {1}", promotion.Id, promotion.PromoFile));
		}

		public void SelectSupplier([DataBind("filter")] SupplierFilter filter)
		{
			PropertyBag["filter"] = filter;
			PropertyBag["suppliers"] = filter.Find<Supplier>();
		}

	}
}