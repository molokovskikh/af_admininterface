using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.ActiveRecordSupport;

namespace AdminInterface.Controllers
{
	public class PricesController : AdminInterfaceController
	{
		public PricesController()
		{
			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
		}

		public void Edit(uint id)
		{
			var price = DbSession.Load<Price>(id);
			price.PrepareEdit(DbSession);
			PropertyBag["price"] = price;

			if (IsPost) {
				BindObjectInstance(price, "price");
				if (IsValid(price)) {
					price.PrepareSave();
					DbSession.Save(price);
					Notify("Сохранено");
					RedirectToReferrer();
				}
			}
		}
	}
}