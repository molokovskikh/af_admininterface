using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;

namespace AdminInterface.Controllers
{
	[
		Layout("GeneralWithJQueryOnly"),
	]
	public class RecipientsController : ARSmartDispatcherController
	{
		public void Index()
		{
			var recipients = Recipient.Queryable.OrderBy(r => r.Name).ToList();
			if (IsPost)
			{
				var forSave = (Recipient[])BindObject(ParamStore.Form, typeof(Recipient[]), "recipients", AutoLoadBehavior.NewInstanceIfInvalidKey);
				var deleted = recipients.Where(r => !forSave.Any(n => n.Id == r.Id));
				deleted.Each(d => d.Delete());
				foreach (var recipient in forSave)
					recipient.Save();
				Flash["Message"] = Message.Notify("Сохранено");
				RedirectToAction("Index");
			}
			else
			{
				PropertyBag["Recipients"] = recipients;
			}
		}

		public void Edit(uint id)
		{
			var recipient = Recipient.Find(id);
			if (IsPost)
			{
				BindObjectInstance(recipient, "recipient");
				recipient.Save();
				RedirectToReferrer();
			}
			else
			{
				PropertyBag["recipient"] = recipient;
			}
		}
	}
}