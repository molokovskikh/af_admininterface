using System;
using AdminInterface.Models;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class PaymentOptionsFixture
	{
		[Test]
		public void WorkForFreeTes()
		{
			var paymentOptions = new PaymentOptions();
			paymentOptions.Comment = "123";
			paymentOptions.WorkForFree = true;

			Assert.That(paymentOptions.GetCommentForPayer(), Is.EqualTo("Клиент обслуживается бесплатно"));
		}

		[Test]
		public void CommentAndPayDateTest()
		{
			var paymentOptions = new PaymentOptions();
			paymentOptions.PaymentPeriodBeginDate = new DateTime(2008, 1, 1);
			paymentOptions.Comment = @"алалала-алал
алала";

			Assert.That(paymentOptions.GetCommentForPayer(), 
				Is.EqualTo(
@"Дата начала платного периода: 01.01.2008
Комментарий: алалала-алал
алала"));
			paymentOptions.Comment = null;
			Assert.That(paymentOptions.GetCommentForPayer(),Is.EqualTo("Дата начала платного периода: 01.01.2008"));
		}
	}
}
