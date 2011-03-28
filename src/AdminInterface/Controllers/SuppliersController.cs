using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	public class SuppliersController : SmartDispatcherController
	{
		public void Show(uint id)
		{
			PropertyBag["supplier"] = ActiveRecordMediator<ServiceSupplier>.FindByPrimaryKey(id);
		}
	}
}