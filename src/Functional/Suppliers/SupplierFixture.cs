using AdminInterface.Models;
using Castle.ActiveRecord;
using Functional.Billing;
using Functional.ForTesting;
using Integration.ForTesting;
using log4net.Config;
using NUnit.Framework;
using WatiN.Core;
using WatiNCssSelectorExtensions;

namespace Functional.Suppliers
{
	public class SupplierFixture : WatinFixture2
	{
		private User user;

		[SetUp]
		public void SetUp()
		{
			user = DataMother.CreateSupplierUser();
			scope.Flush();
		}

		[Test]
		public void Search_supplier_user()
		{
			Open("/users/search");
			browser.Css("#SearchText").TypeText(user.Id.ToString());
			browser.Button(Find.ByValue("�����")).Click();
			Assert.That(browser.Text, Is.StringContaining(user.Login));

			browser.Link(Find.ByText(user.Id.ToString())).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));
		}

		[Test]
		public void Search_supplier()
		{
			Open("/users/search");
			browser.Css("#SearchText").TypeText("�������� ���������");
			browser.Button(Find.ByValue("�����")).Click();
			Assert.That(browser.Text, Is.StringContaining("�������� ���������"));

			browser.Link(Find.ByText("�������� ���������")).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));
		}
	}
}