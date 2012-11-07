using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Controllers;
using AdminInterface.Controllers.Filters;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class ActsControllerFixture : ControllerFixture
	{
		private ActsController _controller;
		private Payer _payer;
		private Invoice _invoice;
		[SetUp]
		public void SetUp()
		{
			_payer = DataMother.CreatePayer();
			session.Save(_payer);
			_controller = new ActsController();
			Prepare(_controller);
			_invoice = new Invoice(_payer,
				new Period(2012, Interval.January),
				new DateTime(2012, 1, 10),
				new List<InvoicePart> { new InvoicePart(null, "Мониторинг оптового фармрынка за декабрь", 500, 2, DateTime.Now) }) {
					Id = 1,
				};
			session.Save(_invoice);
		}

		[Test]
		public void NoBuildActsIfExistsTest()
		{
			var act = new Act {
				Payer = _payer,
				Period = new Period(2012, Interval.January)
			};
			session.Save(act);
			session.Flush();
			var filter = new DocumentBuilderFilter {
				PayerId = new[] { _payer.Id },
				Period = new Period(2012, Interval.January)
			};
			_controller.Build(filter, DateTime.Now);
			var acts = session.Query<Act>().Where(a => a.Payer == _payer && a.Period == new Period(2012, Interval.January));
			Assert.That(acts.Count(), Is.EqualTo(1));
		}

		[Test]
		public void BuildActsTest()
		{
			var filter = new DocumentBuilderFilter {
				PayerId = new[] { _payer.Id },
				Period = new Period(2012, Interval.January)
			};
			_controller.Build(filter, DateTime.Now);
			var acts = session.Query<Act>().Where(a => a.Payer == _payer && a.Period == new Period(2012, Interval.January));
			var invoice = session.Load<Invoice>(_invoice.Id);
			Assert.That(acts.Count(), Is.EqualTo(1));
			Assert.That(invoice.Act, Is.EqualTo(acts.First()));
		}
	}
}
