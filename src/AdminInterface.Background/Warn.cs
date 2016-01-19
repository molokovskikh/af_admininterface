using System;
using System.Configuration;
using System.Linq;
using AdminInterface.Models;
using Common.Tools;
using Common.Tools.Calendar;
using Common.Web.Ui.Helpers;
using NHibernate;

namespace AdminInterface.Background
{
	public class Warn : Task
	{
		public Warn()
		{
		}

		public Warn(ISession session) : base(session)
		{
		}

		protected override void Process()
		{
			var threashold = 2;
			//первый воскресенье
			var wieght = new decimal[7] {
				0.5m,
				2,
				1,
				1,
				2,
				1,
				0.5m
			};

			var ids = Session.CreateSQLQuery(@"
select u.Id
from Customers.Users u
join Customers.Clients c on c.Id = u.ClientId
join Usersettings.RetClientsSet rcs on rcs.ClientCode = c.Id
where c.Status = 1
	and rcs.ServiceClient = 0
	and u.Enabled = 1
	and u.DoNotCheckWellBeing = 0")
				.List<uint>();
			var end = SystemTime.Today().FirstDayOfWeek();
			var begin = SystemTime.Today().AddDays(-7).FirstDayOfWeek();
			foreach (var id in ids) {
				var afUpdateDates = Session.CreateSQLQuery(@"
select Date(RequestTime) from logs.AnalitFUpdates u
where RequestTime > :begin and RequestTime < :end
	and Commit = 1
	and UserId = :userId
group by Date(RequestTime)")
					.SetParameter("begin", begin)
					.SetParameter("end", end)
					.SetParameter("userId", id)
					.List<DateTime>();

				var afNetUpdateDates = Session.CreateSQLQuery(@"
select Date(CreatedOn) from logs.RequestLogs u
where CreatedOn > :begin and CreatedOn < :end
	and IsCompleted = 1
	and IsConfirmed = 1
	and UserId = :userId
group by Date(CreatedOn)")
					.SetParameter("begin", begin)
					.SetParameter("end", end)
					.SetParameter("userId", id)
					.List<DateTime>();
				var total = afNetUpdateDates.Concat(afUpdateDates).Distinct().Select(x => wieght[(int)x.DayOfWeek]).Sum();
				if (total < threashold) {
					var user = Session.Load<User>(id);
					CreateIssue($"Пользователь {user.Id} не обновлялся",
						$"Пользователь {user.LoginAndName} клиента {user.Client.Name} в регионе {user.Client.HomeRegion.Name}"
						+ $" не обновлялся за период с {begin:d} до {end.AddDays(-1):d}");
				}

				CheckOrdering(begin, end, id);
			}
		}

		private static void CreateIssue(string subject, string body)
		{
			var url = ConfigurationManager.AppSettings["RedmineWarnUrl"];
			Redmine.CreateIssue(url,
				subject,
				body,
				ConfigurationManager.AppSettings["RedmineWarnAssignTo"]);
		}

		private void CheckOrdering(DateTime begin, DateTime end, uint id)
		{
			var upperThreashold = 30;
			var lowwerThreashold = 30;
			var sum = Session.CreateSQLQuery(@"select sum(ol.Cost * ol.Quantity)
from Orders.OrdersHead oh
	join Orders.OrdersList ol on ol.OrderId = oh.RowId
where oh.UserId = :userId
	and oh.Processed = 1
	and oh.WriteTime > :begin
	and oh.WriteTime < :end")
				.SetParameter("begin", begin)
				.SetParameter("end", end)
				.SetParameter("userId", id)
				.List<decimal?>()
				.FirstOrDefault()
				.GetValueOrDefault();
			var user = Session.Load<User>(id);
			if (user.LastOrderSum == 0) {
				user.LastOrderSum = sum;
				return;
			}
			var delta = ((sum - user.LastOrderSum) / user.LastOrderSum) * 100m;
			user.OrderSumDelta += delta;
			var orderThreashold = user.OrderSumDelta > 0 ? upperThreashold : lowwerThreashold;
			if (Math.Abs(user.OrderSumDelta) > orderThreashold) {
				if (user.OrderSumDelta < 0) {
					CreateIssue($"Падение объема закупок пользователя {user.Id}",
						$"Для пользователя {user.LoginAndName} клиента {user.Client.Name} в регионе {user.Client.HomeRegion.Name}\r\n"
						+ $"объем закупок уменьшился на {Math.Abs(user.OrderSumDelta):#.##}%\r\n"
						+ $"объем закупок с {begin:d} до {end.AddDays(-1):d} - {sum:C}\r\n"
						+ $"объем закупок в предыдущем периоде {user.LastOrderSum:C}");
				}
				user.OrderSumDelta = 0;
			}
			else {
				user.OrderSumDelta += delta;
			}
			user.LastOrderSum = sum;
		}
	}
}