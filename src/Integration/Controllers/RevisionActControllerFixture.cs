﻿using System;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Models;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Controllers
{
	[TestFixture]
	public class RevisionActControllerFixture : ControllerFixture
	{
		private RevisionActsController controller;
		private Payer payer;

		[SetUp]
		public void Setup()
		{
			payer = DataMother.CreatePayerForBillingDocumentTest();
			controller = new RevisionActsController();
			PrepareController(controller, "RevisionActs", "Mail");
			((StubRequest)Request).UrlReferrer = "https://stat.analit.net/Adm/";
		}

		protected override IMockResponse BuildResponse(UrlInfo info)
		{
			return new StubResponse(info,
				new DefaultUrlBuilder(),
				new StubServerUtility(),
				new RouteMatch(),
				"https://stat.analit.net/Adm/");
		}

		[Test]
		public void Send_revision_act_with_comment()
		{
			controller.Mail(payer.Id, DateTime.Today.AddMonths(3), DateTime.Today, "test@analit.net", "Тестовое примечание");

			var message = notifications.First();
			Assert.That(message.Body, Is.StringContaining("Тестовое примечание"));
		}
	}
}