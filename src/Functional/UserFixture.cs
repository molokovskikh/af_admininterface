using System;
using System.Collections;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using AdminInterface.Helpers;
using System.IO;
using AdminInterface;

namespace Functional
{
	public class UserFixture : WatinFixture
	{
		[Test]
		public void Edit_user()
		{
			using (var browser = Open("client/2575"))
			{
				browser.Link(Find.ByText("kvasov")).Click();
				Assert.That(browser.Text, Is.StringContaining("kvasov"));
			}
		}

		[Test]
		public void Set_user_parent()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();
			var mainUser = new User {
				Client = client,
				Name = "test"
			};
			mainUser.Setup(true);
			using(var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText(user.Login)).Click();
				Assert.That(browser.Text, Is.StringContaining("Пользователь " + user.Login));
				browser.SelectList(Find.ByName("user.InheritPricesFrom.Id")).Select(mainUser.Login);
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранен"));
			}
			using (new SessionScope())
			{
				user = User.Find(user.Id);
				Assert.That(user.InheritPricesFrom.Id, Is.EqualTo(mainUser.Id));
			}
		}

		[Test]
		public void View_password_change_statistics()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();
			using (var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText(user.Login)).Click();
				Assert.That(browser.Text, Is.StringContaining(String.Format("Пользователь {0}", user.Login)));

				browser.Link(Find.ByText("Статистика изменения пароля")).Click();
				using (var stat = IE.AttachToIE(Find.ByTitle(String.Format("Статистика изменения пароля для пользователя {0}", user.Login))))
				{
					Assert.That(stat.Text, Is.StringContaining(String.Format("Статистика изменения пароля для пользователя {0}", user.Login)));
				}
			}
		}

		[Test]
		public void Change_password_and_view_card()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();
			using(var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText(user.Login)).Click();
				Assert.That(browser.Text, Is.StringContaining(String.Format("Пользователь {0}", user.Login)));
				browser.Link(Find.ByText("Изменить пароль")).Click();

				using (var openedWindow = IE.AttachToIE(Find.ByTitle(String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name))))
				{
					Assert.That(openedWindow.Text,
						Is.StringContaining(String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name)));

					openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
					openedWindow.CheckBox(Find.ByName("isSendClientCard")).Click();
					openedWindow.Button(Find.ByValue("Изменить")).Click();
					Assert.That(openedWindow.Text, Is.StringContaining("Регистрационная карта"));
					Assert.That(openedWindow.Text, Is.StringContaining("Изменение пароля по инициативе клиента"));
				}
			}
		}

		[Test]
		public void Change_password_and_send_card()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();
			using (var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText(user.Login)).Click();
				Assert.That(browser.Text, Is.StringContaining(String.Format("Пользователь {0}", user.Login)));
				browser.Link(Find.ByText("Изменить пароль")).Click();

				using (var openedWindow = IE.AttachToIE(Find.ByTitle(String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name))))
				{
					Assert.That(openedWindow.Text,
						Is.StringContaining(String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name)));

					openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
					openedWindow.TextField(Find.ByName("additionEmailsToNotify")).Clear();
					openedWindow.TextField(Find.ByName("additionEmailsToNotify")).TypeText("KvasovTest@analit.net");
					openedWindow.Button(Find.ByValue("Изменить")).Click();
					Assert.That(openedWindow.Text, Is.StringContaining("Пароль успешно изменен"));
				}
			}
		}

		[Test]
		public void Create_user()
		{
			var client = DataMother.CreateTestClient();

			using (var browser = Open("client/{0}", client.Id))
			{
				Assert.That(browser.Text, Is.StringContaining("Клиент test"));
				browser.Link(Find.ByText("Новый пользователь")).Click();

				Assert.That(browser.Text, Is.StringContaining("Новый пользователь"));
				browser.TextField(Find.ByName("user.Name")).TypeText("test");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Регистрационная карта"));
			}

			using(new SessionScope())
			{
				client = Client.Find(client.Id);
				Assert.That(client.Users.Count, Is.EqualTo(1));
				var user = client.Users.Single();
				Assert.That(user.Name, Is.EqualTo("test"));
				var updateInfo = UserUpdateInfo.Find(user.Id);
				Assert.That(updateInfo, Is.Not.Null);

				var result = ArHelper.WithSession(s =>
					s.CreateSQLQuery("select * from future.UserPrices where UserId = :userId")
					.SetParameter("userId", user.Id)
					.List());
				Assert.That(result.Count, Is.GreaterThan(0), "не создали записей в UserPrices, у пользователя ни один прайс не включен");
			}
		}

		[Test]
		public void Reset_user_uin()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.GetUsers().First();
			user.Name = String.Empty;
			user.Update();
			var info = UserUpdateInfo.Find(user.Id);
			info.AFCopyId = "123";
			info.Update();
			using (var browser = Open("client/{0}", client.Id))
			{
				Assert.That(user.HaveUin(), Is.True);
				browser.Link(Find.ByText(user.Login)).Click();
				browser.Button(Find.ByValue("Сбросить УИН")).Click();
				Assert.That(browser.Text, Is.StringContaining("Это поле необходимо заполнить."));
				browser.TextField(Find.ByName("reason")).TypeText("test reason");
				browser.Button(Find.ByValue("Сбросить УИН")).Click();
				Assert.That(browser.Text, Is.StringContaining("УИН сброшен"));
				var count = Convert.ToInt32(ArHelper.WithSession(s =>
					s.CreateSQLQuery("SELECT count(*) FROM `logs`.clientsinfo where ClientCode = :id")
						.SetParameter("id", client.Id)
						.UniqueResult()));
				Assert.IsTrue(count == 1);
			}
		}

		[Test]
		public void Delete_user_prepared_data()
		{
			var formatString = CustomSettings.UserPreparedDataFormatString;
			var client = DataMother.CreateTestClientWithUser();
			var user = client.GetUsers().First();
			user.Name = String.Empty;
			user.Update();
			var preparedDataPath = String.Format(formatString, user.Id);
			using (var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText(user.Login)).Click();
				Assert.That(browser.Button(Find.ByValue("Удалить подготовленные данные")).Enabled, Is.False);
				var directory = Path.GetDirectoryName(preparedDataPath);
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);
				var file = File.Create(preparedDataPath);
				browser.Back();
				browser.Link(Find.ByText(user.Login)).Click();
				Assert.That(browser.Button(Find.ByValue("Удалить подготовленные данные")).Enabled, Is.True);
				browser.Button(Find.ByValue("Удалить подготовленные данные")).Click();
				Assert.That(browser.Text, Is.StringContaining("Ошибка удаления подготовленных данных, попробуйте позднее."));
				file.Close();
				browser.Button(Find.ByValue("Удалить подготовленные данные")).Click();
				Assert.That(browser.Text, Is.StringContaining("Подготовленные данные удалены"));
				try
				{
					File.Delete(file.Name);
				}
				catch
				{
				}
			}
		}

		[Test]
		public void EditAnalitFSettings()
		{
			var client = DataMother.CreateTestClientWithUser();
			using(var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText(client.Users[0].Login)).Click();

				for (int i = 0; i < 25; i++)
					browser.CheckBox(Find.ByName(String.Format("user.AssignedPermissions[{0}].Id", i))).Checked = (i % 2 == 0);

				browser.Button(Find.ByValue("Сохранить")).Click();

				browser.Back(); browser.Back();

				browser.Link(Find.ByText("Новый пользователь")).Click();
				browser.TextField(Find.ByName("user.Name")).TypeText("test2");

				for (int i = 0; i < 25; i++ )
					browser.CheckBox(Find.ByName(String.Format("user.AssignedPermissions[{0}].Id", i))).Checked = (i % 2 == 0);

				browser.Button(Find.ByValue("Создать")).Click();
			}

			using(new SessionScope())
			{
				client = Client.Find(client.Id);
				Assert.AreEqual(2, client.Users.Count);
				Assert.AreEqual(13, client.Users[0].AssignedPermissions.Count);
				Assert.AreEqual(13, client.Users[1].AssignedPermissions.Count);
			}
		}

		[Test]
		public void SendMessage()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				browser.TextField(Find.ByName("message")).TypeText("тестовое сообщение");
				browser.Button(Find.ByValue("Принять")).Click();
				Assert.IsTrue(browser.TableCell(Find.ByText("тестовое сообщение")).Exists);
				browser.Link(Find.ByText("Клиент " + client.Name)).Click();
				Assert.IsTrue(browser.TableCell(Find.ByText("тестовое сообщение")).Exists);
			}
		}

		[Test]
		public void HistoriesMenuExistance()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				string baseUrl = browser.Url;

				browser.GoTo(browser.Link(Find.ByText("История обновлений")).Url);
				Assert.AreEqual("История обновлений пользователя " + client.Users[0].Login, browser.Title);
				browser.GoTo(baseUrl);

				browser.GoTo(browser.Link(Find.ByText("История документов")).Url);
				Assert.AreEqual("Статистика получения\\отправки документов пользователя " + client.Users[0].Login, browser.Title);
				browser.GoTo(baseUrl);

				browser.GoTo(browser.Link(Find.ByText("История заказов")).Url);
				Assert.AreEqual("История заказов пользователя " + client.Users[0].Login, browser.Title);
				browser.GoTo(baseUrl);
			}

			using (var browser = Open("client/{0}", client.Id))
			{
				string baseUrl = browser.Url;

				browser.GoTo(browser.Link(Find.ByText("История обновлений")).Url);
				Assert.AreEqual("История обновлений клиента " + client.Name, browser.Title);
				browser.GoTo(baseUrl);

				browser.GoTo(browser.Link(Find.ByText("История документов")).Url);
				Assert.AreEqual("Статистика получения\\отправки документов клиента " + client.Name, browser.Title);
				browser.GoTo(baseUrl);

				browser.GoTo(browser.Link(Find.ByText("История заказов")).Url);
				Assert.AreEqual("История заказов клиента " + client.Name, browser.Title);
				browser.GoTo(baseUrl);
			}
		}

		[Test]
		public void EditContactInormation()
		{
			var applyButtonText = "Сохранить";
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				browser.Button(Find.ByValue("Добавить")).Click();
				var rowId = 0;
				browser.TextField(String.Format("contacts[{0}].ContactText", --rowId)).TypeText("test@test");
				browser.Button(Find.ByValue(applyButtonText)).Click();
				Assert.That(browser.Text, Is.StringContaining("Некорректный адрес электронной почты"));
				browser.TextField(String.Format("contacts[{0}].ContactText", rowId)).TypeText("test@test.ru");
				browser.Button(Find.ByValue(applyButtonText)).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				rowId = 0;
				browser.Button(Find.ByValue("Добавить")).Click();
				var comboBox = browser.SelectList(Find.ByName(String.Format("contactTypes[{0}]", --rowId)));
				comboBox = browser.SelectLists[2];
				comboBox.SelectByValue(comboBox.Options[1].Value);
				browser.TextField(Find.ById(String.Format("contacts[{0}].ContactText", rowId))).TypeText("556677");
				browser.Button(Find.ByValue(applyButtonText)).Click();
				Assert.That(browser.Text, Is.StringContaining("Некорректный телефонный номер"));
				browser.TextField(Find.ById(String.Format("contacts[{0}].ContactText", rowId))).TypeText("556-677000");
				browser.Button(Find.ByValue("Добавить")).Click();
				browser.TextField(String.Format("contacts[{0}].ContactText", --rowId)).TypeText("test@test.ru");
				browser.Button(Find.ByValue(applyButtonText)).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			}

			// Проверка, что контактные записи создались в БД
			IList contactIds;
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				Assert.NotNull(client.Users[0].ContactGroup);
				var group = client.Users[0].ContactGroup;
				Assert.That(client.ContactGroupOwner.Id, Is.EqualTo(group.ContactGroupOwner.Id),
					"Не совпадают Id владельца группы у клиента и у новой группы");
				var contactGroupCount = ArHelper.WithSession(s =>
					s.CreateSQLQuery("select count(*) from contacts.contact_groups where Id = :ContactGroupId")
						.SetParameter("ContactGroupId", group.Id)
						.UniqueResult());
				Assert.That(Convert.ToInt32(contactGroupCount), Is.EqualTo(1));
				contactIds = ArHelper.WithSession(s =>
					s.CreateSQLQuery("select Id from contacts.contacts where ContactOwnerId = :ownerId")
						.SetParameter("ownerId", group.Id)
						.List());
				Assert.That(contactIds.Count, Is.EqualTo(3));
			}

			// Удаление контактной записи
			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				browser.Button(Find.ByName(String.Format("contacts[{0}].Delete", contactIds[0]))).Click();
				browser.Button(Find.ByValue("Сохранить")).Click();
			}

			// Проверка, что контактная запись удалена
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				var group = client.Users[0].ContactGroup;
				contactIds = ArHelper.WithSession(s =>
					s.CreateSQLQuery("select * from contacts.contacts where ContactOwnerId = :ownerId")
						.SetParameter("ownerId", group.Id)
						.List());
				Assert.That(contactIds.Count, Is.EqualTo(2));
			}
		}
	}
}