using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
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

			ClickLink("Региональные администраторы");
			ClickButton("Создать");
			var id = UserCommon.GeneratePassword();
			var login = String.Format("admin{0}", id);
			var managerName = String.Format("adminName{0}", id);
			browser.TextField(Find.ByName("administrator.UserName")).TypeText(login);
			browser.TextField(Find.ByName("administrator.ManagerName")).TypeText(managerName);
			browser.TextField(Find.ByName("administrator.PhoneSupport")).TypeText("123-123123");
			browser.TextField(Find.ByName("administrator.InternalPhone")).TypeText("123");
			browser.TextField(Find.ByName("administrator.Email")).TypeText(String.Format("{0}@admin.net", id));
			browser.SelectList(Find.ByName("administrator.Department")).Select("IT");
			ClickButton("Сохранить");

			CheckRegistrationCard(browser, id, login, managerName);

			var admins = Administrator.FindAll();
			Assert.That(admins.Where(admin => admin.UserName.Equals(login)).Count(), Is.EqualTo(1));
		}

		private void CheckRegistrationCard(Browser browser, string id, string login, string managerName)
		{
			AssertText("Регистрационная карта");
			AssertText(login);
			AssertText(id);
			AssertText(managerName);
		}

		[Test]
		public void Check_required_fields()
		{
			Open("Main/Index");

			ClickLink("Региональные администраторы");
			ClickButton("Создать");
			var id = UserCommon.GeneratePassword();
			var login = String.Format("admin{0}", id);
			var managerName = String.Format("adminName{0}", id);
			ClickSaveAndCheckRequired(browser);
			browser.TextField(Find.ByName("administrator.UserName")).TypeText(login);
			ClickSaveAndCheckRequired(browser);
			browser.TextField(Find.ByName("administrator.ManagerName")).TypeText(managerName);
			ClickSaveAndCheckRequired(browser);
			browser.TextField(Find.ByName("administrator.PhoneSupport")).TypeText("123-123123");
			browser.TextField(Find.ByName("administrator.Email")).TypeText("kvasovtest@analit.net");
			ClickButton("Сохранить");
			CheckRegistrationCard(browser, id, login, managerName);
		}

		private void ClickSaveAndCheckRequired(Browser browser)
		{
			ClickButton("Сохранить");
			AssertText("Заполнение поля обязательно");
		}

		[Test]
		public void Edit_personal_info()
		{
			var admin = OpenAdmin();

			var phone = new Random().Next(100, 999);
			browser.TextField(Find.ByName("administrator.InternalPhone")).TypeText(phone.ToString());

			var departmentId = new Random().Next(1, 6);
			var department = (Department)Enum.ToObject(typeof(Department), departmentId);
			browser.SelectList(Find.ByName("administrator.Department")).Select(department.GetDescription());

			ClickButton("Сохранить");
			AssertText("Сохранено");

			browser.GoTo(BuildTestUrl("ViewAdministrators.aspx"));
			ClickLink(admin.UserName);
			Assert.That(browser.TextField(Find.ByName("administrator.InternalPhone")).Text, Is.EqualTo(phone.ToString()));

			session.Refresh(admin);
			Assert.That(admin.InternalPhone, Is.EqualTo(phone.ToString()));
			Assert.That(admin.Department, Is.EqualTo(department));
		}

		[Test]
		public void Edit_regional_settings()
		{
			var admin = OpenAdmin();
			var regionId = Convert.ToUInt32(browser.Element(Find.ByName("accessibleRegions[0].Id")).GetValue("value"));
			browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked = false;
			ClickButton("Сохранить");
			AssertText("Сохранено");
			Assert.IsFalse(browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked);

			session.Refresh(admin);
			Assert.That(admin.RegionMask & regionId, Is.EqualTo(0));

			browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked = true;
			ClickButton("Сохранить");
			AssertText("Сохранено");
			Assert.IsTrue(browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked);

			session.Refresh(admin);
			Assert.That(admin.RegionMask & regionId, Is.GreaterThan(0));
		}

		[Test]
		public void Edit_permissions()
		{
			var admin = OpenAdmin();

			browser.TextField(Find.ByName("administrator.PhoneSupport")).TypeText("123-123123");
			var id = Convert.ToUInt32(browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).GetAttributeValue("value"));
			var permissionType = (PermissionType)Enum.ToObject(typeof(PermissionType), id);

			browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked = true;
			ClickButton("Сохранить");
			AssertText("Сохранено");
			Assert.IsTrue(browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked);

			session.Refresh(admin);
			Assert.IsTrue(admin.HavePermision(permissionType));

			browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked = false;
			ClickButton("Сохранить");
			AssertText("Сохранено");
			Assert.IsFalse(browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked);

			session.Refresh(admin);
			Assert.IsFalse(admin.HavePermision(permissionType));
		}

		private Administrator OpenAdmin()
		{
			var admin = CreateAdmin();

			Open("Main/Index");
			ClickLink("Региональные администраторы");
			ClickLink(admin.UserName);
			AssertText("Личные данные");
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
			Save(admin);
			return admin;
		}
	}
}