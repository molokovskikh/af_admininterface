using System;
using System.Collections.Generic;
using System.Text;
using AdminInterface.Helpers;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Helpers
{
	[TestFixture]
	public class ViewHelperFixture
	{
		[Test]
		public void ConverToUserFriendlySizeTest()
		{
			Assert.That(ViewHelper.ConvertToUserFriendlySize(0), Is.EqualTo("-"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(10), Is.EqualTo("10 ม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(1024), Is.EqualTo("1 สม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(80543), Is.EqualTo("78,66 สม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(1024 * 1024), Is.EqualTo("1 ฬม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(98738544), Is.EqualTo("94,16 ฬม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(1024 * 1024 * 1024), Is.EqualTo("1 รม"));
			Assert.That(ViewHelper.ConvertToUserFriendlySize(91660739604), Is.EqualTo("85,37 รม"));
		}
	}
}
