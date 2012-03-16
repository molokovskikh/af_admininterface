using System.Linq;
using AdminInterface.Background;
using AdminInterface.Models.Billing;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;

namespace Integration.Tasks
{
	[TestFixture]
	public class ReportProcessorFixture : IntegrationFixture
	{
		ReportProcessor processor;
		Payer payer;

		[SetUp]
		public void Setup()
		{
			processor = new ReportProcessor();
			payer = DataMother.CreatePayer();

			Save(payer);
		}

		[Test]
		public void Create_account_for_new_reports()
		{
			var report = new Report { Payer = payer };
			Save(report);

			processor.Process();

			var account = session.Query<ReportAccount>().FirstOrDefault(r => r.Report == report);
			Assert.That(account, Is.Not.Null);
			Assert.That(account.ReadyForAccounting, Is.True);
		}

		[Test]
		public void Delete_account_after_delete_report()
		{
			var account = DataMother.Report(payer);
			Save(account);
			Assert.That(account.Id, Is.Not.EqualTo(0u));
			Assert.That(account.Report.Id, Is.Not.EqualTo(0u));
			session.Clear();
			session.Delete(account.Report);
			session.Flush();

			processor.Process();

			session.Clear();
			account = session.Get<ReportAccount>(account.Id);
			Assert.That(account, Is.Null);
		}
	}
}