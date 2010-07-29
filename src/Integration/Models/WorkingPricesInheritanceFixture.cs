using System.Linq;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Functional.ForTesting;
using NUnit.Framework;

namespace Integration.Models
{
	[TestFixture]
	public class WorkingPricesInheritanceFixture
	{
		[Test]
		public void Inherit_working_prices()
		{
			var client = DataMother.CreateTestClient();
			var parent = new User {
				Client = client,
			};
			var child = new User {
				Client = client,
			};

			using(new SessionScope())
			{
				parent.Setup(client);
				child.Setup(client);

				ArHelper.WithSession(s => {
					var prices = s.CreateSQLQuery(@"
select PriceId
from Future.UserPrices
where UserId = :id")
					.SetParameter("id", parent.Id)
					.List();

					Assert.That(prices, Is.Not.Empty);
					s.CreateSQLQuery(@"
delete from Future.UserPrices
where UserId = :userId and priceId = :priceId")
						.SetParameter("userId", parent.Id)
						.SetParameter("priceId", prices.Cast<uint>().Where(priceId => priceId == 5u).First())
						.ExecuteUpdate();

					child.InheritPricesFrom = parent;
					parent.Update();
				});
			}

			using (new SessionScope())
			{
				var pricesForParent = ArHelper.WithSession(s =>
					s.CreateSQLQuery(@"
call Future.GetPrices(:id);
select * from Usersettings.Prices;")
					.SetParameter("id", parent.Id)
					.List());

				var pricesForChild = ArHelper.WithSession(s =>
					s.CreateSQLQuery(@"
drop temporary table Usersettings.Prices;
call Future.GetPrices(:id);
select * from Usersettings.Prices")
					.SetParameter("id", child.Id)
					.List());

				Assert.That(pricesForParent, Is.EquivalentTo(pricesForChild));
			}
		}
	}
}
