using System.Linq;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.ActiveRecordSupport;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class SpecialHandlersController : AdminInterfaceController
	{
		public SpecialHandlersController()
		{
			SetBinder(new ARDataBinder());
			((ARDataBinder)Binder).AutoLoad = AutoLoadBehavior.Always;
		}

		public void Index(uint supplierId)
		{
			var supplier = DbSession.Load<Supplier>(supplierId);
			PropertyBag["supplier"] = supplier;
			PropertyBag["handlers"] = DbSession.Query<SpecialHandler>()
				.Where(h => h.Supplier == supplier)
				.OrderBy(h => h.Handler.ClassName)
				.ToList();
		}

		public void New(uint supplierId)
		{
			var supplier = DbSession.Load<Supplier>(supplierId);
			var handler = new SpecialHandler(supplier);
			PropertyBag["handler"] = handler;
			PropertyBag["supplier"] = supplier;
			ValidateAndSaveIfNeeded(handler);
		}

		public void Edit(uint id)
		{
			var handler = DbSession.Load<SpecialHandler>(id);
			PropertyBag["handler"] = handler;
			PropertyBag["supplier"] = handler.Supplier;
			ValidateAndSaveIfNeeded(handler);
		}

		public void Delete(uint id)
		{
			var handler = DbSession.Load<SpecialHandler>(id);
			DbSession.Delete(handler);
			Notify("Удалено");
			RedirectToAction("Index", new { supplierId = handler.Supplier.Id });
		}

		private void ValidateAndSaveIfNeeded(SpecialHandler handler)
		{
			if (IsPost) {
				BindObjectInstance(handler, "handler");
				if (IsValid(handler)) {
					DbSession.Save(handler);
					Notify("Сохранено");
					RedirectToAction("Index", new { supplierId = handler.Supplier.Id });
				}
			}
		}
	}
}