using AdminInterface.Test.ForTesting;
using NUnit.Framework;

namespace Functional
{
	public class ViewclFixture : WatinFixture
	{
		[Test]
		public void View_update_stat()
		{
			using (var browser = Open("viewcl.aspx?BeginDate=23.11.2009%200:00:00&EndDate=24.11.2009%200:00:00&RegionMask=18446744073709551615&id=1"))
			{
				Assert.That(browser.Text, Is.StringContaining("Статистика"));
			}
		}
	}
}
