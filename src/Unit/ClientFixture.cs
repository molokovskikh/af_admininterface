using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models;
using NUnit.Framework;

namespace Unit
{
	[TestFixture]
	public class ClientFixture
	{
		[Test]
		public void On_region_change_add_for_user_only_new_regions()
		{
			var client = new Client();
			client.HomeRegion = new Region{
				Id = 1,
				Name = "Воронеж"
			};
			client.Settings = new DrugstoreSettings();
			client.Settings.OrderRegionMask = 1 | 2;
			client.MaskRegion = 1 | 2;

			client.Users = new List<User> {
				new User(client) { WorkRegionMask = 1, OrderRegionMask = 1},
			};

			client.UpdateRegionSettings(new [] {
				new RegionSettings{Id = 1, IsAvaliableForBrowse = true, IsAvaliableForOrder = true},
				new RegionSettings{Id = 2, IsAvaliableForBrowse = true, IsAvaliableForOrder = true},
				new RegionSettings{Id = 4, IsAvaliableForBrowse = true, IsAvaliableForOrder = true},
			});

			var user = client.Users.First();
			Assert.That(user.WorkRegionMask, Is.EqualTo(1 | 4));
			Assert.That(user.OrderRegionMask, Is.EqualTo(1 | 4));
		}
	}
}