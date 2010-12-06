using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[Helper(typeof(BindingHelper))]
	public class PayersController : SmartDispatcherController
	{
		public void Payments(uint id)
		{
			var payer = Payer.Find(id);
			PropertyBag["payments"] = Payment.Queryable.Where(p => p.Payer == payer).ToList();
		}

		public void Invoices(uint id)
		{
			var payer = Payer.Find(id);
			PropertyBag["invoices"] = Invoice.Queryable.Where(p => p.Payer == payer).ToList();
		}
	}
}