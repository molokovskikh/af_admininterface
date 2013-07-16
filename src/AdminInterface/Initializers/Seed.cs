using System;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Models;
using NHibernate.Linq;

namespace AdminInterface.Initializers
{
	public class Seed
	{
		public Administrator Run()
		{
			using (new SessionScope()) {
				return ArHelper.WithSession(s => {
					//нужно только для того что бы запустить в браузере
					var admin = Administrator.GetByName(Environment.UserName);
					if (admin == null) {
						admin = Administrator.CreateLocalAdministrator();
						s.Save(admin);
					}

					var origin = SecurityContext.GetAdministrator;
					SecurityContext.GetAdministrator = () => admin;
					var defaults = s.Query<DefaultValues>().First();
					if (defaults.SmartOrderAssortmentPrice == null) {
						var payer = new Payer("Тестовый плательщик");
						var homeRegion = s.Load<Region>(1UL);
						var supplier = new Supplier(homeRegion, payer) {
							Name = "Тестовый поставщик",
							FullName = "Тестовый поставщик",
							ContactGroupOwner = new ContactGroupOwner(ContactGroupType.ClientManagers)
						};
						supplier.RegionalData.Add(new RegionalData { Region = homeRegion, Supplier = supplier });
						defaults.SmartOrderAssortmentPrice = supplier.AddPrice("Базовый", PriceType.Assortment);
						s.Save(supplier);
					}
					SecurityContext.GetAdministrator = origin;
					return admin;
				});
			}
		}
	}
}