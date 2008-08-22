using System;
using System.ComponentModel;
using Common.Web.Ui.Helpers;
using NUnit.Framework;

namespace Common.Web.Ui.Test
{
	public enum TestEnum
	{
		[System.ComponentModel.Description("это тест")]test1
	}

	[TestFixture]
	public class BindingHelperTests
	{
		[Test]
		public void GetDescriptionTest()
		{
			Assert.AreEqual(BindingHelper.GetDescription(TestEnum.test1), "это тест");
		}

		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage="“ип c именем a231 не найден")]
		public void ExceptionIfTypeNotFoundTest()
		{
			BindingHelper.GetDescriptionsDictionary("a231");
		}
	}
}
