using System.Linq;
using AdminInterface.Models.Logs;
using AdminInterface.Test.ForTesting;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using WatiN.Core;

namespace AdminInterface.Test.Watin
{
	[TestFixture]
	public class BindPhoneToClientFixture : WatinFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitialzeAR();
			CallLog.DeleteAll();

			new CallLog { Direction = 0, Id2 = IdentificationStatus.Unknow, From = "4732606000" }.Save();
			new CallLog { Direction = 0, Id2 = IdentificationStatus.Unknow, From = "4732299222" }.Save();
		}

		[Test]
		public void Client_page_should_contains_not_detected_phones()
		{
			using (var browser = new IE(BuildTestUrl("client/2575")))
			{
				Assert.That(browser.Text, Text.Contains("Не опознанные звонки"));
				Assert.That(GetUnknownPhones(browser), Is.EqualTo(new [] { "4732606000", "4732299222" }));
			}
		}

		[Test]
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
