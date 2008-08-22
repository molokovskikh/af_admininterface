using System;
using System.ComponentModel;
using Common.Web.Ui.Helpers;
using NUnit.Framework;

namespace Common.Web.Ui.Test
{
	public enum TestEnum
	{
		[System.ComponentModel.Description("��� ����")]test1
	}

	[TestFixture]
	public class BindingHelperTests
	{
		[Test]
		public void GetDescriptionTest()
		{
			Assert.AreEqual(BindingHelper.GetDescription(TestEnum.test1), "��� ����");
		}

		[Test]
		[ExpectedException(typeof(Exception), ExpectedMessage="��� c ������ a231 �� ������")]
		public void ExceptionIfTypeNotFoundTest()
		{
			BindingHelper.GetDescriptionsDictionary("a231");
		}
	}
}
