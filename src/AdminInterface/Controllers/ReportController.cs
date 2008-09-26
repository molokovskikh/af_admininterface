using System;
using System.ComponentModel;
using System.Globalization;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using NHibernate.Criterion;

namespace AdminInterface.Controllers
{
	public enum Period
	{
		[Description("Первый квартал")] FirstQuarter,
		[Description("Второй квартал")] SecondQuarter,
		[Description("Третий квартал")] ThirdQuarter,
		[Description("Четвертый квартал")] FourthQuarterJanuary,
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

	public static class PeriodExtension
	{
		public static DateTime GetPeriodBegin(this Period period)
		{
			switch (period)
			{
				case Period.August:
					return new DateTime(2008, 8, 1);
				case Period.September:
					return new DateTime(2008, 9, 1);
				case Period.July:
					return new DateTime(2008, 7, 1);
				default:
					throw new NotImplementedException();
			}
		}

		public static DateTime GetPeriodEnd(this Period period)
		{
			switch (period)
			{
				case Period.August:
					return new DateTime(2008, 8, 31);
				case Period.September:
					return new DateTime(2008, 9, 30);
				case Period.July:
					return new DateTime(2008, 7, 31);
				default:
					throw new NotImplementedException();
			}
		}
	}

	[
		Rescue("Fail", typeof(EndUserException)),
		Secure(PermissionType.Billing)
	]
	public class ReportController : SmartDispatcherController
	{
		public void Bill(uint payerId, Period period)
		{
			var payer = Payer.Find(payerId);
			CheckReciver(payer);
			PropertyBag["payer"] = payer;
			PropertyBag["fromDate"] = DateTime.Now;
			PropertyBag["bills"] = AdminInterface.Models.Billing.Bill.FindAll(Order.Asc("On"),
			                                                                  Expression.Eq("For", payer) &&
			                                                                  Expression.Between("On", period.GetPeriodBegin(),
			                                                                                     period.GetPeriodEnd()));
			PropertyBag["Number"] = 1;
		}

		public void Act(uint payerId, Period period)
		{
			var payer = Payer.Find(payerId);
			CheckReciver(payer);
			PropertyBag["payer"] = payer;
			PropertyBag["fromDate"] = DateTime.Now;
			PropertyBag["acts"] = AdminInterface.Models.Billing.Bill.FindAll(Order.Asc("On"),
			                                                                 Expression.Eq("For", payer) &&
			                                                                 Expression.Between("On", period.GetPeriodBegin(),
			                                                                                    period.GetPeriodEnd()));
			PropertyBag["Number"] = 1;
		}

		public void RevisionAct(uint payerId, DateTime from, DateTime to)
		{
			var payer = Payer.Find(payerId);
			CheckReciver(payer);
			PropertyBag["payer"] = payer;
			PropertyBag["movements"] = AdminInterface.Models.Billing.MoneyMove.FindAll(Order.Asc("On"),
			                                                                           Expression.Eq("For", payer) &&
			                                                                           Expression.Between("On", from, to));
			PropertyBag["fromDate"] = from;
		}

		private void CheckReciver(Payer payer)
		{
			if (payer.Reciver == null)
				throw new EndUserException(
					String.Format("Не погу сформировать отчет т.к. платильщика {0} не установлен получатель платежей",
								  payer.ShortName));
		}
	}
}
