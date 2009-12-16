using System;
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
			var client = DataMother.CreateTestClientWithUser();

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
	}
}
