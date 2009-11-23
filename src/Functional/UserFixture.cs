using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using WatiN.Core;

namespace Functional
{
	public class UserFixture : WatinFixture
	{
		[Test]
		public void View_password_change_stat()
		{
			using (var browser = Open("client/3616"))
			{
				browser.Link(Find.ByText("Статистика изменения пароля")).Click();
				using (var stat = IE.AttachToIE(Find.ByTitle("Статистика изменения пароля для пользователя KvasovT")))
				{
					Assert.That(stat.Text, Is.StringContaining("Статистика изменения пароля для пользователя KvasovT"));
				}
			}
		}

		[Test]
		public void Edit_user()
		{
			using (var browser = Open("client/2575"))
			{
				browser.Link(Find.ByText("kvasov")).Click();
				Assert.That(browser.Text, Is.StringContaining("kvasov"));
			}
		}
	}
}
