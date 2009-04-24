﻿using System.Threading;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class ClientCloneFixture : WatinFixture
	{
		[Test]
		public void Try_to_clone_client()
		{
			using (var browser = new IE(BuildTestUrl("CopySynonym.aspx")))
			{
				browser.TextField(Find.ById("ctl00_MainContentPlaceHolder_FromTB")).TypeText("ТестерК");
				browser.TextField(Find.ById("ctl00_MainContentPlaceHolder_ToTB")).TypeText("ТестерК2");
				browser.Button(Find.ByValue("Найти")).Click();
				browser.Button(Find.ByValue("Присвоить")).Click();
				Assert.That(browser.Text, Text.Contains("Присвоение успешно завершен"));
			}
		}

		[Test]
		public void Try_to_update_general_info()
		{
			using (var browser = new IE(BuildTestUrl("client/3616")))
			{
				browser.Input<Client>(client => client.FullName, "ТестерК2");
				browser.Input<Client>(client => client.ShortName, "ТестерК2");
				browser.Input<Client>(client => client.Address, "-");
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Text.Contains("Сохранено"));
			}
		}

		[Test]
		public void Try_to_send_message()
		{
			using (var browser = new IE(BuildTestUrl("client/3616")))
			{
				browser.TextField(Find.ByName("message")).TypeText("тестовое сообщение");
				browser.Button(Find.ByValue("Принять")).Click();
				Assert.That(browser.Text, Text.Contains("Сохранено"));
			}
		}

		[Test]
		public void Change_password()
		{
			using (var browser = new IE(BuildTestUrl("client/3616")))
			{
				browser.Link(Find.ByText("KvasovT")).Click();
				using (var openedWindow = IE.AttachToIE(Find.ByTitle("Изменение пароля для клиента ТестерК2 [login: KvasovT]")))
				{
					openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
					openedWindow.Button(Find.ByValue("Изменить")).Click();
					Assert.That(openedWindow.Text, Text.Contains("Пароль успешно изменен"));
				}
			}
		}
	}
}