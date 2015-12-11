using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;
using NHibernate.Linq;

namespace AdminInterface.Background
{
	public class UpdateAccountTask : Task
	{
		public int PageSize = 100;

		protected override void Process()
		{
			var paged = Page<Account>(a => !a.ReadyForAccounting, PageSize);
			PagedProcess(paged, Process);
		}

		private void Process(uint[] ids)
		{
			foreach (var id in ids) {
				var account = Session.Load<Account>(id);
				if (account.ObjectType == LogObjectType.User) {
					account = Session.Load<UserAccount>(id);
					var user = ((UserAccount)account).User;
					var afUpdateCount = Session.Query<UpdateLogEntity>().Count(u => u.User == user
						&& u.Commit
						&& (u.UpdateType == UpdateType.Accumulative || u.UpdateType == UpdateType.Cumulative));
					var afNetUpdateCount = Session.Query<RequestLog>().Count(u => u.User == user && u.IsConfirmed
						&& u.UpdateType == "MainController");
					if (afUpdateCount >= 10 || afNetUpdateCount >= 10) {
						account.ReadyForAccounting = true;
						Session.Save(account);
					}
				}
				else if (account.ObjectType == LogObjectType.Address) {
					account = Session.Load<AddressAccount>(id);
					var address = ((AddressAccount)account).Address;
					if (address.AvaliableForUsers.Any(u => u.Accounting.ReadyForAccounting)) {
						account.ReadyForAccounting = true;
						Session.Save(account);
					}
				}
			}
		}
	}
}