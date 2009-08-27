using System.Collections.Generic;
using System.Linq;
using AdminInterface.Helpers;
using NUnit.Framework;


namespace AdminInterface.Test.Helpers
{
	[TestFixture]
	public class ViewHelperFixture
	{
		[Test]
		public void ConverToUserFriendlySizeTest()
		{
			Assert.That(ViewHelper.ConvertToUserFriendlySize(0), Is.EqualTo("-"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(10), Is.EqualTo("10 ม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(1024), Is.EqualTo("1 สม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(80543), Is.EqualTo("78,66 สม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(1024 * 1024), Is.EqualTo("1 ฬม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(98738544), Is.EqualTo("94,16 ฬม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(1024 * 1024 * 1024), Is.EqualTo("1 รม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(91660739604), Is.EqualTo("85,37 รม"));
		}

		public class ForSortTest
		{
			public int i { get; set; }
		}

		[Test]
		public void Sort_by_property()
		{
			var sort = "i";
			var direction = "";
			var sorted = new List<ForSortTest>
			           	{
			           		new ForSortTest {i = 1},
			           		new ForSortTest {i = 3},
			           		new ForSortTest {i = 2},
			           	}.Sort(ref sort, ref direction, "i")
						.ToList();
			Assert.That(sorted[0].i, Is.EqualTo(1));
			Assert.That(sorted[1].i, Is.EqualTo(2));
			Assert.That(sorted[2].i, Is.EqualTo(3));
		}
	}
}
