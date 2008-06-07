using System;

namespace AdminInterface.Models
{
	public class PaymentOptions
	{
		public PaymentOptions()
		{
			PaymentPeriodBeginDate = DateTime.Today;
		}

		public DateTime PaymentPeriodBeginDate { get; set; }
		public string Comment { get; set; }
		public bool WorkForFree { get; set; }

		public string GetCommentForPayer()
		{
			if (WorkForFree)
				return "Клиент обслуживается бесплатно";

			var result = String.Format("Дата начала платного периода: {0}", PaymentPeriodBeginDate.ToShortDateString());
			if (!String.IsNullOrEmpty(Comment))
				result += "\r\nКомментарий: " + Comment;

			return result;
		}
	}
}
