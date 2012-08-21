using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class RecipientsController : AdminInterfaceController
	{
		public RecipientsController()
		{
			SetARDataBinder();
		}

		public void Index()
		{
			var recipients = DbSession.Query<Recipient>().OrderBy(r => r.Name).ToList();
			if (IsPost) {
				var forSave = (Recipient[])BindObject(ParamStore.Form, typeof(Recipient[]), "recipients", AutoLoadBehavior.NewInstanceIfInvalidKey);
				var deleted = recipients.Where(r => !forSave.Any(n => n.Id == r.Id));
				deleted.Each(d => DbSession.Delete(d));
				foreach (var recipient in forSave)
					DbSession.Save(recipient);
				Notify("Сохранено");
				RedirectToAction("Index");
			}
			else {
				PropertyBag["Recipients"] = recipients;
			}
		}

		public void Edit(uint id)
		{
			var recipient = DbSession.Load<Recipient>(id);
			if (IsPost) {
				BindObjectInstance(recipient, "recipient");
				DbSession.Save(recipient);
				Notify("Сохранено");
				RedirectToReferrer();
			}
			else {
				PropertyBag["recipient"] = recipient;
			}
		}
	}
}