using System.Linq;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Web.Ui.NHibernateExtentions;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using Test.Support;

namespace Integration.Models
{
	[TestFixture]
	public class CallLogFixture : AdmIntegrationFixture
	{
		[SetUp]
		public void Setup()
		{
			session.DeleteEach(session.Query<CallLog>());
		}

		[Test]
		public void Select_last_5_unknown_calls()
		{
			session.Save(new CallLog { Direction = CallDirection.Input, Id2 = IdentificationStatus.Unknow, From = "4722527321" });
			session.Save(new CallLog { Direction = CallDirection.Input, Id2 = IdentificationStatus.Unknow, From = "4732343644" });
			session.Save(new CallLog { Direction = CallDirection.Output, Id2 = IdentificationStatus.Unknow, From = "141@analit.net" });
			Assert.That(CallLog.LastCalls(), Is.EquivalentTo(new[] { "4722527321", "4732343644" }));
		}
	}
}