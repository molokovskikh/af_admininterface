using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Descriptors;
using Common.Web.Ui.Controllers;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	[Secure(PermissionType.EditSettings)]
	public class CostOptimizationController : AdminInterfaceController
	{
		public void Index()
		{
			PropertyBag["regions"] = DbSession.Query<Region>().OrderBy(r => r.Name).ToArray();
			PropertyBag["concurrents"] = DbSession.Query<CostOptimizationForbiddenConcurrent>()
				.OrderBy(x => x.Supplier.HomeRegion.Name).ThenBy(x => x.Supplier.Name)
				.Select(x => x.Supplier)
				.ToList()
				.Where(x => (x.HomeRegion.Id & Admin.RegionMask) > 0)
				.ToList();
			PropertyBag["clients"] = DbSession.Query<CostOptimizationForbiddenClient>()
				.OrderBy(x => x.Client.HomeRegion.Name).ThenBy(x => x.Client.Name)
				.Select(x => x.Client)
				.ToList()
				.Where(x => (x.HomeRegion.Id & Admin.RegionMask) > 0)
				.ToList();
		}

		public void AddConcurrent()
		{
			var supplier = DbSession.Load<Supplier>(Convert.ToUInt32(Form["supplierId"]));
			if (DbSession.Query<CostOptimizationForbiddenConcurrent>().Any(c => c.Supplier == supplier)) {
				Error(String.Format("Поставщик {0} уже исключен", supplier.Name));
				RedirectToAction("Index");
				return;
			}
			DbSession.Save(new CostOptimizationForbiddenConcurrent(supplier));
			Notify("Сохранено");
			RedirectToAction("Index");
		}

		public void AddClient()
		{
			var client = DbSession.Load<Client>(Convert.ToUInt32(Form["clientId"]));
			if (DbSession.Query<CostOptimizationForbiddenClient>().Any(c => c.Client == client)) {
				Error(String.Format("Клиент {0} уже исключен", client.Name));
				RedirectToAction("Index");
				return;
			}
			DbSession.Save(new CostOptimizationForbiddenClient(client));
			Notify("Сохранено");
			RedirectToAction("Index");
		}

		public void DeleteConcurrent(uint id)
		{
			var concurrent = DbSession.Query<CostOptimizationForbiddenConcurrent>().Where(c => c.Supplier.Id == id).ToArray();
			DbSession.DeleteMany(concurrent);
			Notify("Удалено");
			RedirectToAction("Index");
		}

		public void DeleteClient(uint id)
		{
			var concurrent = DbSession.Query<CostOptimizationForbiddenClient>().Where(c => c.Client.Id == id).ToArray();
			DbSession.DeleteMany(concurrent);
			Notify("Удалено");
			RedirectToAction("Index");
		}

		[return: JSONReturnBinder]
		[RequiredPermission(PermissionType.ViewSuppliers)]
		public object[] Concurrents(string q, ulong? regionId)
		{
			q = q ?? "";
			regionId = regionId & Admin.RegionMask ?? Admin.RegionMask;
			if (regionId == 0)
				regionId = Admin.RegionMask;
			return DbSession.CreateSQLQuery(@"
select s.Id, s.Name, r.Region
from Customers.Suppliers s
	join Farm.Regions r on r.RegionCode = s.HomeRegion
where s.Name like :term
	and s.HomeRegion & :mask > 0
	and s.Disabled = 0
order by s.Name, r.Region")
				.SetParameter("term", "%" + q + "%")
				.SetParameter("mask", regionId.Value)
				.List<object[]>()
				.Select(x => new {
					id = x[0],
					name = x[1],
					regionName = x[2]
				})
				.ToArray();
		}

		[return: JSONReturnBinder]
		[RequiredPermission(PermissionType.ViewDrugstore)]
		public object[] Clients(string q, ulong? regionId)
		{
			q = q ?? "";
			regionId = regionId & Admin.RegionMask ?? Admin.RegionMask;
			if (regionId == 0)
				regionId = Admin.RegionMask;
			return DbSession.CreateSQLQuery(@"
select c.Id, c.Name, r.Region
from Customers.Clients c
	join Farm.Regions r on r.RegionCode = c.RegionCode
where c.Name like :term
	and c.RegionCode & :mask > 0
	and c.Status = 1
order by c.Name, r.Region")
				.SetParameter("term", "%" + q + "%")
				.SetParameter("mask", regionId.Value)
				.List<object[]>()
				.Select(x => new {
					id = x[0],
					name = x[1],
					regionName = x[2]
				})
				.ToArray();
		}
	}
}