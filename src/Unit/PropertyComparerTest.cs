using System.Collections.Generic;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using NUnit.Framework;


namespace AdminInterface.Test
{
	public class TestEntity
	{
		private int _i;
		private string _s;

		public int I
		{
			get { return _i; }
		}

		public string S
		{
			get { return _s; }
		}

		public TestEntity(int i, string s)
		{
			_i = i;
			_s = s;
		}
	}

	[TestFixture]
	public class PropertyComparerTest
	{
		[Test]
		public void AscendingSortTest()
		{
			List<TestEntity> data = GetData();
			data.Sort(new PropertyComparer<TestEntity>(SortDirection.Ascending, "I"));
			Assert.AreEqual(10, data[0].I);
			Assert.AreEqual(10, data[1].I);
			Assert.AreEqual(50, data[2].I);
			Assert.AreEqual(100, data[3].I);
		}

		[Test]
		public void DescendingSortTest()
		{
			List<TestEntity> data = GetData();
			data.Sort(new PropertyComparer<TestEntity>(SortDirection.Descending, "I"));
			Assert.That(data[0].I, Is.EqualTo(100));
			Assert.That(data[1].I, Is.EqualTo(50));
			Assert.That(data[2].I, Is.EqualTo(10));
			Assert.That(data[3].I, Is.EqualTo(10));
		}

		[Test]
		public void TextSortTest()
		{
			List<TestEntity> data = GetData();
			data.Sort(new PropertyComparer<TestEntity>(SortDirection.Descending, "I"));
			Assert.AreEqual("‡‡‡‡‡‡‡‡‡‡‡", data[0].S);
			Assert.AreEqual("‡‚Ò", data[1].S);
			Assert.AreEqual("‡", data[2].S);
			Assert.AreEqual("ÒÒ", data[3].S);
		}

		private List<TestEntity> GetData()
		{
			List<TestEntity> result = new List<TestEntity>();
			result.Add(new TestEntity(50, "‡‚Ò"));
			result.Add(new TestEntity(10, "ÒÒ"));
			result.Add(new TestEntity(100, "‡‡‡‡‡‡‡‡‡‡‡"));
			result.Add(new TestEntity(10, "‡"));
			return result;
		}
	}
}
