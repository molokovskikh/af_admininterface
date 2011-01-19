using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using Integration.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using System.IO;
using AdminInterface;
using Common.Web.Ui.Models;
using System.Threading;
using DescriptionAttribute = NUnit.Framework.DescriptionAttribute;

namespace Functional
{
	public class UserFixture : WatinFixture
	{
		[Test]
		public void Edit_user()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users[0];
			using (var browser = Open("client/" + client.Id))
			{
				browser.Link(Find.ByText(user.Login)).Click();
				Assert.That(browser.Text, Is.StringContaining(user.Login));
			}
		}

		[Test]
		public void Set_user_parent()
		{
			User user;
			User mainUser;
			Client client;
			using (var transaction = new TransactionScope(OnDispose.Rollback))
			{
				client = DataMother.CreateTestClientWithUser();
				user = client.Users.First();
				mainUser = new User(client) {
					Name = "test"
				};
				mainUser.Setup(client);
				transaction.VoteCommit();
			}
			using(var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText(user.Login)).Click();
				Assert.That(browser.Text, Is.StringContaining("Пользователь " + user.Login));
				browser.Link(Find.ByText("Настройка")).Click();
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
				using (var stat = IE.AttachTo<IE>(Find.ByTitle(String.Format("Статистика изменения пароля для пользователя {0}", user.Login))))
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

				using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name))))
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

				using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name))))
				{
					Assert.That(openedWindow.Text,
						Is.StringContaining(String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name)));

					openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
					openedWindow.TextField(Find.ByName("emailsForSend")).Clear();
					openedWindow.TextField(Find.ByName("emailsForSend")).TypeText("KvasovTest@analit.net");
					openedWindow.Button(Find.ByValue("Изменить")).Click();
					Assert.That(openedWindow.Text, Is.StringContaining("Пароль успешно изменен"));
				}
			}
		}

		[Test, Description("При изменении пароля, если логин не совпадает с UserId и установлена соотв. опция, то изменить логин на UserId")]
		public void Change_login_when_change_password()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();
			using (new SessionScope())
			{
				user.Login = "testLogin" + user.Id;
				user.SaveAndFlush();
			}
			using (var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText(user.Login)).Click();
				browser.Link(Find.ByText("Изменить пароль")).Click();

				using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name))))
				{
					openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
					openedWindow.TextField(Find.ByName("emailsForSend")).TypeText("kvasovtest@analit.net");
					Assert.That(openedWindow.RadioButton(Find.ById("changeLogin")).Checked, Is.True);
					openedWindow.Button(Find.ByValue("Изменить")).Click();
					Assert.That(openedWindow.Text, Is.StringContaining("Пароль успешно изменен"));
				}
			}
			using (new SessionScope())
			{
				user = User.Find(user.Id);
				Assert.That(user.Login, Is.EqualTo(user.Id.ToString()));
			}
		}

		[Test, Description("При изменении пароля, если логин совпадает с UserId то изменять логин не нужно")]
		public void Not_change_login_when_change_password()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();
			Assert.IsTrue(user.Login == user.Id.ToString());
			using (var browser = Open("client/{0}", client.Id))
			{
				browser.Link(Find.ByText(user.Login)).Click();
				browser.Link(Find.ByText("Изменить пароль")).Click();

				using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name))))
				{
					openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
					openedWindow.TextField(Find.ByName("emailsForSend")).Clear();
					Assert.That(openedWindow.RadioButton(Find.ById("changeLogin")).Exists, Is.False);
					openedWindow.Button(Find.ByValue("Изменить")).Click();
					Assert.That(openedWindow.Text, Is.StringContaining("Пароль успешно изменен"));
				}
			}
			using (new SessionScope())
			{
				user.Refresh();
				Assert.That(user.Login, Is.EqualTo(user.Id.ToString()));
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

				var intersecionCount = Convert.ToUInt32(ArHelper.WithSession(s =>
					s.CreateSQLQuery("select count(*) from future.intersection where ClientId = :ClientId")
					.SetParameter("ClientId", client.Id)
					.UniqueResult()));
				Assert.That(result.Count, Is.GreaterThan(0), "не создали записей в UserPrices, у пользователя ни один прайс не включен");
				Assert.That(result.Count, Is.EqualTo(intersecionCount), "не совпадает кол-во записей в intersection и в UserPrices для данного клиента");
			}
		}

		[Test]
		public void Reset_user_uin()
		{
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();
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
				Assert.That(browser.Text, Is.StringContaining(String.Format("$$$ Пользователь: {0}", user.Login)));
			}
			info.Refresh();
			Assert.That(info.AFCopyId, Is.Empty);
		}

		[Test]
		public void Delete_user_prepared_data()
		{
			var formatString = CustomSettings.UserPreparedDataFormatString;
			var client = DataMother.CreateTestClientWithUser();
			var user = client.Users.First();
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
				browser.Link(Find.ByText("Настройка")).Click();

				for (int i = 0; i < 25; i++)
					browser.CheckBox(Find.ByName(String.Format("user.AssignedPermissions[{0}].Id", i))).Checked = (i % 2 == 0);

				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));

				browser.Back(); browser.Back(); browser.Back();

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
				Assert.AreEqual(String.Format("История обновлений пользователя {0}", client.Users[0].Id), browser.Title);
				browser.GoTo(baseUrl);

				browser.GoTo(browser.Link(Find.ByText("История документов")).Url);
				Assert.AreEqual("Статистика получения\\отправки документов пользователя " + client.Users[0].Login, browser.Title);
				browser.GoTo(baseUrl);

				browser.GoTo(browser.Link(Find.ByText("История заказов")).Url);
				Assert.AreEqual("История заказов", browser.Title);
				browser.GoTo(baseUrl);
			}

			using (var browser = Open("client/{0}", client.Id))
			{
				string baseUrl = browser.Url;

				browser.GoTo(browser.Link(Find.ByText("История обновлений")).Url);
				Assert.AreEqual(String.Format("История обновлений клиента {0}", client.Name), browser.Title);
				browser.GoTo(baseUrl);

				browser.GoTo(browser.Link(Find.ByText("История документов")).Url);
				Assert.AreEqual("Статистика получения\\отправки документов клиента " + client.Name, browser.Title);
				browser.GoTo(baseUrl);

				browser.GoTo(browser.Link(Find.ByText("История заказов")).Url);
				Assert.AreEqual("История заказов", browser.Title);
				browser.GoTo(baseUrl);
			}
		}

		[Test]
		public void AddContactInformation()
		{
			var applyButtonText = "Сохранить";
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				ContactInformationFixture.AddContact(browser, ContactType.Email, applyButtonText, client.Id);
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				ContactInformationFixture.AddContact(browser, ContactType.Phone, applyButtonText, client.Id);
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			}

			ContactGroup group;
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				group = client.Users[0].ContactGroup;
				Assert.That(client.ContactGroupOwner.Id, Is.EqualTo(group.ContactGroupOwner.Id),
							"Не совпадают Id владельца группы у клиента и у новой группы");
			}
			// Проверка, что контактные записи создались в БД
			ContactInformationFixture.CheckContactGroupInDb(group);
			var countContacts = ContactInformationFixture.GetCountContactsInDb(group);
			Assert.That(countContacts, Is.EqualTo(2));
		}

		[Test]
		public void Add_person_information()
		{
			var applyButtonText = "Сохранить";
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				ContactInformationFixture.AddPerson(browser, "Test person", applyButtonText, client.Id);
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				ContactInformationFixture.AddPerson(browser, "Test person2", applyButtonText, client.Id);
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			}
			ContactGroup group;
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				group = client.Users[0].ContactGroup;
				Assert.That(client.ContactGroupOwner.Id, Is.EqualTo(group.ContactGroupOwner.Id),
							"Не совпадают Id владельца группы у клиента и у новой группы");
			}
			// Проверка, что контактные записи создались в БД
			ContactInformationFixture.CheckContactGroupInDb(group);
			var persons = ContactInformationFixture.GetPersons(group);
			Assert.That(persons.Count, Is.EqualTo(2));
			Assert.That(persons[0], Is.EqualTo("Test person"));
			Assert.That(persons[1], Is.EqualTo("Test person2"));
		}

		[Test]
		public void DeleteContactInformation()
		{
			var applyButtonText = "Сохранить";
			var client = DataMother.CreateTestClientWithUser();
			// Удаление контактной записи
			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				ContactInformationFixture.AddContact(browser, ContactType.Email, applyButtonText, client.Id);
				ContactInformationFixture.AddContact(browser, ContactType.Phone, applyButtonText, client.Id);
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var group = client.Users[0].ContactGroup;
					browser.Button(Find.ByName(String.Format("contacts[{0}].Delete", group.Contacts[0].Id))).Click();
					browser.Button(Find.ByValue("Сохранить")).Click();
				}
			}
			// Проверка, что контактная запись удалена
			var countContacts = ContactInformationFixture.GetCountContactsInDb(client.Users[0].ContactGroup);
			Assert.That(countContacts, Is.EqualTo(1));
		}

		[Test]
		public void Delete_person_information()
		{
			var applyButtonText = "Сохранить";
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				ContactInformationFixture.AddPerson(browser, "Test person", applyButtonText, client.Id);
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				ContactInformationFixture.AddPerson(browser, "Test person2", applyButtonText, client.Id);
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var group = client.Users[0].ContactGroup;
					browser.Button(Find.ByName(String.Format("persons[{0}].Delete", group.Persons[0].Id))).Click();
					browser.Button(Find.ByValue("Сохранить")).Click();
				}
			}
			// Проверка, что контактная запись удалена
			var persons = ContactInformationFixture.GetPersons(client.Users[0].ContactGroup);
			Assert.That(persons.Count, Is.EqualTo(1));
			Assert.That(persons[0], Is.EqualTo("Test person2"));
		}

		[Test]
		public void Delete_person_information_by_fill_empty_string()
		{
			var applyButtonText = "Сохранить";
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				ContactInformationFixture.AddPerson(browser, "Test person", applyButtonText, client.Id);
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				ContactInformationFixture.AddPerson(browser, "Test person2", applyButtonText, client.Id);
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var group = client.Users[0].ContactGroup;
					browser.TextField(Find.ByName(String.Format("persons[{0}].Name", group.Persons[0].Id))).TypeText("");
					browser.Button(Find.ByValue("Сохранить")).Click();
				}
			}
			// Проверка, что контактная запись удалена
			var persons = ContactInformationFixture.GetPersons(client.Users[0].ContactGroup);
			Assert.That(persons.Count, Is.EqualTo(1));
			Assert.That(persons[0], Is.EqualTo("Test person2"));
		}

		[Test]
		public void Change_person_information()
		{
			var applyButtonText = "Сохранить";
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				ContactInformationFixture.AddPerson(browser, "Test person", applyButtonText, client.Id);
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var group = client.Users[0].ContactGroup;
					browser.TextField(Find.ByName(String.Format("persons[{0}].Name", group.Persons[0].Id))).TypeText("Test person changed");
					browser.Button(Find.ByValue("Сохранить")).Click();
					Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				}
			}
			// Проверка, что контактная запись изменена
			var persons = ContactInformationFixture.GetPersons(client.Users[0].ContactGroup);
			Assert.That(persons.Count, Is.EqualTo(1));
			Assert.That(persons[0], Is.EqualTo("Test person changed"));
		}

		[Test]
		public void TestUserRegions()
		{
				var client = DataMother.CreateTestClientWithUser();

				var user = client.Users[0];
				var setting = DrugstoreSettings.Find(client.Id);

				client.MaskRegion = 7;
				client.Save();
				setting.OrderRegionMask = 7;
				setting.Save();
				user.WorkRegionMask = 2;
				user.OrderRegionMask = 1;
				user.Save();

				using (var browser = Open("users/{0}/edit", user.Id))
				{
					Assert.IsTrue(browser.CheckBox("WorkRegions[0]").Checked);
					Assert.IsFalse(browser.CheckBox("WorkRegions[1]").Checked);
					Assert.IsFalse(browser.CheckBox("WorkRegions[2]").Checked);
					Assert.IsFalse(browser.CheckBox("WorkRegions[3]").Exists);

					Assert.IsFalse(browser.CheckBox("OrderRegions[0]").Checked);
					Assert.IsTrue(browser.CheckBox("OrderRegions[1]").Checked);
					Assert.IsFalse(browser.CheckBox("OrderRegions[2]").Checked);
					Assert.IsFalse(browser.CheckBox("OrderRegions[3]").Exists);

					browser.CheckBox("WorkRegions[1]").Checked = true;
					browser.CheckBox("OrderRegions[0]").Checked = true;
					browser.Button(Find.ByValue("Сохранить")).Click();

					using (new SessionScope())
					{
						client = Client.Find(client.Id);
						user = client.Users[0];
					}
					Assert.AreEqual(3, user.WorkRegionMask);
					Assert.AreEqual(3, user.OrderRegionMask);

					browser.CheckBox("WorkRegions[1]").Checked = false;
					browser.CheckBox("OrderRegions[0]").Checked = false;
					browser.Button(Find.ByValue("Сохранить")).Click();

					using (new SessionScope())
					{
						client = Client.Find(client.Id);
						user = client.Users[0];
					}
					Assert.AreEqual(2, user.WorkRegionMask);
					Assert.AreEqual(1, user.OrderRegionMask);

					client.MaskRegion = 31;
					client.Save();
					setting.OrderRegionMask = 3;
					setting.Save();

					browser.Refresh();
					Assert.IsTrue(browser.CheckBox("WorkRegions[3]").Exists);
					Assert.IsTrue(browser.CheckBox("WorkRegions[4]").Exists);
					Assert.IsFalse(browser.CheckBox("WorkRegions[5]").Exists);
					Assert.IsFalse(browser.CheckBox("OrderRegions[2]").Exists);
					Assert.IsTrue(browser.CheckBox("OrderRegions[1]").Exists);
				}
		}

		[Test]
		public void TestRegionsByCreatingNewUser()
		{
			var client = DataMother.CreateTestClient();
			var drugstore = DrugstoreSettings.Find(client.Id);
			// Id-шники регионов
			var browseRegions = new ulong[] { 1, 8, 16, 256 };
			var orderRegions = new ulong[] { 1, 8, 256 };

			client.MaskRegion = 0;
			foreach (var region in browseRegions)
				client.MaskRegion |= region;
			client.SaveAndFlush();
			foreach (var region in browseRegions)
				drugstore.WorkRegionMask |= region;
			foreach (var region in orderRegions)
				drugstore.OrderRegionMask |= region;
			drugstore.SaveAndFlush();

			using (var browser = Open(String.Format("client/{0}", client.Id)))
			{
				browser.Link(Find.ByText("Новый пользователь")).Click();
				Thread.Sleep(2000);
				Assert.That(browser.Text, Is.StringContaining("Новый пользователь"));
				// Указанные регионы для обзора и для заказа должны быть выделены
				foreach (var region in browseRegions)
					Assert.IsTrue(browser.CheckBox(Find.ById("browseRegion" + region)).Checked);
				foreach (var region in orderRegions)
					Assert.IsTrue(browser.CheckBox(Find.ById("orderRegion" + region)).Checked);
				// Если регион помечен только для обзора, то галка "Для заказа" должна быть снята
				var diff = browseRegions.Where(region => !orderRegions.Contains(region));
				foreach (var region in diff)
					Assert.IsFalse(browser.CheckBox(Find.ById("orderRegion" + region)).Checked);
				// Снимаем галку "В обзоре" (должна также сняться галка "Доступен для заказа")
				// и регистрируем нового пользователя
				browser.CheckBox(Find.ById("browseRegion" + browseRegions[0])).Checked = false;
				browser.TextField(Find.ByName("user.Name")).TypeText("User for test regions");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Uri.AbsolutePath.Contains("report.aspx"));
				var login = Helper.GetLoginFromRegistrationCard(browser);				
				browser.GoTo(BuildTestUrl(String.Format("client/{0}", client.Id)));
				browser.Refresh();
				browser.Link(Find.ByText(login.ToString())).Click();
				Assert.That(browser.Text, Is.StringContaining(String.Format("Пользователь {0}", login)));
				// Проверяем, чтобы были доступны нужные регионы. Берем с первого региона, т.к. галку с нулевого сняли
				for (var i = 1; i < browseRegions.Length; i++)
					Assert.IsTrue(browser.CheckBox(Find.ById(String.Format("WorkRegions[{0}]", i))).Checked);
				for (var i = 1; i < orderRegions.Length; i++)
					Assert.IsTrue(browser.CheckBox(Find.ById(String.Format("OrderRegions[{0}]", i))).Checked);
			}
		}

		[Test]
		public void User_must_be_enabled_after_registration()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("client/{0}", client.Id))
			{
				using (new SessionScope())
				{
					browser.Link(Find.ByText("Новый пользователь")).Click();
					browser.TextField(Find.ByName("user.Name")).TypeText("test user");
					browser.Button(Find.ByValue("Создать")).Click();
					browser.GoTo(BuildTestUrl(String.Format("client/{0}", client.Id)));
					client = Client.Find(client.Id);
					Assert.That(client.Users.Count, Is.EqualTo(1));
					Assert.IsTrue(client.Users[0].Enabled);
				}
			}			
		}

		[Test, Description("Регистрация пользователя. При добавлении email в контактную информацию, он должен добавляться в список адресов, на которые нужно отсылать регистрационную карту")]
		public void Add_email_for_registration_card_by_adding_contact_email()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("client/{0}", client.Id))
			{
				using (new SessionScope())
				{
					browser.Link(Find.ByText("Новый пользователь")).Click();
				}
			}
		}

		[Test, Description("Регистрация пользователя. Проверка валидатора списка email-ов для отправки регистрационной карты")]
		public void Validate_email_list_for_sending_registration_card()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open("client/{0}", client.Id))
			{
				using (new SessionScope())
				{
					browser.Link(Find.ByText("Новый пользователь")).Click();
					browser.TextField(Find.ByName("mails")).TypeText("asjkdf sdfj34kjl 4 ./4,524,l5; ");
					browser.Button(Find.ByValue("Создать")).Click();
					Assert.That(browser.Text, Is.StringContaining("Поле содержит некорректный адрес электронной почты"));
					browser.TextField(Find.ByName("mails")).TypeText("test1@test.test,test2@test.test,    test3@test.test.");
					browser.Button(Find.ByValue("Создать")).Click();
					Assert.That(browser.Text, Is.StringContaining("Поле содержит некорректный адрес электронной почты"));
					browser.TextField(Find.ByName("mails")).TypeText("test1@test.test,test2@test.test,    test3@test.test");
					browser.Button(Find.ByValue("Создать")).Click();
					Assert.That(browser.Text, Is.StringContaining("Пользователь создан"));
				}
			}
		}

		[Test, Description("Тест для регистрации адреса при регистрации пользователя")]
		public void Create_user_with_address()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open(String.Format("client/{0}", client.Id)))
			{
				Assert.That(client.Addresses, Is.Null);
				Assert.That(client.Users, Is.Null);
				browser.Link(Find.ByText("Новый пользователь")).Click();
				browser.CheckBox(Find.ByName("sendClientCard")).Checked = true;
				browser.TextField(Find.ByName("mails")).TypeText("KvasovTest@analit.net");
				browser.TextField(Find.ByName("address.Value")).TypeText("TestAddress");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Пользователь создан"));
			}
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				Assert.That(client.Users.Count, Is.EqualTo(1));
				Assert.That(client.Addresses.Count, Is.EqualTo(1));
				Assert.That(client.Users[0].AvaliableAddresses.Count, Is.EqualTo(1));
				var address = client.Addresses[0];
				Assert.That(address.AvaliableForUsers.Count, Is.EqualTo(1));
				Assert.IsTrue(address.AvaliableFor(client.Users[0]));

				var addressIntersection = ArHelper.WithSession(s => s.CreateSQLQuery(
					"select * from future.AddressIntersection where AddressId = :id")
						.SetParameter("id", address.Id)
						.List());
				Assert.That(addressIntersection.Count, Is.GreaterThan(0), "Не найдено записей в AddressIntersection");
			}
		}

		[Test]
		public void Create_user_with_contact_person_info()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open(String.Format("client/{0}", client.Id)))
			{
				browser.Link(Find.ByText("Новый пользователь")).Click();
				browser.Link("addPersonLink" + client.Id).Click();
				browser.TextField(Find.ByName(String.Format("persons[-1].Name"))).TypeText("Alice");
				browser.TextField(Find.ByName("mails")).TypeText("KvasovTest@analit.net");
				browser.TextField(Find.ByName("address.Value")).TypeText("TestAddress");
				browser.Button(Find.ByValue("Создать")).Click();
				Assert.That(browser.Text, Is.StringContaining("Пользователь создан"));
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					Assert.That(client.ContactGroupOwner.ContactGroups.Count, Is.EqualTo(1));
					var persons = client.ContactGroupOwner.ContactGroups[0].Persons;
					Assert.That(persons.Count, Is.EqualTo(1));
					Assert.That(persons[0].Name, Is.EqualTo("Alice"));
				}
			}
		}

		[Test]
		public void Update_person_contact_info()
		{
			var client = DataMother.CreateTestClient();
			using (var browser = Open(String.Format("client/{0}", client.Id)))
			{
				browser.Link(Find.ByText("Новый пользователь")).Click();
				browser.Link("addPersonLink" + client.Id).Click();
				browser.TextField(Find.ByName("persons[-1].Name")).TypeText("Alice");
				browser.TextField(Find.ByName("mails")).TypeText("KvasovTest@analit.net");
				browser.TextField(Find.ByName("address.Value")).TypeText("TestAddress");
				browser.Button(Find.ByValue("Создать")).Click();
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var user = client.Users[0];
					browser.Link(Find.ByText(user.Id.ToString())).Click();
					var group = client.Users[0].ContactGroup;
					Assert.That(browser.TextField(Find.ByName(String.Format("persons[{0}].Name", group.Persons[0].Id))).Text, Is.EqualTo("Alice"));
					browser.TextField(Find.ByName(String.Format("persons[{0}].Name", group.Persons[0].Id))).TypeText("Alice modified");
					browser.Button(Find.ByValue("Сохранить")).Click();
					Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				}
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var persons = client.ContactGroupOwner.ContactGroups[0].Persons;
					Assert.That(persons.Count, Is.EqualTo(1));
					Assert.That(persons[0].Name, Is.EqualTo("Alice modified"));
				}
			}
		}

		[Test]
		public void Test_enable_update_setting()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = Open("Client/{0}", client.Id))
			{
				Assert.IsFalse(client.Users[0].EnableUpdate);
				browser.Link(Find.ByText(client.Users[0].Id.ToString())).Click();
				browser.Link(Find.ByText("Настройка")).Click();
				Assert.IsFalse(browser.CheckBox(Find.ByName("user.EnableUpdate")).Checked);
				browser.CheckBox(Find.ByName("user.EnableUpdate")).Checked = true;
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					Assert.IsTrue(client.Users[0].EnableUpdate);
				}

				browser.GoTo(BuildTestUrl(String.Format("Client/{0}", client.Id)));
				browser.Link(Find.ByText(client.Users[0].Id.ToString())).Click();
				browser.Link(Find.ByText("Настройка")).Click();
				Assert.IsTrue(browser.CheckBox(Find.ByName("user.EnableUpdate")).Checked);
				browser.CheckBox(Find.ByName("user.EnableUpdate")).Checked = false;
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					Assert.IsFalse(client.Users[0].EnableUpdate);
				}
			}
		}

		[Test]
		public void Add_comment_for_contact()
		{
			var applyButtonText = "Сохранить";
			var client = DataMother.CreateTestClientWithUser();

			using (var browser = Open("users/{0}/edit", client.Users[0].Id))
			{
				ContactInformationFixture.AddContact(browser, ContactType.Email, applyButtonText, client.Id);
				ContactInformationFixture.AddContact(browser, ContactType.Phone, applyButtonText, client.Id);
				using (new SessionScope())
				{
					client = Client.Find(client.Id);
					var group = client.Users[0].ContactGroup;
					browser.TextField(Find.ByName(String.Format("contacts[{0}].Comment", group.Contacts[0].Id))).TypeText("some comment");
					browser.Button(Find.ByValue("Сохранить")).Click();
				}
			}
			// Проверка, что комментарий записан
			var contact = Contact.Find(client.Users[0].ContactGroup.Contacts[0].Id);
			Assert.That(contact.Comment, Is.EqualTo("some comment"));
		}

		[Test, Description("Перемещение только пользователя (без адреса доставки) к другому клиенту")]
		public void Move_only_user_to_another_client()
		{
			Client oldClient;
			Client newClient;
			uint userIdForMove = 0;
			using (new SessionScope())
			{
				oldClient = DataMother.CreateTestClientWithAddressAndUser();
				newClient = DataMother.CreateTestClientWithAddressAndUser();

				oldClient.Name += oldClient.Id.ToString();
				oldClient.UpdateAndFlush();
				userIdForMove = oldClient.Users[0].Id;
				newClient.Name += newClient.Id.ToString();
				newClient.UpdateAndFlush();
			}

			using (var browser = Open("users/{0}/edit", oldClient.Users[0].Id))
			{
				browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
				browser.Button(Find.ById("SearchClientButton")).Click();
				Thread.Sleep(2000);
				Assert.IsTrue(browser.SelectList(Find.ById("clientsList")).Exists);
				Assert.That(browser.SelectList(Find.ById("clientsList")).Options.Count, Is.GreaterThan(0));

				Assert.IsTrue(browser.Button(Find.ByValue("Отмена")).Exists);
				Assert.IsTrue(browser.Button(Find.ByValue("Переместить")).Exists);

				browser.Button(Find.ByValue("Переместить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Пользователь успешно перемещен"));
				Assert.That(browser.Text, Is.StringContaining(newClient.Name));
				Assert.That(browser.Text, Is.Not.StringContaining(oldClient.Name));
			}

			using (new SessionScope())
			{
				oldClient.Refresh();
				newClient.Refresh();
				var user = User.Find(userIdForMove);
				Assert.That(user.Client.Id, Is.EqualTo(newClient.Id));
				Assert.That(newClient.Users.Count, Is.EqualTo(2));
				Assert.That(oldClient.Users.Count, Is.EqualTo(0));
			}
		}

		[
			Test,
			Description(@"
После перемещения пользователя должны быть созданы записи в UserPrices 
для тех регионов, которых не было у старого клиента, но они есть у нового")
		]
		public void After_user_moving_must_be_entries_in_UserPrices()
		{
			Client oldClient;
			Client newClient;
			Address address;
			User user;

			using (new SessionScope())
			{
				var maskRegion = 1UL | 16UL;
				oldClient = DataMother.CreateTestClientWithAddressAndUser();
				newClient = DataMother.CreateTestClientWithAddressAndUser(maskRegion);

				user = oldClient.Users[0];
				address = oldClient.Addresses[0];
			}
			var oldCountUserPrices = GetCountUserPrices(user.Id);

			using (var browser = Open("users/{0}/edit", user.Id.ToString()))
			{
				browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
				browser.Button(Find.ById("SearchClientButton")).Click();
				Thread.Sleep(2000);
				browser.Button(Find.ByValue("Переместить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Пользователь успешно перемещен"));
			}

			var newCountUserPricesEntries = GetCountUserPrices(user.Id);
			Assert.That(oldCountUserPrices, Is.LessThan(newCountUserPricesEntries));

			Assert.That(GetCountUserPricesForRegion(user.Id, 16UL), Is.GreaterThan(0));
		}

		private uint GetCountUserPrices(uint userId)
		{
			return Convert.ToUInt32(ArHelper.WithSession(session => session.CreateSQLQuery(@"
SELECT COUNT(*)
FROM
	Future.UserPrices
WHERE UserId = :UserId
")
				.SetParameter("UserId", userId)
				.UniqueResult()));
		}

		private uint GetCountUserPricesForRegion(uint userId, ulong regionId)
		{
			return Convert.ToUInt32(ArHelper.WithSession(session => session.CreateSQLQuery(@"
SELECT COUNT(*)
FROM
	Future.UserPrices
WHERE UserId = :UserId AND RegionId = :RegionId
")
				.SetParameter("UserId", userId)
				.SetParameter("RegionId", regionId)
				.UniqueResult()));
		}

		[Test, Description("Перемещение пользователя с адресом доставки к другому клиенту")]
		public void Move_user_with_address_to_another_client()
		{
			Client oldClient;
			Client newClient;
			Address address;
			User user;

			using (new SessionScope())
			{
				oldClient = DataMother.CreateTestClientWithAddressAndUser();
				user = oldClient.Users[0];
				address = oldClient.Addresses[0];
				newClient = DataMother.CreateTestClientWithAddressAndUser();
			}
			using (var browser = Open("users/{0}/edit", user.Id.ToString()))
			{
				// Даем доступ пользователю к адресу доставки
				browser.CheckBox(Find.ByName("user.AvaliableAddresses[0].Id")).Checked = true;
				browser.Button(Find.ByValue("Сохранить")).Click();
				browser.Refresh();

				// Ищем клиента, к которому нужно передвинуть пользователя и двигаем
				browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
				browser.Button(Find.ById("SearchClientButton")).Click();
				Thread.Sleep(2000);
				Assert.That(browser.Text, Is.StringContaining(String.Format("Перемещать адрес доставки {0}", address.Value)));
				Assert.IsTrue(browser.CheckBox(Find.ByName("moveWithAddress")).Checked);
				browser.Button(Find.ByValue("Переместить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Пользователь успешно перемещен"));
			}

			using (new SessionScope())
			{
				oldClient.Refresh();
				newClient.Refresh();
				user.Refresh();
				Assert.That(user.Client.Id, Is.EqualTo(newClient.Id));

				Assert.That(newClient.Users.Count, Is.EqualTo(2));
				Assert.That(oldClient.Users.Count, Is.EqualTo(0));

				Assert.That(newClient.Addresses.Count, Is.EqualTo(2));
				Assert.That(oldClient.Addresses.Count, Is.EqualTo(0));
			}
		}
	}
}