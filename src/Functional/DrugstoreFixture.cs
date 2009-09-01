using System.Linq;
using System.Threading;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Common.Tools;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class DrugstoreFixture : WatinFixture
	{
		private uint clientId = 2575;

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
		public void Try_to_update_relationship()
		{
			using (new TransactionScope())
			{
				var client = Client.Find(clientId);
				client.Parents.Each(p => p.Delete());
				client.Parents.Clear();
				var parent = Client.Find(3616u);
				client.Parents.Add(new Relationship{ Child = client, Parent = parent, RelationshipType = RelationshipType.Base});

				client.Parents[0].Save();
			}

			using (var browser = new IE(BuildTestUrl("manageret.aspx?cc=2575")))
			{
				var body = browser.Table(Find.ById("ctl00_MainContentPlaceHolder_IncludeGrid")).TableBodies[0];
				body.SelectLists.ElementAt(1).Select("Сеть");
				Thread.Sleep(5*1000);

				browser.Button(b => b.Value.Equals("Применить")).Click();
				Assert.That(browser.Text, Text.Contains("Конфигурация клиента"));

				browser.GoTo(browser.Url);
				browser.Refresh();
				body = browser.Table(Find.ById("ctl00_MainContentPlaceHolder_IncludeGrid")).TableBodies[0];
				Assert.That(body.TableRows.Count(), Is.EqualTo(2));
				Assert.That(body.TableRows[1].SelectLists.First().SelectedItem, Is.EqualTo("3616. ТестерК2"));
				Assert.That(body.TableRows[1].SelectLists.ElementAt(1).SelectedItem, Is.EqualTo("Сеть"));
			}

			using (new SessionScope())
			{
				var client = Client.Find(clientId);
				Assert.That(client.Parents.Count, Is.EqualTo(1));
				Assert.That(client.Parents[0].Parent.Id, Is.EqualTo(3616u));
				Assert.That(client.Parents[0].RelationshipType, Is.EqualTo(RelationshipType.Network));
			}
		}

		[Test]
		public void Try_to_create_relationship()
		{
			using (new SessionScope())
			{
				var client = Client.Find(clientId);
				client.Parents.ToArray().Each(client.RemoveRelationship);
			}

			using (var browser = new IE(BuildTestUrl("manageret.aspx?cc=2575")))
			{
				var body = browser.Table(Find.ById("ctl00_MainContentPlaceHolder_IncludeGrid")).TableBodies[0];
				body.Button(Find.ByValue("Подчинить клиента.")).Click();
				body = browser.Table(Find.ById("ctl00_MainContentPlaceHolder_IncludeGrid")).TableBodies[0];
				body.TextFields.First().TypeText("ТестерК");
				body.Button(Find.ByValue("Найти")).Click();

				browser.Button(b => b.Value.Equals("Применить")).Click();
				Assert.That(browser.Text, Text.Contains("Конфигурация клиента"));

				browser.GoTo(browser.Url);
				browser.Refresh();
				body = browser.Table(Find.ById("ctl00_MainContentPlaceHolder_IncludeGrid")).TableBodies[0];
				Assert.That(body.TableRows.Count(), Is.EqualTo(2));
				Assert.That(body.TableRows[1].SelectLists.First().SelectedItem, Is.EqualTo("2575. ТестерК"));
				Assert.That(body.TableRows[1].SelectLists.ElementAt(1).SelectedItem, Is.EqualTo("Базовый"));
			}

			using (new SessionScope())
			{
				var client = Client.Find(clientId);
				Assert.That(client.Parents.Count, Is.EqualTo(1));
				Assert.That(client.Parents[0].RelationshipType, Is.EqualTo(RelationshipType.Base));
			}
		}

		[Test]
		public void Try_to_delete_include_regulation()
		{
			using (new SessionScope())
			{
				var client = Client.Find(2575u);
				client.Parents.ToArray().Each(client.RemoveRelationship);
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