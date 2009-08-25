using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models.Logs;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Integration.Models
{
	[TestFixture]
	public class CallLogFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitialzeAR();
			CallLog.DeleteAll();
		}

		[Test]
		public void Select_last_5_unknown_calls()
		{
			new CallLog {Direction = CallDirection.Input, Id2 = IdentificationStatus.Unknow, From = "4722527321"}.Save();
			new CallLog {Direction = CallDirection.Input, Id2 = IdentificationStatus.Unknow, From = "4732343644"}.Save();
			new CallLog {Direction = CallDirection.Output, Id2 = IdentificationStatus.Unknow, From = "141@analit.net"}.Save();
			using(new SessionScope())
				Assert.That(CallLog.LastCalls(), Is.EqualTo(new[] {"4722527321", "4732343644"}));
		}
	}
}
