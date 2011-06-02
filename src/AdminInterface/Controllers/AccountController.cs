using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using NHibernate;

namespace AdminInterface.Controllers
{
	[Secure(PermissionType.Billing)]
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

		public void SetUserStatus(uint userId, bool? enabled, bool? free)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var user = User.Find(userId);
				var oldStatus = user.Enabled;
				if (enabled.HasValue)
					user.Enabled = enabled.Value;
				if (free.HasValue)
					user.IsFree = free.Value;
				user.UpdateAndFlush();
				if (enabled != oldStatus)
					this.Mail().EnableChanged(user).Send();
				if (enabled.HasValue && !enabled.Value)
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
				user.Client.Save();
				scope.VoteCommit();
			}
		}

		public void UpdateAccounting(uint accountId, bool? accounted, decimal? payment)
		{
			using (var transaction = new TransactionScope(OnDispose.Rollback))
			{
				var account = Accounting.Find(accountId);
				if (accounted.HasValue)
				{
					if (accounted.Value)
						account.Accounted();
					else
						account.BeAccounted = false;
				}
				if (payment.HasValue)
					account.Payment = payment.Value;

				account.Update();
				transaction.VoteCommit();
			}
			CancelView();
		}

		public void SetAddressStatus(uint addressId, bool? enabled, bool? free)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var address = Address.Find(addressId);
				var oldStatus = address.Enabled;
				if (enabled.HasValue)
					address.Enabled = enabled.Value;
				if (free.HasValue)
					address.FreeFlag = free.Value;
				if (enabled != oldStatus)
					this.Mail().EnableChanged(address).Send();
				address.Client.Save();

				scope.VoteCommit();
			}
		}
	}
}