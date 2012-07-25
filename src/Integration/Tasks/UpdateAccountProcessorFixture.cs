using System.Collections;
using System.Linq;
using AdminInterface.Background;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Scopes;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration.Tasks
{
	[TestFixture]
	public class UpdateAccountProcessorFixture : Test.Support.IntegrationFixture
	{
		Client client;
		User user;
		private Stack savedStack;

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
			session.Refresh(user);
			user.Payer.Refresh();
			Assert.That(user.Accounting.ReadyForAccounting, Is.True);
			Assert.That(user.Payer.PaymentSum, Is.EqualTo(800));
		}

		[Test]
		public void User_not_ready_for_accounting_if_updated_less_than_ten_times()
		{
			MakeUpdates(user, 9);
			Check();
			ActiveRecordMediator.Refresh(user);
			Assert.That(user.Accounting.ReadyForAccounting, Is.False);
		}

		[Test]
		public void All_addresses_ready_for_accounting_user_ready_for_accounting()
		{
			MakeUpdates(user, 10);
			Check();
			var address = user.AvaliableAddresses.First();
			session.Clear();
			address = session.Load<Address>(address.Id);
			Assert.That(address.Accounting.ReadyForAccounting, Is.True, "адрес доставки {0}", address.Id);
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
			ActiveRecordMediator.Save(user);

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
			Flush();
			HideScope();
			try
			{
				new UpdateAccountProcessor().Process();
			}
			finally
			{
				ShowScope();
			}
		}

		private void HideScope()
		{
			savedStack = (Stack)ThreadScopeAccessor.Instance.CurrentStack.Clone();
			ThreadScopeAccessor.Instance.CurrentStack.Clear();
		}

		private void ShowScope()
		{
			var stack = ThreadScopeAccessor.Instance.CurrentStack;
			foreach (var sessionScope in stack.Cast<ISessionScope>().Reverse())
				sessionScope.Dispose();
			foreach (var scope in savedStack.Cast<ISessionScope>())
			{
				stack.Push(scope);
			}
		}
	}
}