using AdminInterface.Models;
using AdminInterface.MonoRailExtentions;
using NHibernate.Criterion;

namespace AdminInterface.Controllers
{
	public class OrdersController : AdminInterfaceController
	{
		public void Details(uint id)
		{
			var order = DbSession.Load<ClientOrder>(id);
			PropertyBag["order"] = order;
			PropertyBag["lines"] = order.Lines(DbSession);
			CancelLayout();
		}
	}
}