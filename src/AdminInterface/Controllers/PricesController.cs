using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;

namespace AdminInterface.Controllers
{
	public class PricesController : AdminInterfaceController
	{
		public void Edit(uint id)
		{
			var price = DbSession.Load<Price>(id);
			PropertyBag["price"] = price;

			if (IsPost) {
				BindObjectInstance(price, "price");
				if (IsValid(price)) {
					DbSession.Save(price);
					Notify("Сохранено");
					RedirectToReferrer();
				}
			}
		}
	}
}