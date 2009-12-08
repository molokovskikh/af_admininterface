using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using AdminInterface.Test.ForTesting;
using WatiN.Core;

namespace Functional
{
	[TestFixture]
	public class ClientRegistrationFixture : WatinFixture
	{
		[Test]
		public void SetExistsBilingTest()
		{
			using(var browser = Open("main/index"))
			{
				browser.Link(Find.ByText("Регистрация клиентов")).Click();
				browser.CheckBox("PayerPresentCB").Checked = true;
				browser.TextField("PayerFTB").TypeText("офис");
				browser.Button("FindPayerB").Click();

				Assert.AreEqual("Регистрация пользователей", browser.Title);
			}
		}
	}
}
