using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using AdminInterface.Mailers;
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
		private MailMessage message;
		private News news;
		[SetUp]
		public void SetUp()
		{
			ForTest.InitializeMailer();
			message = null;
			news = new News {
				Header = "TestNewHeader",
				Body = "TestNewsBody",
				PublicationDate = DateTime.Now
			};
			Save(news);
		}
		[Test]
		public void Notify_about_news_change_properties()
		{
			ChangeNotificationSender.Sender = ForTest.CreateStubSender(m => message = m);
			ChangeNotificationSender.UnderTest = true;

			Reopen();
			news = session.Load<News>(news.Id);
			news.Body = "NewTestNewsBody";
			Save(news);
			Close();
			Assert.That(message, Is.Not.Null);
			Assert.That(message.Body, Is.StringEnding(String.Format(@"Дата изменения: {1}<br>
Сотрудник: test<br>
Хост: localhost<br>
Для разработчиков: Код {0}<br>
Новость: <a href=""/News/{0}"">TestNewHeader</a><br>
Адресат: {2}<br>
Дата и время публикации новости: {3}<br>
Изменено 'Тело новости' </br> <b>было</b> 'TestNewsBody'</br><b>стало</b> 'NewTestNewsBody'
", news.Id, DateTime.Now, news.DestinationType.GetDescription(), news.PublicationDate)));
			Assert.That(message.To[0].ToString(), Is.EqualTo("AFNews@subscribe.analit.net"));
			Assert.That(message.Subject, Is.EqualTo("Изменено поле 'Тело новости'"));
		}

		[Test]
		public void DeleteNewsNotifyTest()
		{
			var mailer = ForTest.TestMailer(m => message = m);
			mailer.RegisterOrDeleteNews(news, "testmail@test.te", "Скрыта новость").Send();
			Assert.That(message, Is.Not.Null);
			Assert.That(message.Body, Is.StringEnding(String.Format(@"Дата изменения: {1}<br>
Сотрудник: test<br>
Хост: localhost<br>
Для разработчиков: Код {0}<br>
Новость: <a href=""https://stat.analit.net/adm/News/{0}"">TestNewHeader</a><br>

Скрыта новость: <br/>
Тема: {3}</br>
Адресат: {2}</br>
Текст: {4}</br>", news.Id, DateTime.Now, news.DestinationType.GetDescription(), news.Header, news.Body)));
			Assert.That(message.To[0].ToString(), Is.EqualTo("testmail@test.te"));
			Assert.That(message.Subject, Is.EqualTo("Скрыта новость"));
		}

		[Test]
		public void NoNotifyIfDeleted()
		{
			ChangeNotificationSender.Sender = ForTest.CreateStubSender(m => message = m);
			ChangeNotificationSender.UnderTest = true;
			news.Deleted = true;
			Save(news);
			news.Body = "NewTestNewsBody";
			Save(news);
			Close();
			Assert.That(message, Is.Null);
		}
	}
}
