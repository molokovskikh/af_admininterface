using Functional.ForTesting;
using Test.Support.Web;
using NUnit.Framework;

namespace Functional
{
	public class NewsFixture : WatinFixture2
	{
		[Test]
		public void View_news()
		{
			Open();
			AssertText("Новости");
			Click("Новости");
			AssertText("Новости");
			Click("Добавить");
			AssertText("Дата публикации");
			AssertText("Адресат");
			Css("#news_Header").Value = "Тестовая новость";
			Click("Сохранить");
			AssertText("Сохранено");
			AssertText("Тестовая новость");
			Click("Скрыть");
			Assert.IsNotNull(browser.Css(".DataTable tbody tr.hidden-news"));
			Click("Восстановить");
			Assert.IsNull(browser.Css(".DataTable tbody tr.hidden-news"));
		}
	}
}