using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Models
{
	[TestFixture]
	public class PaymentOptionsFixture
	{
		public void CommentTest()
		{
			var paymentOptions = new PaymentOptions();
			paymentOptions.ClientServForFree = true;

			Assert.That(paymentOptions.GetCommentAddion(), Is.EqualTo("Клиент обслуживается бессплатно"));
		}
	}
}
