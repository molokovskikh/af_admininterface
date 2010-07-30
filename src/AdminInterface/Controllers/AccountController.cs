using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using NHibernate;

namespace AdminInterface.Controllers
{
	public class AccountController : SmartDispatcherController
	{
		public void Update(uint id, bool? status, bool? free, bool? accounted, decimal? payment)
		{
			var account = Accounting.TryFind(id);
			if (accounted != null || payment != null)
				UpdateAccounting(account.Id, accounted ?? account.BeAccounted, payment ?? account.Payment);
			if (status != null || free != null)
			{
				NHibernateUtil.Initialize(account);
				if (account is UserAccounting)
				{
					var user = ((UserAccounting)account).User;
					SetUserStatus(user.Id, status ?? user.Enabled, free ?? user.IsFree);
				}
				else
				{
					var address = ((AddressAccounting)account).Address;
					SetAddressStatus(address.Id, status ?? address.Enabled, free ?? address.IsFree);
				}
			}
			CancelView();
			CancelLayout();
		}

		public void SetUserStatus(uint userId, bool enabled, bool free)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var user = User.Find(userId);
				var oldStatus = user.Enabled;
				user.Enabled = enabled;
				user.IsFree = free;
				user.UpdateAndFlush();
				if (enabled && !oldStatus)
					Mailer.UserBackToWork(user);
				if (!enabled)
				{
					// Если это отключение, то проходим по адресам и
					// отключаем адрес, который доступен только отключенным пользователям
					foreach (var address in user.AvaliableAddresses)
					{
						if (address.AvaliableForEnabledUsers)
							continue;
						address.Enabled = false;
						address.Update();
					}
				}
				user.Client.UpdateBeAccounted();
				user.Client.Save();
				scope.VoteCommit();
			}
		}

		public void UpdateAccounting(uint accountId, bool accounted, decimal payment)
		{
			using (var transaction = new TransactionScope(OnDispose.Rollback))
			{
				var account = Accounting.Find(accountId);
				if (accounted)
					account.Accounted();
				else
					account.BeAccounted = false;
				account.Payment = payment;

				account.Update();
				transaction.VoteCommit();
			}
			CancelView();
		}

		public void SetAddressStatus(uint addressId, bool enabled, bool free)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var address = Address.Find(addressId);
				var oldStatus = address.Enabled;
				if (enabled && !oldStatus)
					Mailer.AddressBackToWork(address);
				address.Enabled = enabled;
				address.FreeFlag = free;
				address.Client.UpdateBeAccounted();
				address.Client.Save();

				scope.VoteCommit();
			}
		}
	}
}