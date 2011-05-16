using System;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
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
		private Supplier supplier;

		[SetUp]
		public void SetUp()
		{
			user = DataMother.CreateSupplierUser();
			supplier = (Supplier)user.RootService;
			scope.Flush();
		}

		[Test]
		public void Search_supplier_user()
		{
			Open("/users/search");
			browser.Css("#SearchText").TypeText(user.Id.ToString());
			browser.Button(Find.ByValue("�����")).Click();
			Assert.That(browser.Text, Is.StringContaining(user.Login));
/*
 *			����������� ������������� ���� � ������������
			browser.Link(Find.ByText(user.Login)).Click();
			Assert.That(browser.Text, Is.StringContaining("���������"));
*/
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

		[Test]
		public void Update_supplier_name()
		{
			Open(supplier);
			browser.Css("#supplier_Name").TypeText("�������� ��������� �����������");
			browser.Click("���������");
			Assert.That(browser.Text, Is.StringContaining("���������"));
		}

		[Test]
		public void Add_user()
		{
			Open(supplier);
			browser.Click("����� ������������");
			Assert.That(browser.Text, Is.StringContaining("����� ������������"));
			browser.Click("���������");
			Assert.That(browser.Text, Is.StringContaining("���������"));
		}

		[Test]
		public void Register()
		{
			Open();
			browser.Click("���������");
			Assert.That(browser.Text, Is.StringContaining("����������� ����������"));

			Prepare();

			browser.Click("����������������");
			Assert.That(browser.Text, Is.StringContaining("����������� �����������"));
			browser.Click("���������");
			Assert.That(browser.Text, Is.StringContaining("��������� ��������"));
		}

		[Test]
		public void Register_supplier_show_user_card()
		{
			Open("Register/RegisterSupplier");
			Assert.That(browser.Text, Is.StringContaining("����������� ����������"));
			Prepare();

			browser.Css("#FillBillingInfo").Click();
			browser.Click("����������������");

			Assert.That(browser.Text, Is.StringContaining("��������������� �����"));
		}

		private void Prepare()
		{
			Css("#JuridicalName").TypeText("�������� ���������");
			Css("#ShortName").TypeText("��������");
			Css("#ClientContactPhone").TypeText("473-2606000");
			Css("#ClientContactEmail").TypeText("kvasovtest@analit.net");
		}
	}
}