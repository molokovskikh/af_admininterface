using System;
using System.Collections.Generic;
using AdminInterface.Models.Suppliers;
using Common.Web.Ui.Models;
using NUnit.Framework;

namespace Unit.Models
{
	[TestFixture]
	public class SupplierFixture
	{
		[Test]
		public void PricesRegionsTest()
		{
			var supplier = new Supplier();
			supplier.Prices.Add(new Price {
				Enabled = true,
				AgencyEnabled = true,
				RegionalData = new List<PriceRegionalData> {
					new PriceRegionalData {
						Region = new Region {
							Id = 1,
							Name = "1"
						}
					},
					new PriceRegionalData {
						Region = new Region {
							Id = 10,
							Name = "10"
						}
					}
				}
			});
			supplier.Prices.Add(new Price {
				Enabled = true,
				AgencyEnabled = true,
				RegionalData = new List<PriceRegionalData> {
					new PriceRegionalData {
						Region = new Region {
							Id = 2,
							Name = "2"
						}
					}
				}
			});
			var regions = supplier.PricesRegions;
			Assert.That(regions.Count, Is.EqualTo(3));
			Assert.That(regions[0].Id, Is.EqualTo(1));
		}
	}
}
