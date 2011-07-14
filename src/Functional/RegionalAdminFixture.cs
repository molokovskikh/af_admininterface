using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Common.Tools;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;

namespace Functional
{
	[TestFixture]
	public class RegionalAdminFixture : WatinFixture2
	{
		[Test]
		public void Create_regional_admin()
		{
			Open("Main/Index");

			browser.Link(Find.ByText("Региональные администраторы")).Click();
			browser.Button(Find.ByValue("Создать")).Click();
			var id = User.GeneratePassword();
			var login = String.Format("admin{0}", id);
			var managerName = String.Format("adminName{0}", id);
			browser.TextField(Find.ByName("administrator.UserName")).TypeText(login);
			browser.TextField(Find.ByName("administrator.ManagerName")).TypeText(managerName);
			browser.TextField(Find.ByName("administrator.PhoneSupport")).TypeText("123-123123");
			browser.TextField(Find.ByName("administrator.InternalPhone")).TypeText("123");
			browser.TextField(Find.ByName("administrator.Email")).TypeText(String.Format("{0}@admin.net", id));
			browser.SelectList(Find.ByName("administrator.Department")).Select("IT");
			browser.Button(Find.ByValue("Сохранить")).Click();

			CheckRegistrationCard(browser, id, login, managerName);

			var admins = Administrator.FindAll();
			Assert.That(admins.Where(admin => admin.UserName.Equals(login)).Count(), Is.EqualTo(1));
		}

		private void CheckRegistrationCard(Browser browser, string id, string login, string managerName)
		{
			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта"));
			Assert.That(browser.Text, Is.StringContaining(login));
			Assert.That(browser.Text, Is.StringContaining(id));
			Assert.That(browser.Text, Is.StringContaining(managerName));
		}

		[Test]
		public void Check_required_fields()
		{
			Open("Main/Index");

			browser.Link(Find.ByText("Региональные администраторы")).Click();
			browser.Button(Find.ByValue("Создать")).Click();
			var id = User.GeneratePassword();
			var login = String.Format("admin{0}", id);
			var managerName = String.Format("adminName{0}", id);
			ClickSaveAndCheckRequired(browser);
			browser.TextField(Find.ByName("administrator.UserName")).TypeText(login);
			ClickSaveAndCheckRequired(browser);
			browser.TextField(Find.ByName("administrator.ManagerName")).TypeText(managerName);
			ClickSaveAndCheckRequired(browser);
			browser.TextField(Find.ByName("administrator.PhoneSupport")).TypeText("123-123123");
			browser.TextField(Find.ByName("administrator.Email")).TypeText("kvasovtest@analit.net");
			browser.Button(Find.ByValue("Сохранить")).Click();
			CheckRegistrationCard(browser, id, login, managerName);
		}

		private void ClickSaveAndCheckRequired(Browser browser)
		{
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Заполнение поля обязательно"));
		}

		[Test]
		public void Edit_personal_info()
		{
			var admin = OpenAdmin();

			var phone = new Random().Next(100, 999);
			browser.TextField(Find.ByName("administrator.InternalPhone")).TypeText(phone.ToString());

			var departmentId = new Random().Next(1, 6);
			var department = (Department) Enum.ToObject(typeof (Department), departmentId);
			browser.SelectList(Find.ByName("administrator.Department")).Select(department.GetDescription());
				
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));

			browser.GoTo(BuildTestUrl("ViewAdministrators.aspx"));
			browser.Link(Find.ByText(admin.UserName)).Click();
			Assert.That(browser.TextField(Find.ByName("administrator.InternalPhone")).Text, Is.EqualTo(phone.ToString()));

			admin.Refresh();
			Assert.That(admin.InternalPhone, Is.EqualTo(phone.ToString()));
			Assert.That(admin.Department, Is.EqualTo(department));
		}

		[Test]
		public void Edit_regional_settings()
		{
			var admin = OpenAdmin();
			var regionId = Convert.ToUInt32(browser.Element(Find.ByName("accessibleRegions[0].Id")).GetValue("value"));
			browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked = false;
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			Assert.IsFalse(browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked);

			admin.Refresh();
			Assert.That(admin.RegionMask & regionId, Is.EqualTo(0));

			browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked = true;
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			Assert.IsTrue(browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked);

			admin.Refresh();
			Assert.That(admin.RegionMask & regionId, Is.GreaterThan(0));
		}

		[Test]
		public void Edit_permissions()
		{
			var admin = OpenAdmin();

			browser.TextField(Find.ByName("administrator.PhoneSupport")).TypeText("123-123123");
			var id = Convert.ToUInt32(browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).GetAttributeValue("value"));
			var permissionType = (PermissionType) Enum.ToObject(typeof (PermissionType), id);

			browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked = true;
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			Assert.IsTrue(browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked);

			admin.Refresh();
			Assert.IsTrue(admin.HavePermision(permissionType));

			browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked = false;
			browser.Button(Find.ByValue("Сохранить")).Click();
			Assert.That(browser.Text, Is.StringContaining("Сохранено"));
			Assert.IsFalse(browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked);

			admin.Refresh();
			Assert.IsFalse(admin.HavePermision(permissionType));
		}

		private Administrator OpenAdmin()
		{
			var admin = CreateAdmin();

			Open("Main/Index");
			browser.Link(Find.ByText("Региональные администраторы")).Click();
			browser.Link(Find.ByText(admin.UserName)).Click();
			Assert.That(browser.Text, Is.StringContaining("Личные данные"));
			return admin;
		}

		private Administrator CreateAdmin()
		{
			var admin = new Administrator {
				UserName = "admin" + new Random().Next(),
				ManagerName = "Тестовый администратор",
				Email = "kvasovtest@analit.net",
				PhoneSupport = "4732-606000",
			};
			admin.RegionMask = ulong.MaxValue;
			admin.AllowedPermissions = Enum.GetValues(typeof(PermissionType))
				.Cast<PermissionType>()
				.Select(p => Permission.Find(p))
				.ToList();
			admin.Save();
			return admin;
		}
	}
}
