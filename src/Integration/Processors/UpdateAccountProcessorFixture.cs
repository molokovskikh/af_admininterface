using System.Linq;
using AdminInterface.Background;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Processors
{
	[TestFixture]
	public class UpdateAccountProcessorFixture : IntegrationFixture
	{
		Client client;
		User user;

		[SetUp]
		public void Seup()
		{
			client =  DataMother.CreateTestClientWithAddressAndUser();
			user = client.Users.First();
			user.AvaliableAddresses.Add(client.Addresses.First());
			Flush();
		}

		[Test]
		public void User_ready_for_accounting_if_updated_ten_times()
		{
			MakeUpdates(user, 11);
			Check();
			user.Refresh();
			Assert.That(user.Accounting.ReadyForAccounting, Is.True);
			Assert.That(user.Payer.PaymentSum, Is.EqualTo(800));
		}

		[Test]
		public void User_not_ready_for_accounting_if_updated_less_than_ten_times()
		{
			MakeUpdates(user, 9);
			Check();
			user.Refresh();
			Assert.That(user.Accounting.ReadyForAccounting, Is.False);
		}

		[Test]
		public void All_addresses_ready_for_accounting_user_ready_for_accounting()
		{
			MakeUpdates(user, 10);
			Check();
			var address = user.AvaliableAddresses.First();
			address.Refresh();
			Assert.That(address.Accounting.ReadyForAccounting, Is.True);
		}

		[Test]
		public void Do_check_for_newly_join_addresses()
		{
			var address = user.Client.AddAddress("Тестовый адрес доставки");
			address.Save();
			MakeUpdates(user, 10);
			Check();
			address.Refresh();
			Assert.That(address.Accounting.ReadyForAccounting, Is.False);

			address.Refresh();
			user.AvaliableAddresses.Add(address);
			user.Save();

			Check();
			address.Refresh();
			Assert.That(address.Accounting.ReadyForAccounting, Is.True);
		}

		private void MakeUpdates(User user, int count)
		{
			for (var i = 0; i < count; i++)
				Save(new UpdateLogEntity(user) {Commit = true});

		}

		private void Check()
		{
			new UpdateAccountProcessor().Process();
		}
	}
}