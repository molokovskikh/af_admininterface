using System.Linq.Dynamic;
using System.Web.Mvc;
using AdminInterface.Models;
using System.Linq;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	[Secure(PermissionType.ManageSuppliers)]
	public class ParserController : MvcController
	{
		public ActionResult Index(uint supplierId)
		{
			var supplier = DbSession.Load<Supplier>(supplierId);
			ViewBag.Parsers = DbSession.Query<Parser>().Where(x => x.Supplier == supplier).OrderBy(x => x.Name).ToList();
			ViewBag.Supplier = supplier;
			return View();
		}

		public ActionResult Add(uint supplierId)
		{
			var model = new Parser(DbSession.Load<Supplier>(supplierId));
			ViewBag.Supplier = model.Supplier;

			if (Request.HttpMethod == "POST") {
				var target = new Parser(model.Supplier);
				if (!TryUpdateModel(target))
					return View("Edit", target);

				return Update(model, target);
			}

			return View("Edit", model);
		}

		public ActionResult Edit(uint parserId)
		{
			var model = DbSession.Load<Parser>(parserId);
			ViewBag.Supplier = model.Supplier;
			if (Request.HttpMethod == "POST") {
				var target = new Parser();
				if (!TryUpdateModel(target))
					return View(target);

				return Update(model, target);
			}
			return View(model);
		}

		private ActionResult Update(Parser model, Parser target)
		{
			model.Name = target.Name;
			model.Encoding = target.Encoding;
			model.Lines.RemoveEach(model.Lines.Where(x => !target.Lines.Any(y => y.Id == x.Id)));
			model.Lines.Each(x => {
				var src = target.Lines.First(y => y.Id == x.Id);
				x.Dst = src.Dst;
				x.Src = src.Src;
			});
			model.Lines.AddEach(target.Lines.Where(x => x.Id == 0));
			DbSession.Save(model);

			Notify("Сохранено");
			return RedirectToAction("Edit", new {parserId = model.Id});
		}

		public ActionResult Delete(uint parserId)
		{
			var model = DbSession.Load<Parser>(parserId);
			DbSession.Delete(model);
			Notify("Сохранено");
			return RedirectToAction("Index", new { supplierId = model.Supplier.Id });
		}
	}
}