using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models.Billing;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class ActFixture : IntegrationFixture
	{
		[Test]
		public void Do_not_build_duplicate_document()
		{
			var payer = DataMother.BuildPayerForBillingDocumentTest();
			var invoice = new Invoice(payer, Period.December, DateTime.Now);
			invoice.Save();

			var acts = Act.Build(new List<Invoice> { invoice }, DateTime.Now);
			acts.Each(a => a.Save());

			Assert.That(acts.Count(), Is.EqualTo(1));

			acts = Act.Build(new List<Invoice> { invoice }, DateTime.Now);
			Assert.That(acts.Count(), Is.EqualTo(0));
		}
	}
}