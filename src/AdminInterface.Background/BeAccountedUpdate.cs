using System;
using System.Configuration;
using System.Linq;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using NHibernate.Linq;

namespace AdminInterface.Background
{
	public class BeAccountedUpdate : Task
	{
		protected override void Process()
		{
			var timeToRunRaw = ConfigurationManager.AppSettings["BeAccountedUpdateAt"]
				.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);

			var timeToRunHour = int.Parse(timeToRunRaw[0]);
			var timeToRunMinutes = timeToRunRaw.Length > 1 ? int.Parse(timeToRunRaw[1]) : 0;
			var runTime = SystemTime.Now().Date.AddHours(timeToRunHour).AddMinutes(timeToRunMinutes);

			if (SystemTime.Now() >= runTime && SystemTime.Now() < runTime.AddMinutes(30)) {
				var allResult = Session.Query<Account>().Where(a =>
					a.BeAccounted && a.Payment > 0 && a.IsFree && a.FreePeriodEnd != null && a.FreePeriodEnd < SystemTime.Now()).ToList();
				foreach (var item in allResult) {
					item.BeAccounted = false;
					Session.Save(item);
				}
			}
		}
	}
}