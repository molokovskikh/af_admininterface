using System;
using AdminInterface.Helpers;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Helpers
{
	[TestFixture]
	public class ADHelperFixture
	{
		[Test]
		public void IsLoginExistsTest()
		{
			Assert.That(ADHelper.IsLoginExists("kvasov"), Is.True);
			Assert.That(ADHelper.IsLoginExists("loasdvhuq34y89rhawf"), Is.False);
		}

		[Test]
		public void GetPasswordExpirationDate()
		{
			Assert.That(ADHelper.GetPasswordExpirationDate("kvasov"), Is.GreaterThan(DateTime.Now));
		}

		[Test]
		public void IsLockedTest()
		{
			Assert.That(ADHelper.IsLocked("kvasov"), Is.False);
		}

		[Test]
		public void IsDisabledTest()
		{
			Assert.That(ADHelper.IsDisabled("kvasov"), Is.False);
		}
	}
}
