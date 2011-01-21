using System;
using System.Globalization;
using System.Resources;
using Castle.Components.Validator;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class LocalizationFixture
	{
		[Test]
		public void Test()
		{
			var defaultResourceManager = new ResourceManager("Castle.Components.Validator.Messages", typeof(CachedValidationRegistry).Assembly);
			Console.WriteLine(defaultResourceManager.GetString("email", CultureInfo.GetCultureInfo("ru")));
		}
	}
}