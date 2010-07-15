using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;

namespace AdminInterface.Controllers
{
	[
		Secure(PermissionType.Billing),
		Layout("GeneralWithJQueryOnly"),
	]
	public class RecipientController : ARSmartDispatcherController
	{
		public void Show()
		{
			PropertyBag["Recipients"] = Recipient.Queryable.OrderBy(r => r.Name).ToList();
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
			RedirectToAction("show");
		}
	}
}