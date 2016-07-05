using System;
using System.Configuration;
using System.Linq;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Core.Smtp;
using Common.Tools;
using Common.Tools.Calendar;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.Models.Jobs;
using Common.Web.Ui.MonoRailExtentions;

namespace AdminInterface.Background
{
	public class SendPremoderatedPomotionList : Task
	{
		private MonorailMailer _mailer;

		public SendPremoderatedPomotionList()
		{
		}

		public SendPremoderatedPomotionList(MonorailMailer mailer)
		{
			_mailer = mailer;
		}

		protected override void Process()
		{
			var timeToSendMail = ConfigurationManager.AppSettings["SendPremoderatedPomotionListAt"]
				.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

			var timeToSendMailHour = int.Parse(timeToSendMail[0]);
			var timeToSendMailMinutes = timeToSendMail.Length > 1 ? int.Parse(timeToSendMail[1]) : 0;
			var mailTime = SystemTime.Now().Date.AddHours(timeToSendMailHour).AddMinutes(timeToSendMailMinutes);

			if (SystemTime.Now() >= mailTime && SystemTime.Now() < mailTime.AddMinutes(30)) {
				using (new SessionScope(FlushAction.Never)) {
					var promotions = ActiveRecordLinq.AsQueryable<SupplierPromotion>()
						.Where(p => !p.Moderated && p.Enabled).OrderBy(s=>s.Begin).ToList();
					if (promotions.Count > 0) {
						_mailer = (_mailer ?? new MonorailMailer()).PremoderatedPromotions(promotions);
						_mailer.Send();
					}
				}
			}
		}
	}
}