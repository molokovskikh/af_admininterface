using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;

namespace AdminInterface.Controllers
{
	public class BalanceOperationsController : AdminInterfaceController
	{
		public void Show(uint id)
		{
			var operation = ActiveRecordMediator<BalanceOperation>.FindByPrimaryKey(id);
			PropertyBag["operation"] = operation;
		}
	}
}