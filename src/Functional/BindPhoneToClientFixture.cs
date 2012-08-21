using System.Linq;
using AdminInterface.Models;
using Integration.ForTesting;
using NHibernate.Linq;
using NUnit.Framework;
using WatiN.Core;
using Test.Support.Web;
using AdminInterface.Models.Telephony;

namespace Functional
{
	[TestFixture]
	public class BindPhoneToClientFixture : WatinFixture2
	{
		private Client client;

		[SetUp]
		public void SetUp()
		{
			var phones = new[] { "4732299222", "4732605000", "4732299223", "4732299224", "4732606000" };

			session.CreateSQLQuery("delete from telephony.UnresolvedPhone where Phone in (:phones)")
				.SetParameterList("phones", phones)
				.ExecuteUpdate();

			foreach (var phone in phones)
				Save(new UnresolvedCall(phone));

			client = DataMother.CreateTestClientWithUser();
			Open(client.Users[0]);
		}

		[Test]
		public void Client_page_should_contains_not_detected_phones()
		{
			Assert.That(browser.Text, Is.StringContaining("Неопознанные звонки"));
			Assert.That(GetUnknownPhones(browser), Is.EqualTo(new[] { "4732606000", "4732299224", "4732299223", "4732605000", "4732299222" }));
		}

		[Test]
		public void On_bind_click_call_must_move_to_known_phone_contact_group()
		{
			var row = browser.Table(Find.ById("UnknownPhones")).TableRows.First();
			row.Button(Find.ByValue("Связать")).Click();

			Assert.That(GetUnknownPhones(browser).Contains("4732299222"));
			Assert.That(GetUnknownPhones(browser).Contains("4732605000"));
			Assert.That(GetUnknownPhones(browser).Contains("4732299223"));
			Assert.That(GetUnknownPhones(browser).Contains("4732299224"));

			Open(client);

			Assert.That(browser.Text, Is.StringContaining("Известные телефоны"));
			Assert.That(browser.Text, Is.StringContaining("4732-606000"));

			var count = session.Query<UnresolvedCall>().Count(c => c.PhoneNumber == "4732606000");
			Assert.That(count, Is.EqualTo(0));
		}

		private string[] GetUnknownPhones(IElementContainer browser)
		{
			return MapColumn(browser.Table(Find.ById("UnknownPhones")), 1);
		}

		public static string[] MapColumn(Table table, int columnIndex)
		{
			return table.TableRows.Select(r => r.TableCells.Skip(columnIndex).First().Text).ToArray();
		}
	}
}