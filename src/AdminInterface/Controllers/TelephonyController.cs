using AdminInterface.Helpers;
using AdminInterface.Models.Telephony;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using NHibernate.Criterion;

namespace AdminInterface.Controllers
{
	[
		Secure,
		Helper(typeof(ViewHelper)),
		Layout("General"),
	]
	public class TelephonyController : SmartDispatcherController
	{
		public void Show()
		{
			PropertyBag["callbacks"] = Callback.FindAll(Order.Asc("Comment"));
		}

		public void Update([DataBind("callback")] Callback callback)
		{
			callback.Save();
			RedirectToAction("Show");
		}

		public void Edit(uint id)
		{
			PropertyBag["callback"] = Callback.Find(id);
		}

		public void New()
		{
			PropertyBag["callback"] = new Callback();
			RenderView("Edit");
		}

		public void Delete(uint id)
		{
			Callback.Find(id).Delete();
			RedirectToAction("Show");
		}
	}
}
