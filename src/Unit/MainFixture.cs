using System;
using AdminInterface.Controllers;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class MainFixture
	{
		[Test]
		public void CheckFormalizeTimeTest()
		{
			var nowDate = DateTime.Now.Date;
			Assert.False(MainController.CheckFormalizeTime(nowDate.AddHours(4), nowDate.AddHours(4).AddMinutes(15).AddSeconds(1)));
			Assert.False(MainController.CheckFormalizeTime(nowDate.AddHours(6), nowDate.AddHours(6).AddMinutes(15)));
			Assert.True(MainController.CheckFormalizeTime(nowDate.AddHours(6), nowDate.AddHours(6).AddMinutes(15).AddSeconds(1)));
		}
	}
}
