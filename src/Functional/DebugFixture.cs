using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Support.Web;

namespace Functional
{
	[TestFixture]
	public class DebugFixture : WatinFixture2
	{
		[Test]
		public void Test_lock_time_out_test()
		{
			CheckStatusCode = false;
			Open("Error/TestLockTimeOut?error_fixture=true");
			AssertText("Ошибка при выполнении операции");
		}
	}
}
