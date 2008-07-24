using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Controllers
{
	[TestFixture]
	public class RegisterControllerFixture : BaseControllerTest
	{
		private RegisterController _controller;

		[SetUp]
		public void Setup()
		{
			_controller = new RegisterController();
			PrepareController(_controller, "Registered");
			ForTest.InitialzeAR();
		}

		[Test]
		public void Append_to_payer_comment_comment_from_payment_options()
		{
			Payer payer;
			Client client;
			using (new TransactionScope())
			{
				client = ForTest.CreateClient();
				client.SaveAndFlush();

				payer = ForTest.CreatePayer();
				payer.Comment = "ata";
				payer.SaveAndFlush();
			}

			Context.Session["ShortName"] = "Test";

			var paymentOptions = new PaymentOptions { WorkForFree = true };
			_controller.Registered(payer, paymentOptions, client.Id, false);

			Assert.That(Payer.Find(payer.PayerID).Comment, Is.EqualTo("ata\r\nКлиент обслуживается бесплатно"));
		}
	}
}
