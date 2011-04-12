using AdminInterface.Helpers;
using AdminInterface.Models.Suppliers;
using AdminInterface.Models.Telephony;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Controller = AdminInterface.MonoRailExtentions.Controller;

namespace AdminInterface.Controllers
{
	[Layout("GeneralWithJQuery")]
	public class SuppliersController : Controller
	{
		public void Show(uint id)
		{
			var supplier = ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
			PropertyBag["supplier"] = supplier;
			PropertyBag["users"] = supplier.Users;
			PropertyBag["contactGroups"] = supplier.ContactGroupOwner.ContactGroups;
			PropertyBag["usersInfo"] = ADHelper.GetPartialUsersInformation(supplier.Users);

			PropertyBag["CallLogs"] = UnresolvedCall.LastCalls;
		}
	}
}