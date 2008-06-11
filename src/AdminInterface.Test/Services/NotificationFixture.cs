﻿using System;
using System.Net.Mail;
using AdminInterface.Models;
using AdminInterface.Services;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Services
{
	[TestFixture]
	public class NotificationFixture
	{
		private NotificationService _service;
		private MailMessage _message;

		[SetUp]
		public void Setup()
		{
			_service = new NotificationService(message => _message = message);
		}

		[Test]
		public void BillingNotificationTest()
		{
			var clientCode = 58u;
			var payerId = 10u;
			var clientName = "Тестовый клиент";
			var paymentOptions = new PaymentOptions{ WorkForFree = true };
			var userName = "test";
			var isBasicSubmission = false;

			_service.SendNotificationToBillingAboutClientRegistration(clientCode,
			                                                          payerId,
			                                                          clientName,
			                                                          userName,
			                                                          isBasicSubmission,
			                                                          paymentOptions);

			Assert.That(_message, Is.Not.Null, "Сообщение не послано");
			Assert.That(_message.To.Count, Is.EqualTo(1));
			Assert.That(_message.To[0].Address, Is.EqualTo("billing@analit.net"));

			Assert.That(_message.From.Address, Is.EqualTo("register@analit.net"));

			Assert.That(_message.Subject, Is.EqualTo("Регистрация нового клиента"));
			Assert.That(_message.Body, Is.EqualTo(
@"Зарегистрирован новый клиент
<br>
Название: <a href='https://stat.analit.net/Adm/Billing/edit.rails?clientCode=58'>Тестовый клиент</a>
<br>
Код: 58
<br>
Биллинг код: 10
<br>
Кем зарегистрирован: test
<br>
Независимая копия
<br>
Клиент обслуживается бесплатно".Replace(Environment.NewLine, "")));
			Assert.That(_message.IsBodyHtml, Is.True);
		}

		[Test]
		public void Billing_notification_for_client_with_basic_submission()
		{
			var clientCode = 58u;
			var payerId = 10u;
			var clientName = "Тестовый клиент";
			var paymentOptions = new PaymentOptions { PaymentPeriodBeginDate = new DateTime(2007, 1, 1), Comment = "Test comment"};
			var userName = "test";
			var isBasicSubmission = true;

			_service.SendNotificationToBillingAboutClientRegistration(clientCode,
																	  payerId,
																	  clientName,
																	  userName,
																	  isBasicSubmission,
																	  paymentOptions);

			Assert.That(_message.Body, Is.EqualTo(
@"Зарегистрирован новый клиент
<br>
Название: <a href='https://stat.analit.net/Adm/Billing/edit.rails?clientCode=58'>Тестовый клиент</a>
<br>
Код: 58
<br>
Биллинг код: 10
<br>
Кем зарегистрирован: test
<br>
Клиент с Базовым подчинением
<br>
Дата начала платного периода: 01.01.2007
<br>
Комментарий: Test comment".Replace(Environment.NewLine, "")));
		}

		[Test]
		public void Billing_notification_for_client_without_payment_options()
		{
			var clientCode = 58u;
			var payerId = 10u;
			var clientName = "Тестовый клиент";
			var userName = "test";
			var isBasicSubmission = true;

			_service.SendNotificationToBillingAboutClientRegistration(clientCode,
																	  payerId,
																	  clientName,
																	  userName,
																	  isBasicSubmission,
																	  null);

			Assert.That(_message.Body, Is.EqualTo(
@"Зарегистрирован новый клиент
<br>
Название: <a href='https://stat.analit.net/Adm/Billing/edit.rails?clientCode=58'>Тестовый клиент</a>
<br>
Код: 58
<br>
Биллинг код: 10
<br>
Кем зарегистрирован: test
<br>
Клиент с Базовым подчинением".Replace(Environment.NewLine, "")));
		}
	}
}