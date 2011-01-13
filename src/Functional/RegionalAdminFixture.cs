using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using AdminInterface.Models.Security;
using Castle.ActiveRecord;

namespace Functional
{
	[TestFixture]
	public class RegionalAdminFixture : WatinFixture
	{
		private string _administratorLogin = "Boss";

		[Test]
		public void Create_regional_admin()
		{
			using (var browser = Open("Main/Index"))
			{
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
				browser.Button(Find.ByValue("Создать")).Click();

				CheckRegistrationCard(browser, id, login, managerName);
				using (new SessionScope())
				{
					var admins = Administrator.FindAll();
					Assert.That(admins.Where(admin => admin.UserName.Equals(login)).Count(), Is.EqualTo(1));
				}
			}
		}

		private void CheckRegistrationCard(IE browser, string id, string login, string managerName)
		{
			Assert.That(browser.Text, Is.StringContaining("Регистрационная карта"));
			Assert.That(browser.Text, Is.StringContaining(login));
			Assert.That(browser.Text, Is.StringContaining(id));
			Assert.That(browser.Text, Is.StringContaining(managerName));
		}

		[Test]
		public void Check_required_fields()
		{
			using (var browser = Open("Main/Index"))
			{
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
				browser.Button(Find.ByValue("Создать")).Click();
				CheckRegistrationCard(browser, id, login, managerName);
			}
		}

		private void ClickSaveAndCheckRequired(IE browser)
		{
			browser.Button(Find.ByValue("Создать")).Click();
			Assert.That(browser.Text, Is.StringContaining("Это поле необходимо заполнить"));
		}

		[Test]
		public void Edit_personal_info()
		{
			using (var browser = Open("Main/Index"))
			{
				browser.Link(Find.ByText("Региональные администраторы")).Click();
				browser.Link(Find.ByText(_administratorLogin)).Click();
				var phone = new Random().Next(100, 999);
				browser.TextField(Find.ByName("administrator.InternalPhone")).TypeText(phone.ToString());

				var departmentId = new Random().Next(1, 6);
				var department = (Department) Enum.ToObject(typeof (Department), departmentId);
				browser.SelectList(Find.ByName("administrator.Department")).Select(department.GetDescription());
				
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));

				browser.GoTo(BuildTestUrl("ViewAdministrators.aspx"));
				browser.Link(Find.ByText(_administratorLogin)).Click();
				Assert.That(browser.TextField(Find.ByName("administrator.InternalPhone")).Text, Is.EqualTo(phone.ToString()));
				var admin = Administrator.GetByName(_administratorLogin);
				Assert.That(admin.InternalPhone, Is.EqualTo(phone.ToString()));
				Assert.That(admin.Department, Is.EqualTo(department));
			}
		}

		[Test]
		public void Edit_regional_settings()
		{
			using (var browser = Open("Main/Index"))
			{
				browser.Link(Find.ByText("Региональные администраторы")).Click();
				browser.Link(Find.ByText(_administratorLogin)).Click();

				var regionId = Convert.ToUInt32(browser.Element(Find.ByName("accessibleRegions[0].Id")).GetValue("value"));
				browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked = false;
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				Assert.IsFalse(browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked);
				var admin = Administrator.GetByName(_administratorLogin);
				Assert.That(admin.RegionMask & regionId, Is.EqualTo(0));

				browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked = true;
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				Assert.IsTrue(browser.CheckBox(Find.ByName("accessibleRegions[0].IsAvaliableForBrowse")).Checked);
				admin = Administrator.GetByName(_administratorLogin);
				Assert.That(admin.RegionMask & regionId, Is.GreaterThan(0));
			}
		}

		[Test]
		public void Edit_permissions()
		{
			using (var browser = Open("Main/Index"))
			{
				browser.Link(Find.ByText("Региональные администраторы")).Click();
				browser.Link(Find.ByText(_administratorLogin)).Click();
				browser.TextField(Find.ByName("administrator.PhoneSupport")).TypeText("123-123123");
				var id = Convert.ToUInt32(browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).GetAttributeValue("value"));

				browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked = true;
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				Assert.IsTrue(browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked);
				var admin = Administrator.GetByName(_administratorLogin);
				Assert.IsTrue(admin.HavePermision((PermissionType)Enum.ToObject(typeof (PermissionType), id)));

				browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked = false;
				browser.Button(Find.ByValue("Сохранить")).Click();
				Assert.That(browser.Text, Is.StringContaining("Сохранено"));
				Assert.IsFalse(browser.CheckBox(Find.ByName("administrator.AllowedPermissions[0].Id")).Checked);
				admin = Administrator.GetByName(_administratorLogin);
				Assert.IsFalse(admin.HavePermision((PermissionType) Enum.ToObject(typeof (PermissionType), id)));
			}
		}
	}
}
