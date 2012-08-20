using System;
using AdminInterface.Controllers;
using AdminInterface.Models.Billing;
using Castle.Components.Validator;
using Common.Tools;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class PayersControllerFixture : ControllerFixture
	{
		private PayersController controller;
		private Payer payer;

		[SetUp]
		public new void Setup()
		{
			controller = new PayersController();
			Prepare(controller);

			payer = DataMother.CreatePayer();
			payer.Save();
		}

		[Test]
		public void Create_new_payer_for_payer_without_recipient()
		{
			referer = String.Format("http://localhost/Billing/edit?BillingCode={0}", payer.Id);
			PrepareController(controller);
			Request.Params.Add("payment.Sum", "500");

			controller.NewPayment(payer.Id, DateTime.Now.Year);
			var message = (Message)Context.Flash["Message"];
			Assert.That(message.IsError, Is.True);
			Assert.That(message.MessageText, Is.EqualTo("Получатель платежа не установлен"));
		}
	}
}