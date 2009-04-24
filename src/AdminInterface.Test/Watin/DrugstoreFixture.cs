using System.Linq;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Common.Tools;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class DrugstoreFixture : WatinFixture
	{
		[Test]
		public void Try_to_send_email_notification()
		{
			using (var browser = new IE(BuildTestUrl("manageret.aspx?cc=2575")))
			{
				browser.Button(Find.ByValue("Отправить уведомления о регистрации поставщикам")).Click();
				Assert.That(browser.ContainsText("Конфигурация клиента"));
			}
		}

		[Test]
		public void Try_to_change_home_region_for_drugstore()
		{
			using (var browser = new IE(BuildTestUrl("manageret.aspx?cc=2575")))
			{
				var homeRegionSelect = GetHomeRegionSelect(browser);
				var changeTo = homeRegionSelect.Options.Skip(1).First(o => o.Value != homeRegionSelect.SelectedOption.Value).Text;
				homeRegionSelect.Select(changeTo);
				browser.Button(b => b.Value.Equals("Применить")).Click();
				Assert.That(browser.ContainsText("Сохранено"), Is.True);
				Assert.That(GetHomeRegionSelect(browser).SelectedOption.Text, Is.EqualTo(changeTo));

				//перезагружаем, потому что иначе увидим data bind
				browser.GoTo(browser.Url);
				browser.Refresh();
				Assert.That(GetHomeRegionSelect(browser).SelectedOption.Text, Is.EqualTo(changeTo));
			}
		}

		[Test]
		public void Try_to_delete_include_regulation()
		{
			ForTest.InitialzeAR();
			
			using (new SessionScope())
			{
				var client = Client.Find(2575u);
				client.Parents.Each(p => p.Delete());	
			}
			
			using (var browser = new IE(BuildTestUrl("manageret.aspx?cc=2575")))
			{
				var body = browser.Table(Find.ById("ctl00_MainContentPlaceHolder_IncludeGrid")).TableBodies[0];
				body.Button(Find.ByValue("Подчинить клиента.")).Click();
				body = browser.Table(Find.ById("ctl00_MainContentPlaceHolder_IncludeGrid")).TableBodies[0];
				body.TextFields.First().TypeText("ТестерК");
				body.Button(Find.ByValue("Найти")).Click();

				browser.Button(b => b.Value.Equals("Применить")).Click();

				browser.GoTo(browser.Url);
				browser.Refresh();
				body = browser.Table(Find.ById("ctl00_MainContentPlaceHolder_IncludeGrid")).TableBodies[0];
				body.Button(Find.ByValue("Удалить")).Click();

				browser.Button(b => b.Value.Equals("Применить")).Click();
				Assert.That(browser.Text, Text.Contains("Конфигурация клиента "));
			}
		}

		[Test]
		public void Try_to_view_orders()
		{
			using (var browser = new IE(BuildTestUrl("Client/2575")))
			{
				browser.Link(l => l.Text == "История заказов").Click();
				using (var openedWindow = IE.AttachToIE(Find.ByTitle("Статистика заказов")))
				{
					Assert.That(openedWindow.ContainsText("Укажите период"));
				}
			}
		}

		private SelectList GetHomeRegionSelect(IE browser)
		{
			return (SelectList)browser.Label(l => l.Text.Contains("Домашний регион")).NextSibling;
		}
	}
}
