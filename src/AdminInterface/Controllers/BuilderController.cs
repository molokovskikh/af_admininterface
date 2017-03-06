using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AdminInterface.Models;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	public class BuilderController : MvcController
	{
		public class DocViewModel
		{
			public DocViewModel()
			{
				Begin = DateTime.Today.AddMonths(-1);
				End = DateTime.Today;
			}

			public DateTime Begin { get; set; }
			public DateTime End { get; set; }
		}

		public class InventoryViewModel
		{
			public InventoryViewModel()
			{
				Begin = DateTime.Today.AddMonths(-1);
				End = DateTime.Today;
				Markup = 30;
			}

			public DateTime Begin { get; set; }
			public DateTime End { get; set; }
			public decimal Markup { get; set; }
		}

		public ActionResult Docs(uint userId)
		{
			var model = new DocViewModel();
			if (IsPost) {
				if (TryUpdateModel(model)) {
					var docsCount = DbSession.CreateSQLQuery(@"
insert into Logs.DocumentSendLogs(UserId, DocumentId)
select :userId, d.RowId
from (
	select l.RowId
	from Logs.Document_logs l
		join Customers.UserAddresses ua on ua.AddressId = l.AddressId
			join Customers.Addresses a on a.Id = ua.AddressId
		left join Logs.DocumentSendLogs sl on sl.DocumentId = l.RowId and sl.UserId = :userId
	where a.Enabled = 1
		and ua.UserId = :userId
		and sl.Id is null
		and l.WriteTime > :begin
		and l.WriteTime < :end
) as d")
						.SetParameter("begin", model.Begin)
						.SetParameter("end", model.End.AddDays(1))
						.SetParameter("userId", userId)
						.ExecuteUpdate();
					if (docsCount > 0)
						Notify($"Доступно к загрузке новый документов {docsCount}");
					else
						Warn("Документов не найдено");
					return RedirectToAction(nameof(Docs), new { userId });
				}
			}
			ViewBag.User = DbSession.Load<User>(userId);
			return View(model);
		}

		public ActionResult Inventory(uint userId)
		{
			var model = new InventoryViewModel();
			if (IsPost) {
				if (TryUpdateModel(model)) {
					var count = DbSession.CreateSQLQuery(@"
insert into Inventory.Stocks(WaybillLineId,
	Status,
	AddressId,
	ProductId,
	CatalogId,
	Product,
	ProducerId,
	Producer,
	Country,
	ProducerCost,
	RegistryCost,
	SupplierPriceMarkup,
	SupplierCostWithoutNds,
	SupplierCost,
	Quantity,
	SupplyQuantity,
	Nds,
	SerialNumber,
	NdsAmount,
	Unit,
	BillOfEntryNumber,
	ExciseTax,
	VitallyImportant,
	Period,
	Exp,
	Certificates,
	Barcode,
	CountryCode,
	RetailCost,
	WaybillNumber,
	SupplierId,
	SupplierFullName
)
select db.Id,
	:status,
	dh.AddressId,
	db.ProductId,
	p.CatalogId,
	IFNULL(c.Name, db.Product),
	db.ProducerId,
	db.Producer,
	db.Country,
	db.ProducerCost,
	db.RegistryCost,
	db.SupplierPriceMarkup,
	db.SupplierCostWithoutNds,
	db.SupplierCost,
	db.Quantity,
	db.Quantity,-- SupplyQuantity
	db.Nds,
	db.SerialNumber,
	db.NdsAmount,
	db.Unit,
	db.BillOfEntryNumber,
	db.ExciseTax,
	db.VitallyImportant,
	db.Period,
	str_to_date(db.Period, '%d.%m.%Y') as Exp,
	db.Certificates,
	db.EAN13 as Barcode,
	db.CountryCode,
	round(db.SupplierCost + db.SupplierCost * :markup, 2) as RetailCost,
	dh.ProviderDocumentId,
	dh.FirmCode,
	sp.FullName
from Customers.UserAddresses ua
	join Customers.Addresses a on a.Id = ua.AddressId
		join Documents.DocumentHeaders dh on dh.Addressid = a.Id
			join Documents.DocumentBodies db on db.DocumentId = dh.Id
				left join Catalogs.Products p on p.Id = db.ProductId
				left join Catalogs.Catalog c on c.Id = p.CatalogId
				left join Customers.Suppliers sp on sp.Id = dh.FirmCode
				left join Inventory.Stocks s on s.WaybillLineId = db.Id
where ua.UserId = :userId
	and a.Enabled = 1
	and s.Id is null
	and db.SupplierCost is not null
	and dh.WriteTime > :begin
	and dh.WriteTime < :end")
						.SetParameter("begin", model.Begin)
						.SetParameter("end", model.End.AddDays(1))
						.SetParameter("userId", userId)
						.SetParameter("markup", model.Markup)
						//Available
						.SetParameter("status", 0)
						.ExecuteUpdate();
					if (count > 0)
						Notify($"Создано складских остатков {count}");
					else
						Warn("Складских остатков не создано");
					return RedirectToAction(nameof(Inventory), new { userId });
				}
			}
			ViewBag.User = DbSession.Load<User>(userId);
			return View(model);
		}
	}
}