using AdminInterface.Models.Logs;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test
{
	[TestFixture]
	public class InternetLogEntityFixture
	{
		[Test]
		public void GetBeginByteNubmerTest()
		{
			var entity = new InternetLogEntity
			             	{
			             		Parameters = "Id=128293788612187500True&RangeStart=1319424"
			             	};
			Assert.That(entity.GetBeginByteNubmer(), Is.EqualTo(1319424));
			entity.Parameters = "Id=128293788612187500True";
			Assert.That(entity.GetBeginByteNubmer(), Is.EqualTo(0));
		}
	}
}
