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
	public class ActFixture : Test.Support.IntegrationFixture
	{
		[Test]
		public void Do_not_build_duplicate_document()
		{
			var payer = DataMother.CreatePayerForBillingDocumentTest();
			var invoice = new Invoice(payer, new Period(2010, Interval.December), DateTime.Now);
			invoice.Save();

			var acts = Act.Build(new List<Invoice> { invoice }, DateTime.Now);
			acts.Each(a => a.Save());

			Assert.That(acts.Count(), Is.EqualTo(1));

			acts = Act.Build(new List<Invoice> { invoice }, DateTime.Now);
			Assert.That(acts.Count(), Is.EqualTo(0));
		}
	}
}