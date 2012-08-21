using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Models.Audit;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support;

namespace Integration
{
	[TestFixture]
	public class NewsFixture : IntegrationFixture
	{
		[Test]
		public void Notify_about_news_change_properties()
		{
			ForTest.InitializeMailer();
			MailMessage message = null;
			ChangeNotificationSender.Sender = ForTest.CreateStubSender(m => message = m);
			ChangeNotificationSender.UnderTest = true;

			Reopen();
			var news = new News {
				Header = "TestNewHeader",
				Body = "TestNewsBody",
				PublicationDate = DateTime.Now
			};
			Save(news);
			Reopen();
			news = session.Load<News>(news.Id);
			news.Body = "NewTestNewsBody";
			Save(news);
			Close();
			Assert.That(message, Is.Not.Null);
			Assert.That(message.Body, Is.StringEnding(String.Format(@"{1}<br>
test<br>
localhost<br>
Код {0}<br>
Новость <a href=""/News/{0}"">TestNewHeader</a><br>
Изменено 'Тело новости' </br> <b>было</b> 'TestNewsBody'</br><b>стало</b> 'NewTestNewsBody'
", news.Id, DateTime.Now)));
			Assert.That(message.To[0].ToString(), Is.EqualTo("AFNews@subscribe.analit.net"));
			Assert.That(message.Subject, Is.EqualTo("Изменено поле 'Тело новости'"));
		}
	}
}