﻿using System;
using Common.Web.Ui.Helpers;
using AdminInterface.Extentions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Extentions
{
	[TestFixture]
	public class EnumExtentionsFixtre
	{
		public enum TestEnum
		{
			[System.ComponentModel.Description("описание")] v1,
		}

		[Test]
		public void Desciption()
		{
			var v = TestEnum.v1;
			Assert.That(v.Description(), Is.EqualTo("описание"));
		}
	}
}