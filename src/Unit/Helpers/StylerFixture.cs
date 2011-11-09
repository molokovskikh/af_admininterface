using System;
using System.Linq;
using AdminInterface.Helpers;
using AdminInterface.Models;
using NUnit.Framework;

namespace Unit.Helpers
{
	[TestFixture]
	public class StylerFixture
	{
		private Styler styler;

		public class TestClass
		{
			[Style]
			public virtual bool Disabled { get; set; }
		}

		public class TestChild : TestClass
		{
			public override bool Disabled
			{
				get
				{
					return base.Disabled;
				}
				set
				{
					base.Disabled = value;
				}
			}
		}

		[SetUp]
		public void Setup()
		{
			styler = new Styler();
		}

		[Test]
		public void Get_object_styles()
		{
			var styles = styler.GetStyles(new TestClass { Disabled = true });
			Assert.That(styles.ToArray(), Is.EquivalentTo(new [] {"disabled"}));
		}

		[Test]
		public void Do_not_show_disabled_styles()
		{
			var styles = styler.GetStyles(new TestClass { Disabled = false });
			Assert.That(styles.ToArray(), Is.EquivalentTo(new string[0]));
		}

		[Test]
		public void Convert_style()
		{
			Assert.That(Styler.ToStyle("DisabledByBilling"), Is.EqualTo("disabled-by-billing"));
		}

		[Test]
		public void Stop_words()
		{
			Assert.That(Styler.ToStyle("IsFree"), Is.EqualTo("free"));
		}

		[Test]
		public void Inherit_styles()
		{
			var styles = styler.GetStyles(new TestChild { Disabled = true });
			Assert.That(styles.ToArray(), Is.EquivalentTo(new [] {"disabled"}));
		}
	}
}