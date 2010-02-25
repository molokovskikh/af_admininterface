using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class ViewAdministratorsFixture : WatinFixture
	{
		[Test]
		public void ViewPage()
		{
			using (var browser = Open("Index.brail"))
			{
//				browser.Link(Find.ByText("Региональные администраторы")).Click();
//				Assert.That(browser.Text, Is.StringContaining("Пользователи офиса"));
//				Assert.That(browser.Uri, Is.StringContaining("ViewAdministrators.aspx"));
			}
		}

		[Test]
		public void SetupAdministrator()
		{
			using (var browser = Open("ViewAdministrators.aspx"))
			{
//				Assert.That(browser.Text, Is.StringContaining("Пользователи офиса"));
			}
		}
	}
}
