using System.IO;
using System.Web;
using System.Web.Hosting;
using AdminInterface.Controllers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Castle.MonoRail.TestSupport;
using NUnit.Framework;
using Functional.ForTesting;


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
			SecurityContext.GetAdministrator = () => new Administrator {UserName = "TestAdmin"};
			PrepareController(_controller, "Registered");
			ForTest.InitialzeAR();
		}

		[Test, Ignore("Чинить. Нужно вместо null передавать объект JuridicalOrganisation")]
		public void Append_to_payer_comment_comment_from_payment_options()
		{
			var workerRequest = new SimpleWorkerRequest("", "", "", "http://test", new StreamWriter(new MemoryStream()));
			var context = new HttpContext(workerRequest);
			HttpContext.Current = context;
			
			Client client = DataMother.CreateTestClient();
			Payer payer = client.BillingInstance;
			payer.Comment = "ata";
			payer.Update();
			Context.Session["ShortName"] = "Test";

			var paymentOptions = new PaymentOptions { WorkForFree = true };
			_controller.Registered(payer, null, paymentOptions, client.Id, false);

			Assert.That(Payer.Find(payer.PayerID).Comment, Is.EqualTo("ata\r\nКлиент обслуживается бесплатно"));
		}
	}
}
