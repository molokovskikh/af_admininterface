using AdminInterface.Controllers;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class SortFixture
	{
		[Test]
		public void Read_direction()
		{
			var sort = new Sort("Messages.WriteTime", "asc");
			Assert.That(sort.Property, Is.EqualTo("Messages.WriteTime"));
			Assert.That(sort.Direction, Is.EqualTo("asc"));
		}
	}
}