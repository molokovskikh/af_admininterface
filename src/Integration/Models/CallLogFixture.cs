using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class CallLogFixture
	{
		[SetUp]
		public void Setup()
		{
			CallLog.DeleteAll();
		}

		[Test]
		public void Select_last_5_unknown_calls()
		{
			new CallLog {Direction = CallDirection.Input, Id2 = IdentificationStatus.Unknow, From = "4722527321"}.Save();
			new CallLog {Direction = CallDirection.Input, Id2 = IdentificationStatus.Unknow, From = "4732343644"}.Save();
			new CallLog {Direction = CallDirection.Output, Id2 = IdentificationStatus.Unknow, From = "141@analit.net"}.Save();
			using(new SessionScope())
				Assert.That(CallLog.LastCalls(), Is.EquivalentTo(new[] {"4722527321", "4732343644"}));
		}
	}
}
