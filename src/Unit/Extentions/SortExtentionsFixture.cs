using System.Collections.Generic;
using System.Linq;
using AdminInterface.Helpers;
using NUnit.Framework;

namespace Unit.Extentions
{
	[TestFixture]
	public class SortExtentionsFixture
	{
		public class ForSortTest
		{
			public int i { get; set; }
		}

		[Test]
		public void Sort_by_property()
		{
			var sort = "i";
			var direction = "";
			var sorted = new List<ForSortTest> {
				new ForSortTest {i = 1},
				new ForSortTest {i = 3},
				new ForSortTest {i = 2},
			}
				.Sort(ref sort, ref direction, "i")
				.ToList();
			Assert.That(sorted[0].i, Is.EqualTo(1));
			Assert.That(sorted[1].i, Is.EqualTo(2));
			Assert.That(sorted[2].i, Is.EqualTo(3));
		}
		
	}
}