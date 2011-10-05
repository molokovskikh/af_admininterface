using System.Configuration;
using Castle.Components.Binder;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class DataBinderFixture
	{
		public class TestSettings
		{
			public int IntValue { get; set; }
		}

		[Test]
		public void Bind_from_app_settings()
		{
			var builder = new TreeBuilder();
			var tree = builder.BuildSourceNode(ConfigurationManager.AppSettings);
			var binder = new DataBinder();
			var settings = new TestSettings();
			binder.BindObjectInstance(settings, tree);
			Assert.That(settings.IntValue, Is.EqualTo(100));
			Assert.That(binder.ErrorList.Count, Is.EqualTo(0));
		}
	}
}