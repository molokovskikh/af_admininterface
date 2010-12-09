using System.Linq;
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
			PropertyBag["Recipients"] = Recipient.Queryable.OrderBy(r => r.Name).ToList();
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

		public void Update([ARDataBind("recipients", AutoLoad = AutoLoadBehavior.NewInstanceIfInvalidKey)] Recipient[] recipients)
		{
			using (var transaction = new TransactionScope(OnDispose.Rollback))
			{
				var all = Recipient.Queryable.ToList();
				var deleted = all.Where(r => !recipients.Any(n => n.Id == r.Id));
				deleted.Each(d => d.Delete());
				foreach (var recipient in recipients)
				{
					if (recipient.Id == 0)
						recipient.Save();
					else
						recipient.Save();
				}
				transaction.VoteCommit();
			}
			Flash["isUpdated"] = true;
			RedirectToAction("Index");
		}
	}
}