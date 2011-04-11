using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;

namespace AdminInterface.Controllers
{
	public class SuppliersController : Controller
	{
		public void Show(uint id)
		{
			PropertyBag["supplier"] = ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
		}
	}
}