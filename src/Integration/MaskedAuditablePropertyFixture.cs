using AdminInterface.Models;
using Common.Web.Ui.Helpers;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class MaskedAuditablePropertyFixture
	{
		public class Test
		{
			public ulong MaskRegion { get; set; }
		}

		[Test]
		public void Build_region()
		{
			var property = new MaskedAuditableProperty(typeof(Test).GetProperty("MaskRegion"), "Регион", 4ul, 5ul);
			Assert.That(property.ToString(), Is.EqualTo("$$$Изменено 'Регион' Удалено 'Воронеж'"));
		}

		[Test]
		public void Change_region()
		{
			var property = new MaskedAuditableProperty(typeof(Test).GetProperty("MaskRegion"), "Регион", 1UL, 16UL);
			Assert.That(property.ToString(), Is.EqualTo("$$$Изменено 'Регион' Удалено 'Тамбов' Добавлено 'Воронеж'"));
		}

		[Test]
		public void Ignore_unknown_region()
		{
			var property = new MaskedAuditableProperty(typeof(Test).GetProperty("MaskRegion"), "Регион", 0UL, 18446742976345407488UL);
			Assert.That(property.ToString(), Is.EqualTo("$$$Изменено 'Регион' Удалено 'Ижевск', 'Справка-Воронеж', 'Справка-Белкород', 'Справка-Курск'")
				.Or.EqualTo("$$$Изменено 'Регион' Удалено 'Ижевск'"));
		}
	}
}