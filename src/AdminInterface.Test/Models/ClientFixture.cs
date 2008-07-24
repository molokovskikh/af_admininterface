using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Models;
using AdminInterface.Test.ForTesting;
using Castle.ActiveRecord;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test.Models
{
	[TestFixture]
	public class ClientFixture
	{
		[SetUp]
		public void Setup()
		{
			ForTest.InitialzeAR();
		}

		[Test]
		public void For_suppliers_get_users_must_include_users_from_show_clients()
		{
			var client
				= new Client
				  	{
				  		Users = new List<User>
				  		        	{
				  		        		new User { Login = "test" },
				  		        	},
				  		ShowClients = new List<ShowRelationship>
				  		              	{
				  		              		new ShowRelationship
				  		              			{
				  		              				Parent = new Client
				  		              				         	{
				  		              				         		Users = new List<User>
				  		              				         		        	{
				  		              				         		        		new User { Login = "test1"}
				  		              				         		        	}
				  		              				         	}
				  		              			}
				  		              	},
				  		Type = ClientType.Supplier
				  	};
			Assert.That(client.GetUsers(), Is.EquivalentTo(new[] {client.Users[0], client.ShowClients[0].Parent.Users[0]}));
		}

		[Test]
		public void For_drug_store_get_users_must_return_only_users_from_os_user_access_right()
		{
			var client
				= new Client
				  	{
				  		Users = new List<User>
				  		        	{
				  		        		new User { Login = "test"},
				  		        	},
				  		ShowClients = new List<ShowRelationship>
				  		              	{
				  		              		new ShowRelationship
				  		              			{
				  		              				Parent = new Client
				  		              				         	{
				  		              				         		Users = new List<User>
				  		              				         		        	{
				  		              				         		        		new User { Login = "test1"}
				  		              				         		        	}
				  		              				         	}
				  		              			}
				  		              	},
				  		Type = ClientType.Drugstore
				  	};
			Assert.That(client.GetUsers(), Is.EquivalentTo( new [] {  client.Users[0] }));
		}

		[Test]
		public void GetSubordinateTypeTest()
		{
			var client = new Client {Id = 1, ShortName = "Test1", Parents = new List<Relationship>()};
			Assert.That(client.GetSubordinateType(), Is.EqualTo("-"));
			Assert.That(client.GetNameWithParents(), Is.EqualTo(client.ShortName));
			Assert.That(client.GetIdWithParentId(), Is.EqualTo("1"));

			var paretnClient1 = new Client {Id = 2, ShortName = "Parent1"};
			client.Parents.Add(new Relationship { RelationshipType = RelationshipType.Base, Parent = paretnClient1});
			Assert.That(client.GetSubordinateType(), Is.EqualTo("Базовый"));
			Assert.That(client.GetNameWithParents(), Is.EqualTo("Test1[Parent1]"));
			Assert.That(client.GetIdWithParentId(), Is.EqualTo("1[2]"));

			var paretnClient2 = new Client { Id = 3, ShortName = "Parent2" };
			client.Parents.Add(new Relationship { RelationshipType = RelationshipType.Hidden, Parent = paretnClient2 });
			Assert.That(client.GetSubordinateType(), Is.EqualTo("Базовый"));
			Assert.That(client.GetNameWithParents(), Is.EqualTo("Test1[Parent1]"));
			Assert.That(client.GetIdWithParentId(), Is.EqualTo("1[2]"));
		}

		[Test]
		public void ResetUinTest()
		{
			var client = ForTest.CreateClient();
			var retupdate = new RetUpdateInfo { UniqueCopyID = "123" };

			using (new TransactionScope())
			{
				client.SaveAndFlush();
				retupdate.Id = client.Id;
				retupdate.Save();
			}

			client.ResetUin();

			var retUpdateInfo = RetUpdateInfo.Get(client.Id);
			Assert.That(retUpdateInfo.UniqueCopyID, Is.Empty);
		}
	}
}
