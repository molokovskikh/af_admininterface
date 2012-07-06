using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using Integration.ForTesting;
using NUnit.Framework;

namespace Integration
{
	[TestFixture]
	public class ClientFixture : Test.Support.IntegrationFixture
	{
		public Client client;
		public Supplier supplier;
		public User user;

		[SetUp]
		public void SetUp()
		{
			supplier = DataMother.CreateSupplier();
			Save(supplier);

			client = DataMother.CreateClientAndUsers();
			client.Save();
			scope.Flush();
			user = client.Users.First();

			Assert.That(GetReplication(user.Id).Count, Is.EqualTo(0));

			session
				.CreateSQLQuery("insert into Usersettings.AnalitfReplicationInfo(UserId, FirmCode, ForceReplication) values (:UserId, :SupplierId, 0)")
				.SetParameter("UserId", user.Id)
				.SetParameter("SupplierId", supplier.Id)
				.ExecuteUpdate();
		}

		[Test]
		public void Add_user_region_force_replication()
		{
			user.WorkRegionMask = ulong.MaxValue;
			session.Save(user);
			scope.Flush();
			var info = GetReplication(user.Id);

			Assert.That(info.Count, Is.GreaterThan(0));
			Assert.That(info, Is.EqualTo(new [] {true}));
		}

		[Test]
		public void Add_client_region_force_replication()
		{
			client.MaskRegion = ulong.MaxValue;
			session.Save(client);
			scope.Flush();
			var info = GetReplication(user.Id);

			Assert.That(info.Count, Is.GreaterThan(0));
			Assert.That(info, Is.EqualTo(new [] {true}));
		}

		[Test]
		public void Add_supplier_region_force_replication()
		{
			supplier.RegionMask = ulong.MaxValue;
			session.Save(supplier);
			scope.Flush();
			var info =  session.CreateSQLQuery("select ForceReplication from Usersettings.AnalitfReplicationInfo where FirmCode = :SupplierId")
				.SetParameter("SupplierId", supplier.Id)
				.List<object>()
				.Select(v => Convert.ToBoolean(v))
				.ToList();

			Assert.That(info.Count, Is.GreaterThan(0));
			Assert.That(info, Is.EqualTo(new [] {true}));
		}

		private IList<bool> GetReplication(uint userId)
		{
			return session.CreateSQLQuery("select ForceReplication from Usersettings.AnalitfReplicationInfo where UserId = :UserId")
				.SetParameter("UserId", userId)
				.List<object>()
				.Select(v => Convert.ToBoolean(v))
				.ToList();
		}
	}
}
