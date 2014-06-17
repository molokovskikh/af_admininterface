using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using System.IO;
using AdminInterface;
using Common.Web.Ui.Models;
using System.Threading;
using WatiN.Core.Native.Windows;

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
			Flush();

			Open(client);
			AssertText("Клиент");
		}

		[Test]
		public void Edit_user()
		{
			ClickLink(user.Login);
			AssertText(user.Login);
		}

		[Test]
		public void Set_user_parent()
		{
			var mainUser = client.AddUser("test", Guid.NewGuid().ToString().GetHashCode().ToString());

			Open("client/{0}", client.Id);
			ClickLink(user.Login);
			AssertText("Пользователь " + user.Login);
			ClickLink("Настройка");
			browser.SelectList(Find.ByName("user.InheritPricesFrom.Id")).Select(mainUser.LoginAndName);
			ClickButton("Сохранить");
			AssertText("Сохранен");

			session.Refresh(user);
			Assert.That(user.InheritPricesFrom.Id, Is.EqualTo(mainUser.Id));
		}

		[Test]
		public void View_password_change_statistics()
		{
			ClickLink(user.Login);
			AssertText(String.Format("Пользователь {0}", user.Login));

			ClickLink("Статистика изменения пароля");
			using (var stat = IE.AttachTo<IE>(Find.ByTitle(String.Format("Статистика изменения пароля для пользователя {0}", user.Login)))) {
				Assert.That(stat.Text, Is.StringContaining(String.Format("Статистика изменения пароля для пользователя {0}", user.Login)));
			}
		}

		[Test(Description = "Тест для проверки состояния галок 'Получать накладные', 'Получать отказы', 'Игнорировать проверку минимальной суммы заказа у Поставщика' при регистрации нового пользователя")]
		public void Check_flags_by_adding_user()
		{
			ClickLink("Новый пользователь");
			ClickButton("Создать");

			client = session.Load<Client>(client.Id);
			Assert.That(client.Users.Count, Is.GreaterThan(0));
			browser.GoTo(BuildTestUrl(String.Format("client/{0}", client.Id)));
			Refresh();
			var userLink = browser.Link(Find.ByText(client.Users[0].Login));
			Assert.IsTrue(userLink.Exists);
			userLink.Click();
			ClickLink("Настройка");
			Assert.IsFalse(browser.CheckBox(Find.ByName("user.SendWaybills")).Checked);
			Assert.IsTrue(browser.CheckBox(Find.ByName("user.SendRejects")).Checked);
			Assert.IsFalse(browser.CheckBox(Find.ByName("user.IgnoreCheckMinOrder")).Checked);
		}

		private void GoToChangePassword()
		{
			ClickLink(user.Login);
			AssertText(String.Format("Пользователь {0}", user.Login));
			ClickLink("Изменить пароль");
			AssertText("Изменение пароля пользователя");
		}

		[Test]
		public void Change_password_and_view_card()
		{
			GoToChangePassword();

			browser.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
			browser.CheckBox(Find.ByName("isSendClientCard")).Click();
			ClickButton("Изменить");
			AssertText("Регистрационная карта");
			AssertText("Изменение пароля по инициативе клиента");
		}

		[Test]
		public void Change_password_and_send_card()
		{
			GoToChangePassword();

			browser.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
			browser.TextField(Find.ByName("emailsForSend")).Clear();
			browser.TextField(Find.ByName("emailsForSend")).TypeText("KvasovTest@analit.net");
			ClickButton("Изменить");
			AssertText("Пароль успешно изменен");
		}

		[Test, NUnit.Framework.Description("При изменении пароля, если логин не совпадает с UserId и установлена соотв. опция, то изменить логин на UserId")]
		public void Change_login_when_change_password()
		{
			user.Login = "testLogin" + user.Id;
			session.SaveOrUpdate(user);
			Refresh();

			GoToChangePassword();
			browser.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
			browser.TextField(Find.ByName("emailsForSend")).TypeText("kvasovtest@analit.net");
			Assert.That(browser.RadioButton(Find.ById("changeLogin")).Checked, Is.True);
			ClickButton("Изменить");
			AssertText("Пароль успешно изменен");

			session.Refresh(user);
			Assert.That(user.Login, Is.EqualTo(user.Id.ToString()));
		}

		[Test, NUnit.Framework.Description("При изменении пароля, если логин совпадает с UserId то изменять логин не нужно")]
		public void Not_change_login_when_change_password()
		{
			GoToChangePassword();
			browser.TextField(Find.ByName("reason")).TypeText("Тестовое изменение пароля");
			browser.TextField(Find.ByName("emailsForSend")).TypeText("kvasovtest@analit.net");
			Assert.That(browser.RadioButton(Find.ById("changeLogin")).Exists, Is.False);
			ClickButton("Изменить");
			AssertText("Пароль успешно изменен");

			session.Refresh(user);
			Assert.That(user.Login, Is.EqualTo(user.Id.ToString()));
		}

		[Test]
		public void Create_user()
		{
			AssertText("Клиент test");
			ClickLink("Новый пользователь");

			AssertText("Новый пользователь");
			browser.TextField(Find.ByName("user.Name")).TypeText("test");
			ClickButton("Создать");
			AssertText("Регистрационная карта");

			session.Refresh(client);
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
			session.Save(user.UserUpdateInfo);
			Assert.That(user.HaveUin(), Is.True);
			session.Flush();

			Refresh();
			ClickLink(user.Login);
			ClickButton("Сбросить УИН");
			AssertText("Это поле необходимо заполнить.");
			browser.TextField(Find.ByName("reason")).TypeText("test reason");
			ClickButton("Сбросить УИН");
			AssertText("УИН сброшен");
			AssertText(String.Format("$$$ Пользователь: {0}", user.Login));

			session.Refresh(user.UserUpdateInfo);
			Assert.That(user.UserUpdateInfo.AFCopyId, Is.Empty);
		}

		[Test]
		public void Delete_user_prepared_data()
		{
			var preparedDataPath = String.Format(@"C:\Windows\Temp\{0}_123456.zip", user.Id);

			ClickLink(user.Login);
			Assert.That(browser.Button(Find.ByValue("Удалить подготовленные данные")).Enabled, Is.False);
			var directory = Path.GetDirectoryName(preparedDataPath);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			var file = File.Create(preparedDataPath);

			Refresh();

			Assert.That(browser.Button(Find.ByValue("Удалить подготовленные данные")).Enabled, Is.True);
			ClickButton("Удалить подготовленные данные");
			AssertText("Ошибка удаления подготовленных данных, попробуйте позднее.");
			file.Close();
			ClickButton("Удалить подготовленные данные");
			AssertText("Подготовленные данные удалены");
			try {
				File.Delete(file.Name);
			}
			catch {
			}
		}

		[Test]
		public void EditAnalitFSettings()
		{
			Click(user.Login);
			Click("Настройка");

			var types = new[] {
				UserPermissionTypes.Base,
				UserPermissionTypes.AnalitFExcel,
				UserPermissionTypes.AnalitFPrint
			};
			var total = session.Query<UserPermission>().Count(p => types.Contains(p.Type));
			for (int i = 0; i < total; i++)
				browser.CheckBox(Find.ByName(String.Format("user.AssignedPermissions[{0}].Id", i)))
					.Checked = (i % 2 == 0);

			ClickButton("Сохранить");
			AssertText("Сохранено");

			browser.Back();
			browser.Back();
			browser.Back();

			ClickLink("Новый пользователь");
			browser.TextField(Find.ByName("user.Name")).TypeText("test2");

			for (int i = 0; i < total; i++)
				browser.CheckBox(Find.ByName(String.Format("user.AssignedPermissions[{0}].Id", i)))
					.Checked = (i % 2 == 0);

			ClickButton("Создать");

			session.Refresh(client);
			session.Refresh(user);
			Assert.AreEqual(2, client.Users.Count);

			Assert.AreEqual(total / 2, client.Users[0].AssignedPermissions.Count);
			Assert.AreEqual(total / 2, client.Users[1].AssignedPermissions.Count);
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
			ContactInformationHelper.AddContact(browser, ContactType.Email, applyButtonText, client.Id);
			AssertText("Сохранено");
			ContactInformationHelper.AddContact(browser, ContactType.Phone, applyButtonText, client.Id);
			AssertText("Сохранено");

			session.Refresh(user);
			var group = user.ContactGroup;
			Assert.That(client.ContactGroupOwner.Id, Is.EqualTo(group.ContactGroupOwner.Id),
				"Не совпадают Id владельца группы у клиента и у новой группы");
			// Проверка, что контактные записи создались в БД
			ContactInformationHelper.CheckContactGroupInDb(group);
			var countContacts = ContactInformationHelper.GetCountContactsInDb(group);
			Assert.That(countContacts, Is.EqualTo(2));
		}

		[Test]
		public void Add_person_information()
		{
			var applyButtonText = "Сохранить";
			Open(user, "Edit");

			ContactInformationHelper.AddPerson(browser, "Test person", applyButtonText, client.Id);
			AssertText("Сохранено");
			ContactInformationHelper.AddPerson(browser, "Test person2", applyButtonText, client.Id);
			AssertText("Сохранено");

			session.Refresh(client);
			session.Refresh(client.Users[0]);
			var group = client.Users[0].ContactGroup;
			Assert.That(client.ContactGroupOwner.Id, Is.EqualTo(group.ContactGroupOwner.Id),
				"Не совпадают Id владельца группы у клиента и у новой группы");
			// Проверка, что контактные записи создались в БД
			ContactInformationHelper.CheckContactGroupInDb(group);
			var persons = ContactInformationHelper.GetPersons(group);
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

			ContactInformationHelper.AddContact(browser, ContactType.Email, applyButtonText, client.Id);
			ContactInformationHelper.AddContact(browser, ContactType.Phone, applyButtonText, client.Id);

			session.Refresh(user);
			var group = user.ContactGroup;
			browser.Button(Find.ByName(String.Format("contacts[{0}].Delete", group.Contacts[0].Id))).Click();
			ClickButton("Сохранить");
			// Проверка, что контактная запись удалена
			var countContacts = ContactInformationHelper.GetCountContactsInDb(client.Users[0].ContactGroup);
			Assert.That(countContacts, Is.EqualTo(1));
		}

		[Test]
		public void Delete_person_information()
		{
			var applyButtonText = "Сохранить";
			Open(user, "Edit");

			ContactInformationHelper.AddPerson(browser, "Test person", applyButtonText, client.Id);
			AssertText("Сохранено");
			ContactInformationHelper.AddPerson(browser, "Test person2", applyButtonText, client.Id);
			AssertText("Сохранено");

			session.Refresh(user);
			var group = user.ContactGroup;
			browser.Button(Find.ByName(String.Format("persons[{0}].Delete", group.Persons[0].Id))).Click();
			ClickButton("Сохранить");
			// Проверка, что контактная запись удалена
			var persons = ContactInformationHelper.GetPersons(client.Users[0].ContactGroup);
			Assert.That(persons.Count, Is.EqualTo(1));
			Assert.That(persons[0], Is.EqualTo("Test person2"));
		}

		[Test]
		public void Delete_person_information_by_fill_empty_string()
		{
			var applyButtonText = "Сохранить";
			Open(user, "Edit");

			ContactInformationHelper.AddPerson(browser, "Test person", applyButtonText, client.Id);
			AssertText("Сохранено");
			ContactInformationHelper.AddPerson(browser, "Test person2", applyButtonText, client.Id);
			AssertText("Сохранено");

			session.Refresh(user);
			var group = user.ContactGroup;
			browser.TextField(Find.ByName(String.Format("persons[{0}].Name", group.Persons[0].Id))).TypeText("");
			ClickButton("Сохранить");
			// Проверка, что контактная запись удалена
			var persons = ContactInformationHelper.GetPersons(client.Users[0].ContactGroup);
			Assert.That(persons.Count, Is.EqualTo(1));
			Assert.That(persons[0], Is.EqualTo("Test person2"));
		}

		[Test]
		public void Change_person_information()
		{
			var applyButtonText = "Сохранить";
			Open(user, "Edit");
			ContactInformationHelper.AddPerson(browser, "Test person", applyButtonText, client.Id);

			session.Refresh(user);
			var group = user.ContactGroup;
			browser.TextField(Find.ByName(String.Format("persons[{0}].Name", group.Persons[0].Id))).TypeText("Test person changed");
			ClickButton("Сохранить");
			AssertText("Сохранено");
			// Проверка, что контактная запись изменена
			var persons = ContactInformationHelper.GetPersons(user.ContactGroup);
			Assert.That(persons.Count, Is.EqualTo(1));
			Assert.That(persons[0], Is.EqualTo("Test person changed"));
		}

		[Test]
		public void TestUserRegions()
		{
			client.MaskRegion = 7;
			session.SaveOrUpdate(client);
			settings.OrderRegionMask = 7;
			session.SaveOrUpdate(settings);
			user.WorkRegionMask = 2;
			user.OrderRegionMask = 1;
			session.SaveOrUpdate(user);
			Flush();

			Open(user, "Edit");
			Click("Настройка");
			Wait(() => browser.CheckBox("WorkRegions[0]").Checked, "Не дождались");
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
			ClickButton("Сохранить");
			Click("Настройка");

			session.Refresh(user);
			Assert.AreEqual(3, user.WorkRegionMask);
			Assert.AreEqual(3, user.OrderRegionMask);

			browser.CheckBox("WorkRegions[1]").Checked = false;
			browser.CheckBox("OrderRegions[0]").Checked = false;
			ClickButton("Сохранить");
			Click("Настройка");

			session.Refresh(user);
			Assert.AreEqual(2, user.WorkRegionMask);
			Assert.AreEqual(1, user.OrderRegionMask);

			client.MaskRegion = 31;
			session.SaveOrUpdate(client);
			settings.OrderRegionMask = 3;
			session.SaveOrUpdate(settings);

			Refresh();
			Wait(() => browser.CheckBox("WorkRegions[3]").Exists, "Не дождались");
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
			session.SaveOrUpdate(client);
			foreach (var region in browseRegions)
				settings.WorkRegionMask |= region;
			foreach (var region in orderRegions)
				settings.OrderRegionMask |= region;
			session.SaveOrUpdate(settings);
			Flush();

			ClickLink("Новый пользователь");
			Thread.Sleep(2000);
			AssertText("Новый пользователь");
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
			ClickButton("Создать");

			AssertText("Регистрационная карта ");
			var login = Helper.GetLoginFromRegistrationCard(browser);
			browser.GoTo(BuildTestUrl(String.Format("client/{0}", client.Id)));
			Refresh();
			ClickLink(login.ToString());
			Click("Настройка");
			// Проверяем, чтобы были доступны нужные регионы. Берем с первого региона, т.к. галку с нулевого сняли
			for (var i = 1; i < browseRegions.Length; i++)
				Assert.IsTrue(browser.CheckBox(Find.ById(String.Format("WorkRegions[{0}]", i))).Checked);
			for (var i = 1; i < orderRegions.Length; i++)
				Assert.IsTrue(browser.CheckBox(Find.ById(String.Format("OrderRegions[{0}]", i))).Checked);
		}

		[Test]
		public void User_must_be_enabled_after_registration()
		{
			ClickLink("Новый пользователь");
			browser.TextField(Find.ByName("user.Name")).TypeText("test user");
			ClickButton("Создать");
			browser.GoTo(BuildTestUrl(String.Format("client/{0}", client.Id)));
			client = session.Load<Client>(client.Id);
			Assert.That(client.Users.Count, Is.EqualTo(1));
			Assert.IsTrue(client.Users[0].Enabled);
		}

		[Test, NUnit.Framework.Description("Регистрация пользователя. Проверка валидатора списка email-ов для отправки регистрационной карты")]
		public void Validate_email_list_for_sending_registration_card()
		{
			ClickLink("Новый пользователь");
			FillRequiredFields();
			browser.TextField(Find.ByName("mails")).TypeText("asjkdf sdfj34kjl 4 ./4,524,l5; ");
			ClickButton("Создать");
			AssertText("Поле содержит некорректный адрес электронной почты");
			browser.TextField(Find.ByName("mails")).TypeText("test1@test.test,test2@test.test,    test3@test.test.");
			ClickButton("Создать");
			AssertText("Поле содержит некорректный адрес электронной почты");
			browser.TextField(Find.ByName("mails")).TypeText("test1@test.test,test2@test.test,    test3@test.test");
			ClickButton("Создать");
			AssertText("Пользователь создан");
		}

		[Test, NUnit.Framework.Description("Тест для регистрации адреса при регистрации пользователя")]
		public void Create_user_with_address()
		{
			RegisterUserWithAddress(client, browser);
			ClickButton("Создать");
			AssertText("Пользователь создан");
			AssertText("Коометарий Тестовый пользователь");

			session.Refresh(client);
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
			ClickLink("Новый пользователь");
			browser.CheckBox(Find.ByName("sendClientCard")).Checked = true;
			browser.TextField(Find.ByName("mails")).TypeText("KvasovTest@analit.net");
			browser.TextField(Find.ByName("address.Value")).TypeText("TestAddress");
			FillRequiredFields();
		}

		private void FillRequiredFields()
		{
			Css("#user_Name").TypeText("Коометарий Тестовый пользователь");
		}

		[Test]
		public void RegisterUserForMultiPayerClient()
		{
			var payer = new Payer("Тестовый плательщик");
			session.Save(payer);
			client.Payers.Add(payer);
			session.SaveOrUpdate(client);
			Open(client);
			ClickLink("Новый пользователь");
			FillRequiredFields();
			Click("Создать");
			AssertText("Заполнение поля обязательно");
			Css("#user_Payer_Id").Select("Тестовый плательщик");
			Click("Создать");
			AssertText("Регистрационная карта");
			session.Refresh(client);
			Assert.That(client.Users.Count, Is.EqualTo(2));
		}

		[Test]
		public void Register_user_with_addresses_not_default_legal_entity()
		{
			var payer = client.Payers.First();
			var legalEntity = new LegalEntity {
				Name = "Тестовая организация 2",
				Payer = payer
			};
			session.Save(legalEntity);
			payer.JuridicalOrganizations.Add(legalEntity);

			RegisterUserWithAddress(client, browser);
			Css("#address_LegalEntity_Id").Select("Тестовая организация 2");
			ClickButton("Создать");
			AssertText("Пользователь создан");

			session.Refresh(client);
			var createdAddress = client.Addresses.OrderBy(a => a.Id).Last();
			Assert.That(createdAddress.LegalEntity.Name, Is.EqualTo("Тестовая организация 2"));
		}

		[Test]
		public void Create_user_with_contact_person_info()
		{
			ClickLink("Новый пользователь");
			FillRequiredFields();
			browser.Link("addPersonLink").Click();
			browser.TextField(Find.ByName(String.Format("persons[-1].Name"))).TypeText("Alice");
			browser.TextField(Find.ByName("mails")).TypeText("KvasovTest@analit.net");
			browser.TextField(Find.ByName("address.Value")).TypeText("TestAddress");
			ClickButton("Создать");
			AssertText("Пользователь создан");

			session.Refresh(client);
			Assert.That(client.ContactGroupOwner.ContactGroups.Count, Is.EqualTo(1));
			var persons = client.ContactGroupOwner.ContactGroups[0].Persons;
			Assert.That(persons.Count, Is.EqualTo(1));
			Assert.That(persons[0].Name, Is.EqualTo("Alice"));
		}

		[Test]
		public void Update_person_contact_info()
		{
			Click("Новый пользователь");

			FillRequiredFields();
			browser.Link("addPersonLink").Click();
			browser.TextField(Find.ByName("persons[-1].Name")).TypeText("Alice");
			browser.TextField(Find.ByName("mails")).TypeText("KvasovTest@analit.net");
			browser.TextField(Find.ByName("address.Value")).TypeText("TestAddress");
			ClickButton("Создать");
			AssertText("Пользователь создан");

			Open(client);

			session.Refresh(client);
			var user = client.Users[1];
			ClickLink(user.Id.ToString());
			var group = user.ContactGroup;
			var person = @group.Persons[0];
			Assert.That(browser.TextField(Find.ByName(String.Format("persons[{0}].Name", person.Id))).Text, Is.EqualTo("Alice"));
			browser.TextField(Find.ByName(String.Format("persons[{0}].Name", person.Id))).TypeText("Alice modified");
			ClickButton("Сохранить");
			AssertText("Сохранено");

			var persons = client.ContactGroupOwner.ContactGroups[0].Persons;
			Assert.That(persons.Count, Is.EqualTo(1));
			session.Refresh(person);
			Assert.That(persons[0].Name, Is.EqualTo("Alice modified"));
		}

		[Test]
		public void Add_comment_for_contact()
		{
			var applyButtonText = "Сохранить";

			Open(user, "Edit");
			ContactInformationHelper.AddContact(browser, ContactType.Email, applyButtonText, client.Id);
			ContactInformationHelper.AddContact(browser, ContactType.Phone, applyButtonText, client.Id);
			session.Refresh(client.Users[0]);
			var group = client.Users[0].ContactGroup;
			var contact = @group.Contacts[0];
			browser.TextField(Find.ByName(String.Format("contacts[{0}].Comment", contact.Id))).TypeText("some comment");
			ClickButton("Сохранить");

			// Проверка, что комментарий записан
			session.Refresh(contact);
			Assert.That(contact.Comment, Is.EqualTo("some comment"));
		}

		[Test, NUnit.Framework.Description("Перемещение только пользователя (без адреса доставки) к другому клиенту")]
		public void Move_only_user_to_another_client()
		{
			var oldClient = DataMother.CreateTestClientWithAddressAndUser();
			var newClient = DataMother.CreateTestClientWithAddressAndUser();

			oldClient.Name += oldClient.Id.ToString();
			session.SaveOrUpdate(oldClient);
			var user = oldClient.Users[0];
			newClient.Name += newClient.Id.ToString();
			session.SaveOrUpdate(newClient);

			Open("users/{0}/edit", user.Id);
			browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
			browser.Button(Find.ById("SearchClientButton")).Click();
			Thread.Sleep(2000);
			Assert.IsTrue(browser.SelectList(Find.ById("clientsList")).Exists);
			Assert.That(browser.SelectList(Find.ById("clientsList")).Options.Count, Is.GreaterThan(0));

			Assert.IsTrue(browser.Button(Find.ByValue("Отмена")).Exists);
			Assert.IsTrue(browser.Button(Find.ByValue("Переместить")).Exists);

			ClickButton("Переместить");
			AssertText("Пользователь успешно перемещен");

			session.Refresh(oldClient);
			session.Refresh(newClient);
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
			var supplier = DataMother.CreateSupplier(s => s.AddRegion(session.Load<Region>(16UL), session));
			Save(supplier);
			var maskRegion = 1UL | 16UL;
			var newClient = DataMother.CreateTestClientWithAddressAndUser(maskRegion);

			var oldCountUserPrices = user.GetUserPriceCount();

			Open(user, "edit");
			browser.TextField(Find.ById("TextForSearchClient")).TypeText(newClient.Id.ToString());
			browser.Button(Find.ById("SearchClientButton")).Click();
			Thread.Sleep(2000);
			ClickButton("Переместить");
			AssertText("Пользователь успешно перемещен");

			var newCountUserPricesEntries = user.GetUserPriceCount();
			Assert.That(oldCountUserPrices, Is.LessThan(newCountUserPricesEntries));

			Assert.That(GetCountUserPricesForRegion(user.Id, 16UL), Is.GreaterThan(0));
		}

		private uint GetCountUserPricesForRegion(uint userId, ulong regionId)
		{
			return Convert.ToUInt32(ArHelper.WithSession(session => session.CreateSQLQuery(@"
SELECT COUNT(*)
FROM
	Customers.UserPrices
WHERE UserId = :UserId AND RegionId = :RegionId
")
				.SetParameter("UserId", userId)
				.SetParameter("RegionId", regionId)
				.UniqueResult()));
		}

		[Test]
		public void Message_on_forbidden_symbols()
		{
			browser.TextField(Find.ByName("message")).TypeText("<Тестовое сообщение с ><запрещенными<> символами");
			ClickButton("Принять");
			Assert.That(browser.ContainsText("Поле содержит запрещенные символы(<, >)."), Is.True);
		}

		[Test]
		public void Reset_af_version_test()
		{
			Open(user, "Settings");
			Click(string.Format("Сбросить версию АФ (Сейчас {0})", user.UserUpdateInfo.AFAppVersion));
			AssertText("Вы уверены, что хотите произвести сброс параметра, отвечающего за версию АФ");
		}
	}
}