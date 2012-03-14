using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
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

namespace Functional.Drugstore
{
	public class UserFixture : WatinFixture2
	{
		private Client client;
		private User user;
		private DrugstoreSettings settings;

		[SetUp]
		public void Setup()
		{
			client = DataMother.CreateTestClientWithUser();
			settings = client.Settings;
			user = client.Users.First();
			scope.Flush();

			Open(client);
			Assert.That(browser.Text, Is.StringContaining("Клиент"));
		}

		[Test]
		public void Edit_user()
		{
			browser.Link(Find.ByText(user.Login)).Click();
			Assert.That(browser.Text, Is.StringContaining(user.Login));
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
				mainUser.Setup();
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
			browser.Link(Find.ByText(user.Login)).Click();
			Assert.That(browser.Text, Is.StringContaining(String.Format("Пользователь {0}", user.Login)));

			browser.Link(Find.ByText("Статистика изменения пароля")).Click();
			using (var stat = IE.AttachTo<IE>(Find.ByTitle(String.Format("Статистика изменения пароля для пользователя {0}", user.Login))))
			{
				Assert.That(stat.Text, Is.StringContaining(String.Format("Статистика изменения пароля для пользователя {0}", user.Login)));
			}
		}

		[Test]
		public void Change_password_and_view_card()
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

		[Test(Description = "Тест для проверки состояния галок 'Получать накладные', 'Получать отказы', 'Игнорировать проверку минимальной суммы заказа у Поставщика' при регистрации нового пользователя")]
		public void Check_flags_by_adding_user()
		{
			browser.Link(Find.ByText("Новый пользователь")).Click();
			browser.Button(Find.ByValue("Создать")).Click();
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				Assert.That(client.Users.Count, Is.GreaterThan(0));
				browser.GoTo(BuildTestUrl(String.Format("client/{0}", client.Id)));
				browser.Refresh();
				var userLink = browser.Link(Find.ByText(client.Users[0].Login));
				Assert.IsTrue(userLink.Exists);
				userLink.Click();
				browser.Link(Find.ByText("Настройка")).Click();
				Assert.IsTrue(browser.CheckBox(Find.ByName("user.SendWaybills")).Checked);
				Assert.IsTrue(browser.CheckBox(Find.ByName("user.SendRejects")).Checked);
				Assert.IsFalse(browser.CheckBox(Find.ByName("user.IgnoreCheckMinOrder")).Checked);
			}
		}

		[Test]
		public void Change_password_and_send_card()
		{
			browser.Link(Find.ByText(user.Login)).Click();
			Assert.That(browser.Text, Is.StringContaining(String.Format("Пользователь {0}", user.Login)));
			browser.Link(Find.ByText("Изменить пароль")).Click();

			var title = String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name);
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(title)))
			{
				Assert.That(openedWindow.Text, Is.StringContaining(title));

				openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
				openedWindow.TextField(Find.ByName("emailsForSend")).Clear();
				openedWindow.TextField(Find.ByName("emailsForSend")).TypeText("KvasovTest@analit.net");
				openedWindow.Button(Find.ByValue("Изменить")).Click();
				Assert.That(openedWindow.Text, Is.StringContaining("Пароль успешно изменен"));
			}
		}

		[Test, NUnit.Framework.Description("При изменении пароля, если логин не совпадает с UserId и установлена соотв. опция, то изменить логин на UserId")]
		public void Change_login_when_change_password()
		{
			user.Login = "testLogin" + user.Id;
			user.Save();
			Refresh();

			browser.Link(Find.ByText(user.Login)).Click();
			browser.Link(Find.ByText("Изменить пароль")).Click();
			var title = String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name);
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(title)))
			{
				openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
				openedWindow.TextField(Find.ByName("emailsForSend")).TypeText("kvasovtest@analit.net");
				Assert.That(openedWindow.RadioButton(Find.ById("changeLogin")).Checked, Is.True);
				openedWindow.Button(Find.ByValue("Изменить")).Click();
				Assert.That(openedWindow.Text, Is.StringContaining("Пароль успешно изменен"));
			}

			session.Refresh(user);

			Assert.That(user.Login, Is.EqualTo(user.Id.ToString()));
		}

		[Test, NUnit.Framework.Description("При изменении пароля, если логин совпадает с UserId то изменять логин не нужно")]
		public void Not_change_login_when_change_password()
		{
			browser.Link(Find.ByText(user.Login)).Click();
			browser.Link(Find.ByText("Изменить пароль")).Click();

			var title = String.Format("Изменение пароля пользователя {0} [Клиент: {1}]", user.Login, client.Name);
			using (var openedWindow = IE.AttachTo<IE>(Find.ByTitle(title)))
			{
				openedWindow.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
				openedWindow.TextField(Find.ByName("emailsForSend")).TypeText("kvasovtest@analit.net");
				Assert.That(openedWindow.RadioButton(Find.ById("changeLogin")).Exists, Is.False);
				openedWindow.Button(Find.ByValue("Изменить")).Click();
				Assert.That(openedWindow.Text, Is.StringContaining("Пароль успешно изменен"));
			}

			session.Refresh(user);
			Assert.That(user.Login, Is.EqualTo(user.Id.ToString()));
		}

		[Test]
		public void Create_user()
		{
			Assert.That(browser.Text, Is.StringContaining("Клиент test"));
			browser.Link(Find.ByText("Новый пользователь")).Click();

			Assert.That(browser.Text, Is.StringContaining("Новый пользователь"));
			browser.TextField(Find.ByName("user.Name")).TypeText("test");
			browser.Button(Find.ByValue("Создать")).Click();
			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта"));

			client.Refresh();
			Assert.That(client.Users.Count, Is.EqualTo(2));
			var user = client.Users.OrderBy(u => u.Id).Last();
			Assert.That(user.Name, Is.EqualTo("test"));
			Assert.That(user.UserUpdateInfo, Is.Not.Null);

			var userPriceCount = user.GetUserPriceCount();
			var intersecionCount = client.GetIntersectionCount();

			Assert.That(userPriceCount, Is.GreaterThan(0), "не создали записей в UserPrices, у пользователя ни один прайс не включен");
			Assert.That(userPriceCount, Is.EqualTo(intersecionCount), "не совпадает кол-во записей в intersection и в UserPrices для данного клиента");
		}

		[Test]
		public void Reset_user_uin()
		{
			user.UserUpdateInfo.AFCopyId = "123";
			user.UserUpdateInfo.UpdateAndFlush();

			Assert.That(user.HaveUin(), Is.True);
			browser.Link(Find.ByText(user.Login)).Click();
			browser.Button(Find.ByValue("Сбросить УИН")).Click();
			Assert.That(browser.Text, Is.StringContaining("Это поле необходимо заполнить."));
			browser.TextField(Find.ByName("reason")).TypeText("test reason");
			browser.Button(Find.ByValue("Сбросить УИН")).Click();
			Assert.That(browser.Text, Is.StringContaining("УИН сброшен"));
			Assert.That(browser.Text, Is.StringContaining(String.Format("$$$ Пользователь: {0}", user.Login)));

			user.UserUpdateInfo.Refresh();
			Assert.That(user.UserUpdateInfo.AFCopyId, Is.Empty);
		}

		[Test]
		public void Delete_user_prepared_data()
		{
			var preparedDataPath = String.Format(@"C:\Windows\Temp\{0}_123456.zip", user.Id);

			browser.Link(Find.ByText(user.Login)).Click();
			Assert.That(browser.Button(Find.ByValue("Удалить подготовленные данные")).Enabled, Is.False);
			var directory = Path.GetDirectoryName(preparedDataPath);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			var file = File.Create(preparedDataPath);

			browser.Refresh();

			Assert.That(browser.Button(Find.ByValue("Удалить подготовленные данные")).Enabled, Is.True);
			browser.Button(Find.ByValue("Удалить подготовленные данные")).Click();
			Assert.That(browser.Text, Is.StringContaining("Ошибка удаления подготовленных данных, попробуйте позднее."));
			file.Close();
			browser.Button(Find.ByValue("Удалить подготовленные данные")).Click();
			Assert.That(browser.Text, Is.StringContaining("Подготовленные данные удалены"));
			try
			{
				File.Delete(file.Name);
			} catch {}
		}

		[Test]
		public void EditAnalitFSettings()
		{
			Click(user.Login);
			Click("Настройка");

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

			client.Refresh();
			session.Refresh(user);
			Assert.AreEqual(2, client.Users.Count);
			Assert.AreEqual(13, client.Users[0].AssignedPermissions.Count);
			Assert.AreEqual(13, client.Users[1].AssignedPermissions.Count);
		}

		[Test]
		public void SendMessage()
		{
			Open(user, "Edit");

			browser.TextField(Find.ByName("message")).TypeText("тестовое сообщение");
			Click("Принять");
			Assert.IsTrue(browser.TableCell(Find.ByText("тестовое сообщение")).Exists);
			Click(client.Name);
			Assert.IsTrue(browser.TableCell(Find.ByText("тестовое сообщение")).Exists);
		}

		[Test]
		public void HistoriesMenuExistance()
		{
			Open(user, "Edit");
			var baseUrl = browser.Url;

			browser.GoTo(browser.Link(Find.ByText("История обновлений")).Url);
			Assert.AreEqual(String.Format("История обновлений пользователя {0}", client.Users[0].Id), browser.Title);
			browser.GoTo(baseUrl);

			browser.GoTo(browser.Link(Find.ByText("История документов")).Url);
			Assert.AreEqual("История документов", browser.Title);
			browser.GoTo(baseUrl);

			browser.GoTo(browser.Link(Find.ByText("История заказов")).Url);
			Assert.AreEqual("История заказов", browser.Title);
			browser.GoTo(baseUrl);
		}

		[Test]
		public void AddContactInformation()
		{
			var applyButtonText = "Сохранить";
			Open(user, "Edit");
			ContactInformationFixture.AddContact(browser, ContactType.Email, applyButtonText, client.Id);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			ContactInformationFixture.AddContact(browser, ContactType.Phone, applyButtonText, client.Id);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

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
			Open(user, "Edit");

			ContactInformationFixture.AddPerson(browser, "Test person", applyButtonText, client.Id);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			ContactInformationFixture.AddPerson(browser, "Test person2", applyButtonText, client.Id);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
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
			// Удаление контактной записи
			Open(user, "Edit");

			ContactInformationFixture.AddContact(browser, ContactType.Email, applyButtonText, client.Id);
			ContactInformationFixture.AddContact(browser, ContactType.Phone, applyButtonText, client.Id);
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				var group = client.Users[0].ContactGroup;
				browser.Button(Find.ByName(String.Format("contacts[{0}].Delete", group.Contacts[0].Id))).Click();
				browser.Button(Find.ByValue("Сохранить")).Click();
			}
			// Проверка, что контактная запись удалена
			var countContacts = ContactInformationFixture.GetCountContactsInDb(client.Users[0].ContactGroup);
			Assert.That(countContacts, Is.EqualTo(1));
		}

		[Test]
		public void Delete_person_information()
		{
			var applyButtonText = "Сохранить";
			Open(user, "Edit");

			ContactInformationFixture.AddPerson(browser, "Test person", applyButtonText, client.Id);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			ContactInformationFixture.AddPerson(browser, "Test person2", applyButtonText, client.Id);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			session.Refresh(user);
			var group = user.ContactGroup;
			browser.Button(Find.ByName(String.Format("persons[{0}].Delete", group.Persons[0].Id))).Click();
			browser.Button(Find.ByValue("Сохранить")).Click();
			// Проверка, что контактная запись удалена
			var persons = ContactInformationFixture.GetPersons(client.Users[0].ContactGroup);
			Assert.That(persons.Count, Is.EqualTo(1));
			Assert.That(persons[0], Is.EqualTo("Test person2"));
		}

		[Test]
		public void Delete_person_information_by_fill_empty_string()
		{
			var applyButtonText = "Сохранить";
			Open(user, "Edit");

			ContactInformationFixture.AddPerson(browser, "Test person", applyButtonText, client.Id);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			ContactInformationFixture.AddPerson(browser, "Test person2", applyButtonText, client.Id);
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			session.Refresh(user);
			var group = user.ContactGroup;
			browser.TextField(Find.ByName(String.Format("persons[{0}].Name", group.Persons[0].Id))).TypeText("");
			browser.Button(Find.ByValue("Сохранить")).Click();
			// Проверка, что контактная запись удалена
			var persons = ContactInformationFixture.GetPersons(client.Users[0].ContactGroup);
			Assert.That(persons.Count, Is.EqualTo(1));
			Assert.That(persons[0], Is.EqualTo("Test person2"));
		}

		[Test]
		public void Change_person_information()
		{
			var applyButtonText = "Сохранить";
			Open(user, "Edit");
			ContactInformationFixture.AddPerson(browser, "Test person", applyButtonText, client.Id);

			session.Refresh(user);
			var group = user.ContactGroup;
			browser.TextField(Find.ByName(String.Format("persons[{0}].Name", group.Persons[0].Id))).TypeText("Test person changed");
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			// Проверка, что контактная запись изменена
			var persons = ContactInformationFixture.GetPersons(user.ContactGroup);
			Assert.That(persons.Count, Is.EqualTo(1));
			Assert.That(persons[0], Is.EqualTo("Test person changed"));
		}

		[Test]
		public void TestUserRegions()
		{
			client.MaskRegion = 7;
			client.Save();
			settings.OrderRegionMask = 7;
			settings.Save();
			user.WorkRegionMask = 2;
			user.OrderRegionMask = 1;
			user.Save();
			scope.Flush();

			Open(user, "Edit");
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

			session.Refresh(user);
			Assert.AreEqual(3, user.WorkRegionMask);
			Assert.AreEqual(3, user.OrderRegionMask);

			browser.CheckBox("WorkRegions[1]").Checked = false;
			browser.CheckBox("OrderRegions[0]").Checked = false;
			browser.Button(Find.ByValue("Сохранить")).Click();

			session.Refresh(user);
			Assert.AreEqual(2, user.WorkRegionMask);
			Assert.AreEqual(1, user.OrderRegionMask);

			client.MaskRegion = 31;
			client.Save();
			settings.OrderRegionMask = 3;
			settings.Save();

			Refresh();
			Assert.IsTrue(browser.CheckBox("WorkRegions[3]").Exists);
			Assert.IsTrue(browser.CheckBox("WorkRegions[4]").Exists);
			Assert.IsFalse(browser.CheckBox("WorkRegions[5]").Exists);
			Assert.IsFalse(browser.CheckBox("OrderRegions[2]").Exists);
			Assert.IsTrue(browser.CheckBox("OrderRegions[1]").Exists);
		}

		[Test]
		public void TestRegionsByCreatingNewUser()
		{
			// Id-шники регионов
			var browseRegions = new ulong[] { 1, 8, 16, 256 };
			var orderRegions = new ulong[] { 1, 8, 256 };

			client.MaskRegion = 0;
			foreach (var region in browseRegions)
				client.MaskRegion |= region;
			client.Save();
			foreach (var region in browseRegions)
				settings.WorkRegionMask |= region;
			foreach (var region in orderRegions)
				settings.OrderRegionMask |= region;
			settings.Save();
			scope.Flush();

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
			
			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта "));
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

		[Test]
		public void User_must_be_enabled_after_registration()
		{
			browser.Link(Find.ByText("Новый пользователь")).Click();
			browser.TextField(Find.ByName("user.Name")).TypeText("test user");
			browser.Button(Find.ByValue("Создать")).Click();
			browser.GoTo(BuildTestUrl(String.Format("client/{0}", client.Id)));
			client = Client.Find(client.Id);
			Assert.That(client.Users.Count, Is.EqualTo(1));
			Assert.IsTrue(client.Users[0].Enabled);
		}

		[Test, NUnit.Framework.Description("Регистрация пользователя. Проверка валидатора списка email-ов для отправки регистрационной карты")]
		public void Validate_email_list_for_sending_registration_card()
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

		[Test, NUnit.Framework.Description("Тест для регистрации адреса при регистрации пользователя")]
		public void Create_user_with_address()
		{
			RegisterUserWithAddress(client, browser);
			browser.Button(Find.ByValue("Создать")).Click();
			Assert.That(browser.Text, Is.StringContaining("Пользователь создан"));

			client.Refresh();
			Assert.That(client.Users.Count, Is.EqualTo(2));
			Assert.That(client.Addresses.Count, Is.EqualTo(1));
			var createdUser = client.Users.OrderBy(u => u.Id).Last();
			Assert.That(createdUser.AvaliableAddresses.Count, Is.EqualTo(1));
			var createdAddress = client.Addresses.OrderBy(a => a.Id).Last();
			Assert.That(createdAddress.AvaliableForUsers.Count, Is.EqualTo(1));
			Assert.IsTrue(createdAddress.AvaliableFor(createdUser));

			Assert.That(createdAddress.GetAddressIntersectionCount(), Is.GreaterThan(0), "Не найдено записей в AddressIntersection");
		}

		private void RegisterUserWithAddress(Client client, IElementContainer browser)
		{
			browser.Link(Find.ByText("Новый пользователь")).Click();
			browser.CheckBox(Find.ByName("sendClientCard")).Checked = true;
			browser.TextField(Find.ByName("mails")).TypeText("KvasovTest@analit.net");
			browser.TextField(Find.ByName("address.Value")).TypeText("TestAddress");
		}

		[Test]
		public void Register_user_with_addresses_not_default_legal_entity()
		{
			var payer = client.Payers.First();
			var legalEntity = new LegalEntity {
				Name = "Тестовая организация 2",
				Payer = payer
			};
			legalEntity.Save();
			payer.JuridicalOrganizations.Add(legalEntity);

			RegisterUserWithAddress(client, browser);
			browser.Css("#address_LegalEntity_Id").Select("Тестовая организация 2");
			browser.Button(Find.ByValue("Создать")).Click();
			Assert.That(browser.Text, Is.StringContaining("Пользователь создан"));

			client.Refresh();
			var createdAddress = client.Addresses.OrderBy(a => a.Id).Last();
			Assert.That(createdAddress.LegalEntity.Name, Is.EqualTo("Тестовая организация 2"));
		}

		[Test]
		public void Create_user_with_contact_person_info()
		{
			browser.Link(Find.ByText("Новый пользователь")).Click();
			browser.Link("addPersonLink").Click();
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

		[Test]
		public void Update_person_contact_info()
		{
			var client = DataMother.TestClient();
			using (var browser = Open(String.Format("client/{0}", client.Id)))
			{
				Click("Новый пользователь");
				browser.Link("addPersonLink").Click();
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
		public void Add_comment_for_contact()
		{
			var applyButtonText = "Сохранить";

			Open(user, "Edit");
			ContactInformationFixture.AddContact(browser, ContactType.Email, applyButtonText, client.Id);
			ContactInformationFixture.AddContact(browser, ContactType.Phone, applyButtonText, client.Id);
			using (new SessionScope())
			{
				client = Client.Find(client.Id);
				var group = client.Users[0].ContactGroup;
				browser.TextField(Find.ByName(String.Format("contacts[{0}].Comment", group.Contacts[0].Id))).TypeText("some comment");
				browser.Button(Find.ByValue("Сохранить")).Click();
			}

			// Проверка, что комментарий записан
			var contact = Contact.Find(client.Users[0].ContactGroup.Contacts[0].Id);
			Assert.That(contact.Comment, Is.EqualTo("some comment"));
		}

		[Test, NUnit.Framework.Description("Перемещение только пользователя (без адреса доставки) к другому клиенту")]
		public void Move_only_user_to_another_client()
		{
			var oldClient = DataMother.CreateTestClientWithAddressAndUser();
			var newClient = DataMother.CreateTestClientWithAddressAndUser();

			oldClient.Name += oldClient.Id.ToString();
			oldClient.SaveAndFlush();
			var user = oldClient.Users[0];
			newClient.Name += newClient.Id.ToString();
			newClient.SaveAndFlush();
			scope.Flush();

			using (var browser = Open("users/{0}/edit", user.Id))
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
			}

			oldClient.Refresh();
			newClient.Refresh();
			session.Refresh(user);
			Assert.That(user.Client.Id, Is.EqualTo(newClient.Id));
			Assert.That(newClient.Users.Count, Is.EqualTo(2));
			Assert.That(oldClient.Users.Count, Is.EqualTo(0));
		}

		[
			Test,
			NUnit.Framework.Description(@"
После перемещения пользователя должны быть созданы записи в UserPrices 
для тех регионов, которых не было у старого клиента, но они есть у нового")
		]
		public void After_user_moving_must_be_entries_in_UserPrices()
		{
			var supplier = DataMother.CreateSupplier(s => {
				s.AddRegion(Region.Find(16UL));
			});
			supplier.Save();
			var maskRegion = 1UL | 16UL;
			var newClient = DataMother.CreateTestClientWithAddressAndUser(maskRegion);

			var oldCountUserPrices = user.GetUserPriceCount();

			Open(user, "edit");
			browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
			browser.Button(Find.ById("SearchClientButton")).Click();
			Thread.Sleep(2000);
			browser.Button(Find.ByValue("Переместить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Пользователь успешно перемещен"));

			var newCountUserPricesEntries = user.GetUserPriceCount();
			Assert.That(oldCountUserPrices, Is.LessThan(newCountUserPricesEntries));

			Assert.That(GetCountUserPricesForRegion(user.Id, 16UL), Is.GreaterThan(0));
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

		[Test, NUnit.Framework.Description("Перемещение пользователя с адресом доставки к другому клиенту"), Ignore("Нет больше флага о переносе адреса")]
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
			using (var browser = Open(user, "edit"))
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
				session.Refresh(user);
				Assert.That(user.Client.Id, Is.EqualTo(newClient.Id));

				Assert.That(newClient.Users.Count, Is.EqualTo(2));
				Assert.That(oldClient.Users.Count, Is.EqualTo(0));

				Assert.That(newClient.Addresses.Count, Is.EqualTo(2));
				Assert.That(oldClient.Addresses.Count, Is.EqualTo(0));
			}
		}
	}
}