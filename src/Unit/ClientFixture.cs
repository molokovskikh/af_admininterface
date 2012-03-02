using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using Common.Web.Ui.Models;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class ClientFixture
	{
		Client client;

		[SetUp]
		public void Setup()
		{
			client = new Client(new Payer(), new Region());
			client.HomeRegion = new Region{
				Id = 1,
				Name = "Воронеж"
			};
			client.Settings = new DrugstoreSettings();
			client.Settings.OrderRegionMask = 1 | 2;
			client.Settings.WorkRegionMask = 1 | 2;
			client.MaskRegion = 1 | 2;

			client.Users = new List<User> {
				new User(client) { WorkRegionMask = 1, OrderRegionMask = 1},
			};
		}

		[Test]
		public void On_region_change_add_for_user_only_new_regions()
		{
			client.UpdateRegionSettings(new [] {
				new RegionSettings{Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true},
				new RegionSettings{Id = 2, IsAvaliableForBrowse = true, IsAvaliableForOrder = true},
				new RegionSettings{Id = 4, IsAvaliableForBrowse = true, IsAvaliableForOrder = true},
			});

			var user = client.Users.First();
			Assert.That(user.WorkRegionMask, Is.EqualTo(1 | 4));
			Assert.That(user.OrderRegionMask, Is.EqualTo(1 | 4));
		}

		[Test]
		public void Add_new_region_if_it_not_registred()
		{
			client.UpdateRegionSettings(new [] {
				new RegionSettings{Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true},
				new RegionSettings{Id = 2, IsAvaliableForBrowse = true, IsAvaliableForOrder = true},
				new RegionSettings{Id = 4, IsAvaliableForBrowse = true, IsAvaliableForOrder = true},
			});
			Assert.That(client.Settings.WorkRegionMask, Is.EqualTo(1 | 2 | 4));
		}

		[Test]
		public void If_client_registred_for_special_payer_set_receive_waybills_in_true()
		{
			var payer = new Payer("Тестовый плательщик", "Тестовый плательщик") {
				Id =  921
			};
			client = new Client(payer, Data.DefaultRegion);
			Assert.That(client.Settings.SendWaybillsFromClient, Is.True);
		}
	}
}