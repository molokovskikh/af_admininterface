using DiffMatchPatch;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class DiffFixture
	{
		[Test]
		public void t()
		{
			var diff = new diff_match_patch();
			var d = diff.diff_main("test\r\n123\r\ntest", "test\r\ntest");
		}
	}
}