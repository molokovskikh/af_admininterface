using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(ViewHelper)),
		Layout("General"),
		Secure(PermissionType.ViewDrugstore, PermissionType.ViewSuppliers)
	]
	public class OrdersController : SmartDispatcherController
	{
		public void Show()
		{
			PropertyBag["Orders"] = OrderView.FindNotSendedOrders();
		}
	}
}
