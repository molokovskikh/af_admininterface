using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AdminInterface.Helpers;
using System.Web.UI.WebControls;

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
			Assert.AreEqual(100, data[0].I);
			Assert.AreEqual(50, data[1].I);
			Assert.AreEqual(10, data[2].I);
			Assert.AreEqual(10, data[3].I);
		}

		[Test]
		public void TextSortTest()
		{
			List<TestEntity> data = GetData();
			data.Sort(new PropertyComparer<TestEntity>(SortDirection.Descending, "I"));
			Assert.AreEqual("ààààààààààà", data[0].S);
			Assert.AreEqual("àâñ", data[1].S);
			Assert.AreEqual("ğà", data[2].S);
			Assert.AreEqual("ğññ", data[3].S);
		}

		private List<TestEntity> GetData()
		{
			List<TestEntity> result = new List<TestEntity>();
			result.Add(new TestEntity(50, "àâñ"));
			result.Add(new TestEntity(10, "ğññ"));
			result.Add(new TestEntity(100, "ààààààààààà"));
			result.Add(new TestEntity(10, "ğà"));
			return result;
		}
	}
}
