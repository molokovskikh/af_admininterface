using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.MonoRail.TestSupport;
using Common.Tools;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class SmapRejectorControllerFixture : BaseControllerTest
	{
		private SmapRejectorController controller;

		[SetUp]
		public void SetUp()
		{
			controller = new SmapRejectorController();
			PrepareController(controller);
			using (new TransactionScope())
			{
				RejectedEmail.Queryable
					.Where(r => r.LogTime >= DateTime.Today.AddDays(-1))
					.Each(r => r.Delete());

				RejectedEmail.Queryable
					.Where(r => r.LogTime >= new DateTime(2008, 10, 1) && r.LogTime <= new DateTime(2008, 10, 4))
					.Each(r => r.Delete());
			}
		}

		[Test]
		public void Show_method_should_return_rejected_messages_for_current_day()
		{
			var mail1 = new RejectedEmail { SmtpId = 64789, Comment = "нафиг", LogTime = DateTime.Now, From = "tech@analit.net", Subject = "test"};
			mail1.SaveAndFlush();
			var mail2 = new RejectedEmail { SmtpId = 65469, Comment = "еще нафиг", LogTime = DateTime.Now, From = "tech@analit.net", Subject = "test"};
			mail2.SaveAndFlush();
			var mail3 = new RejectedEmail { SmtpId = 64619, Comment = "снова нафиг", LogTime = DateTime.Now.AddDays(-1), From = "tech@analit.net", Subject = "test"};
			mail3.SaveAndFlush();

			controller.Show();

			var mails = (RejectedEmail[]) ControllerContext.PropertyBag["rejects"];
			Assert.That(mails.Length, Is.EqualTo(2));
			Assert.That(mails[0].Id, Is.EqualTo(mail1.Id));
			Assert.That(mails[1].Id, Is.EqualTo(mail2.Id));
			Assert.That(ControllerContext.PropertyBag["fromDate"], Is.EqualTo(DateTime.Today));
			Assert.That(ControllerContext.PropertyBag["toDate"], Is.EqualTo(DateTime.Today));
		}

		[Test]
		public void Search_should_find_rejects_with_given_text_in_from_and_subject_for_given_period()
		{
			var mail1 = new RejectedEmail { SmtpId = 64789, Comment = "нафиг", LogTime = new DateTime(2008, 10, 1, 12, 10, 00), From = "test@analit.net", Subject = "Самоучитель Windows Vista" };
			mail1.SaveAndFlush();
			var mail2 = new RejectedEmail { SmtpId = 65469, Comment = "нафиг", LogTime = new DateTime(2008, 10, 3, 15, 40, 00), From = "tech@analit.net", Subject = "Увеличение объемов продаж test" };
			mail2.SaveAndFlush();
			var mail3 = new RejectedEmail { SmtpId = 4613, Comment = "нафиг", LogTime = new DateTime(2008, 10, 2, 13, 1, 00), From = "tech@analit.net", Subject = "Увеличение объемов продаж" };
			mail3.SaveAndFlush();
			var mail4 = new RejectedEmail { SmtpId = 4634, Comment = "нафиг", LogTime = new DateTime(2008, 9, 30, 12, 10, 0), From = "test@analit.net", Subject = "Увеличение объемов продаж" };
			mail4.SaveAndFlush();
			var mail5 = new RejectedEmail { SmtpId = 4497, Comment = "нафиг", LogTime = new DateTime(2008, 10, 4, 8, 10, 0), From = "test@analit.net", Subject = "Увеличение объемов продаж" };
			mail5.SaveAndFlush();

			controller.Search("test", new DateTime(2008, 10, 1), new DateTime(2008, 10, 3));

			var mails = (RejectedEmail[])ControllerContext.PropertyBag["rejects"];
			Assert.That(mails.Length, Is.EqualTo(2));
			Assert.That(mails[0].Id, Is.EqualTo(mail1.Id));
			Assert.That(mails[1].Id, Is.EqualTo(mail2.Id));
			Assert.That(ControllerContext.PropertyBag["fromDate"], Is.EqualTo(new DateTime(2008, 10, 1)));
			Assert.That(ControllerContext.PropertyBag["toDate"], Is.EqualTo(new DateTime(2008, 10, 3)));
			Assert.That(ControllerContext.PropertyBag["searchText"], Is.EqualTo("test"));
		}
	}
}
