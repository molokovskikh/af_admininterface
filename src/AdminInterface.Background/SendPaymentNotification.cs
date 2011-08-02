using System;
using System.Linq;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Tools;

namespace AdminInterface.Background
{
	public class SendPaymentNotification
	{
		private static string FirstNotificationText = @"Вами не оплачено обслуживание в ИС АналитФармация за текущий месяц.
Просим своевременно производить оплату (не позднее первой недели текущего месяца (авансом).";

		private static string SecondNotificationText = @"Вами не оплачено обслуживание в ИС АналитФармация за текущий месяц.
В случае отсутствия оплаты до 20 числа обслуживание будет приостановлено.";

		public void Process()
		{
			using(new SessionScope(FlushAction.Never))
			{
				var payers = ActiveRecordLinq.AsQueryable<Payer>()
					.Where(p => p.SendPaymentNotification && p.Balance < 0)
					.ToList();
				var usersForNotification = payers.SelectMany(p => p.Users).Where(u => u.Client != null);
				foreach (var user in usersForNotification)
				{
					var message = UserMessage.Find(user.Id);
					message.ShowMessageCount = 1;
					if (IsFirstNotification())
						message.Message = FirstNotificationText;
					else
						message.Message = SecondNotificationText;

					using(var scope = new TransactionScope(OnDispose.Rollback))
					{
						message.Save();
						scope.VoteCommit();
					}
				}
			}
		}

		private bool IsFirstNotification()
		{
			return SystemTime.Now().Day < 15;
		}
	}
}