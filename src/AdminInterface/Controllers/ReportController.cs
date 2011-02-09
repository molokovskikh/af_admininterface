using System;
using System.Linq;
using System.ComponentModel;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	public enum Period
	{
		[Description("Первый квартал")] FirstQuarter,
		[Description("Второй квартал")] SecondQuarter,
		[Description("Третий квартал")] ThirdQuarter,
		[Description("Четвертый квартал")] FourthQuarter,
		[Description("Январь")] January,
		[Description("Февраль")] February,
		[Description("Март")] March,
		[Description("Апрель")] April,
		[Description("Май")] May,
		[Description("Июнь")] June,
		[Description("Июль")] July,
		[Description("Август")] August,
		[Description("Сентябрь")] September,
		[Description("Ноябрь")] November,
		[Description("Октябрь")] October,
		[Description("Декабрь")] December,
	}

	[
		Layout("Report"),
		Helper(typeof(ViewHelper)),
		Rescue("Fail", typeof(EndUserException)),
		Secure(PermissionType.Billing)
	]
	public class ReportController : SmartDispatcherController
	{
		public void RevisionAct(uint payerId, DateTime from, DateTime to)
		{
/*			var payer = Payer.Find(payerId);
			PropertyBag["payer"] = payer;
			PropertyBag["credit"] = payer.CreditOn(from);
			PropertyBag["debit"] = payer.DebitOn(from);
			PropertyBag["movements"] = payer.FindPayments(from, to);
			PropertyBag["fromDate"] = from;
			PropertyBag["toDate"] = to;*/
		}
		
		public void Contract(uint payerId)
		{
			CancelLayout();
			var payer = Payer.Find(payerId);
			PropertyBag["payer"] = payer;
		}
	}
}
