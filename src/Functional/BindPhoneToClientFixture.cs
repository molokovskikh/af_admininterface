using System;
using System.Linq;
using AdminInterface.Models.Logs;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using WatiN.Core;
using AdminInterface.Models.Telephony;
using Functional.ForTesting;
using Common.Web.Ui.Helpers;
using System.Threading;
using Castle.ActiveRecord;
using Common.Tools;

namespace Functional
{
	[TestFixture]
	public class BindPhoneToClientFixture : WatinFixture
	{
		[SetUp]
		public void SetUp()
		{
			using (var scope = new TransactionScope())
			{
				var phoneNumbers = new[] { "4732299222", "4732605000", "4732299223", "4732299224", "4732606000" };

				foreach (var phone in phoneNumbers)
				{
					ArHelper.WithSession(s => s.CreateSQLQuery(@"
delete from telephony.UnresolvedPhone where Phone like :phone")
							.SetParameter("phone", phone)
							.ExecuteUpdate());
					new UnresolvedCall { PhoneNumber = phone }.SaveAndFlush();
				}
				scope.VoteCommit();
			}
		}

		[Test]
		public void Client_page_should_contains_not_detected_phones()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = new IE(BuildTestUrl(String.Format("client/{0}", client.Id))))
			{
				Assert.That(browser.Text, Text.Contains("Неопознанные звонки"));
				Assert.That(GetUnknownPhones(browser), Is.EqualTo(new[] { "4732606000", "4732299224", "4732299223", "4732605000", "4732299222" }));
			}
		}

		[Test, Ignore("Устарел, использует таблицу CallLogs")]
		public void On_bind_click_call_should_be_marked_as_known_and_moved_to_known_phone_contact_group()
		{
			using (var browser = new IE(BuildTestUrl("client/2575")))
			{
				var row = browser.Table(Find.ById("UnknownPhones")).TableRows.First();
				row.Button(Find.ByValue("Связать")).Click();

				Assert.That(GetUnknownPhones(browser), Is.EqualTo(new [] { "4732299222" }));

				Assert.That(browser.Text, Text.Contains("Известные телефоны"));
				Assert.That(browser.Text, Text.Contains("4732-606000"));
			}
		}

		[Test]
		public void On_bind_click_call_must_move_to_known_phone_contact_group()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = new IE(BuildTestUrl(String.Format("client/{0}", client.Id))))
			{
				var row = browser.Table(Find.ById("UnknownPhones")).TableRows.First();
				row.Button(Find.ByValue("Связать")).Click();

				Assert.That(GetUnknownPhones(browser).Contains("4732299222"));
				Assert.That(GetUnknownPhones(browser).Contains("4732605000"));
				Assert.That(GetUnknownPhones(browser).Contains("4732299223"));
				Assert.That(GetUnknownPhones(browser).Contains("4732299224"));

				Assert.That(browser.Text, Text.Contains("Известные телефоны"));
				Assert.That(browser.Text, Text.Contains("4732-606000"));
			}
		}

		[Test]
		public void After_bind_phone_must_be_deleted_from_unresolved()
		{
			var client = DataMother.CreateTestClientWithUser();
			using (var browser = new IE(BuildTestUrl(String.Format("client/{0}", client.Id))))
			{
				var row = browser.Table(Find.ById("UnknownPhones")).TableRows.First();
				row.Button(Find.ByValue("Связать")).Click();

				var count = Convert.ToUInt32(ArHelper.WithSession(s => s.CreateSQLQuery(@"
select count(*) from telephony.UnresolvedPhone where Phone like :phone")
							.SetParameter("phone", "4732606000")
							.UniqueResult()));
				Assert.That(count, Is.EqualTo(0));
			}
		}

		private string[] GetUnknownPhones(IE browser)
		{
			return MapColumn(browser.Table(Find.ById("UnknownPhones")), 1);
		}

		public static string[] MapColumn(Table table, int columnIndex)
		{
			return table.TableRows.Select(r => r.TableCells.Skip(columnIndex).First().Text).ToArray();
		}
	}
}