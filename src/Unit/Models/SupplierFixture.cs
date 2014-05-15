using System;
using System.Collections.Generic;
using AdminInterface.Models.Billing;
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
			supplier.RegionMask = 1 | 10 | 2;
			supplier.Prices.Add(new Price {
				Enabled = true,
				AgencyEnabled = true,
				RegionalData = new List<PriceRegionalData> {
					new PriceRegionalData {
						Region = new Region {
							Id = 1,
							Name = "1"
						},
						Enabled = true
					},
					new PriceRegionalData {
						Region = new Region {
							Id = 10,
							Name = "10"
						},
						Enabled = true
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
						},
						Enabled = true
					}
				}
			});
			var regions = supplier.PricesRegions;
			Assert.That(regions.Count, Is.EqualTo(3));
			Assert.That(regions[0].Id, Is.EqualTo(1));
		}

		[Test]
		public void Merge_person()
		{
			var supplier = new Supplier(new Region("Вороне"), new Payer("тест"));
			supplier.ContactGroupOwner = new ContactGroupOwner();
			var person = new Person("Иван Иванович", new[] { new Contact(ContactType.Phone, "473-2898721") });
			supplier.MergePerson(ContactGroupType.General, person);

			Assert.AreEqual(1, supplier.ContactGroupOwner.Group(ContactGroupType.General).Persons.Count);

			supplier = new Supplier(new Region("Вороне"), new Payer("тест"));
			supplier.ContactGroupOwner = new ContactGroupOwner();
			var group = supplier.ContactGroupOwner.AddContactGroup(ContactGroupType.General);
			group.Persons.Add(new Person("Иван Иванович"));
			group.Adopt();
			person = new Person("Иван Иванович", new[] { new Contact(ContactType.Phone, "473-2898721") });
			supplier.MergePerson(ContactGroupType.General, person);

			Assert.AreEqual(1, supplier.ContactGroupOwner.Group(ContactGroupType.General).Persons[0].Contacts.Count);
		}
	}
}
