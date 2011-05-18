using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;

namespace AdminInterface.Controllers
{
	[Layout("GeneralWithJQueryOnly")]
	public class IgnoredInnsController : ARSmartDispatcherController
	{
		public void Index()
		{
			var inns = IgnoredInn.Queryable.OrderBy(r => r.Inn).ToList();
			if (IsPost)
			{
				var forSave = (IgnoredInn[])BindObject(ParamStore.Form, typeof(IgnoredInn[]), "inns", AutoLoadBehavior.NewInstanceIfInvalidKey);
				var deleted = inns.Where(r => !forSave.Any(n => n.Id == r.Id));
				deleted.Each(d => d.Delete());
				foreach (var inn in forSave)
					inn.Save();
				Flash["Message"] = Message.Notify("Сохранено");
				RedirectToAction("Index");
			}
			else
			{
				PropertyBag["inns"] = inns;
			}
		}
	}
}