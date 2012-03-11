using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using Common.Tools.Calendar;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Jobs;

namespace AdminInterface.Background
{
	public class SendPaymentNotification : MonthlyJob
	{
		private static string FirstNotificationText = @"Вами не оплачено обслуживание в ИС АналитФармация за текущий месяц.
Просим своевременно производить оплату (не позднее первой недели текущего месяца (авансом).";

		private static string SecondNotificationText = @"Вами не оплачено обслуживание в ИС АналитФармация за текущий месяц.
В случае отсутствия оплаты до 20 числа обслуживание будет приостановлено.";

		public SendPaymentNotification()
		{
			Action = Process;
		}

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
						ArHelper.WithSession(s => s.Save(new UserMessageSendLog(message)));
						message.Save();
						scope.VoteCommit();
					}
				}
			}
		}

		public override DateTime NextRun
		{
			get
			{
				var lazyMonthOffset = TimeSpan.Zero;
				var month = SystemTime.Now().Month;
				if (month == 1 || month == 5)
					lazyMonthOffset = 7.Days();

				if (IsFirstNotification())
					return CalculateNextRun(8.Day() + lazyMonthOffset);
				else
					return CalculateNextRun(15.Day() + lazyMonthOffset);
			}
		}

		private bool IsFirstNotification()
		{
			return SystemTime.Now().Day < 15;
		}
	}
}
