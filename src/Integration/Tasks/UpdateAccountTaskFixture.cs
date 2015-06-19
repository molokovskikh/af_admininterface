using System;
using System.Collections;
using System.Linq;
using AdminInterface.Background;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Scopes;
using Integration.ForTesting;
using NUnit.Framework;
using Test.Support.log4net;

namespace Integration.Tasks
{
	[TestFixture]
	public class UpdateAccountTaskFixture : AdmIntegrationFixture
	{
		private Client client;
		private User user;
		private Stack savedStack;

		[SetUp]
		public void Seup()
		{
			client = DataMother.CreateTestClientWithAddressAndUser();
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
			session.Refresh(user.Payer);
			Assert.That(user.Accounting.ReadyForAccounting, Is.True);
			Assert.That(user.Payer.PaymentSum, Is.EqualTo(800));
		}

		[Test]
		public void User_not_ready_for_accounting_if_updated_less_than_ten_times()
		{
			MakeUpdates(user, 9);
			Check();
			session.Refresh(user);
			Assert.That(user.Accounting.ReadyForAccounting, Is.False);
		}

		[Test]
		public void All_addresses_ready_for_accounting_user_ready_for_accounting()
		{
			MakeUpdates(user, 11);
			Check();
			var address = user.AvaliableAddresses.First();
			session.Refresh(address);
			Assert.That(address.Accounting.ReadyForAccounting, Is.True, "адрес доставки {0}", address.Id);
		}

		[Test]
		public void ReadyForAccountingIfProcessWithLittlePage()
		{
			MakeUpdates(user, 11);
			Check(1);
			var address = user.AvaliableAddresses.First();
			session.Refresh(address);
			Assert.That(address.Accounting.ReadyForAccounting, Is.True, "адрес доставки {0}", address.Id);
		}

		[Test]
		public void Do_check_for_newly_join_addresses()
		{
			var address = user.Client.AddAddress("Тестовый адрес доставки");
			session.Save(address);
			MakeUpdates(user, 10);
			Check();
			session.Refresh(address);
			Assert.That(address.Accounting.ReadyForAccounting, Is.False);

			session.Refresh(address);
			user.AvaliableAddresses.Add(address);
			session.SaveOrUpdate(user);

			Check();
			session.Refresh(address);
			Assert.That(address.Accounting.ReadyForAccounting, Is.True);
		}

		private void MakeUpdates(User user, int count)
		{
			for (var i = 0; i < count; i++)
				Save(new UpdateLogEntity(user) { Commit = true });
		}

		private void Check()
		{
			Flush();
			HideScope();
			try {
				new UpdateAccountTask().Execute();
			}
			finally {
				ShowScope();
			}
		}

		private void Check(int pageSize)
		{
			Flush();
			HideScope();
			try {
				new UpdateAccountTask {
					PageSize = pageSize
				}.Execute();
			}
			finally {
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
			foreach (var scope in savedStack.Cast<ISessionScope>()) {
				stack.Push(scope);
			}
		}
	}
}