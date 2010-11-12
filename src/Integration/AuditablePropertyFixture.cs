using AdminInterface.Models;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class AuditablePropertyFixture
	{
		public class Test
		{
			public ulong MaskRegion { get; set; }
		}

		[Test]
		public void Build_region()
		{
			var property = new AuditableProperty(typeof(Test).GetProperty("MaskRegion"), "Регион", 4ul, 5ul);
			Assert.That(property.ToString(), Is.EqualTo("$$$Изменено 'Регион' Удалено 'Воронеж'"));
		}
	}
}