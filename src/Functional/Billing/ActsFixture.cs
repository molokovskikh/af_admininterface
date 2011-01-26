using System;
using AdminInterface.Models.Billing;
using Functional.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional.Billing
{
	[TestFixture]
	public class ActsFixture : WatinFixture
	{
		[Test]
		public void View_acts()
		{
			using (var browser = Open("/"))
			{
				Assert.That(browser.Text, Is.StringContaining("���������������� ���������"));
				browser.Link(Find.ByText("����")).Click();
				Assert.That(browser.Text, Is.StringContaining("������������ ����"));
			}
		}

		[Test]
		public void View_act_print_form()
		{
			Act act = null;
			using (var browser = Open("/acts/"))
			{
				browser.Link(Find.ByText(String.Format("{0}", act.Id))).Click();
				Assert.That(browser.Text, Is.StringContaining("��� �����-�������"));
			}
		}
	}
}