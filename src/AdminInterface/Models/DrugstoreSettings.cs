﻿using System;
using System.ComponentModel;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	public enum DrugstoreType
	{
		[Description("Стандартный")] Standart = 0,
		[Description("Скрытый")] Hidden = 2
	}

	public enum BuyingMatrixType
	{
		[Description("Черный список")] BlackList = 1,
		[Description("Белый список")] WhiteList = 0,
	}

	public enum BuyingMatrixAction
	{
		[Description("Запретить заказ")] Block = 0,
		[Description("Выводить предупреждения")] Warning = 1,
	}

	[ActiveRecord("RetClientsSet", Schema = "usersettings", Lazy = true)]
	public class DrugstoreSettings : ActiveRecordBase<DrugstoreSettings>
	{
		private bool _noiseCosts;
		private Supplier _noiseCostExceptSupplier;

		public DrugstoreSettings() {}

		public DrugstoreSettings(uint id)
		{
			Id = id;
		}

		[PrimaryKey("ClientCode", Generator = PrimaryKeyType.Assigned)]
		public virtual uint Id { get; set; }

		[Property, Description("Сотрудник АК \"Инфорум\""), Auditable]
		public virtual bool ServiceClient { get; set; }

		[Property, Description("Тип клиента в интефрейсе поставщика"), Auditable]
		public virtual DrugstoreType InvisibleOnFirm { get; set; }

		[Property(NotNull = true), Description("Зашумлять цены"), Auditable]
		public virtual bool NoiseCosts
		{
			get { return _noiseCosts; }
			set
			{
				if (_noiseCosts != value)
				{
					var supplierId = GetSupplierId();
					FirmCodeOnly = value
						? (uint?)supplierId
						: null;
				}
				_noiseCosts = value;
			}
		}

		private uint GetSupplierId()
		{
			uint supplierId = 0;
			if (NoiseCostExceptSupplier != null)
				supplierId = NoiseCostExceptSupplier.Id;
			return supplierId;
		}

		[BelongsTo("NoiseCostExceptSupplierId"), Description("Зашумлять все прайс листы всех поставщиков кроме"), Auditable]
		public virtual Supplier NoiseCostExceptSupplier {
			get { return _noiseCostExceptSupplier; }
			set
			{
				_noiseCostExceptSupplier = value;
				if (NoiseCosts)
				{
					FirmCodeOnly = GetSupplierId();
				}
			}
		}

		[Property]
		public virtual uint? FirmCodeOnly { get; set; }

		[Property]
		public virtual ulong WorkRegionMask { get; set; }

		[Property, Description("Регионы заказа"), Auditable]
		public virtual ulong OrderRegionMask { get; set; }

		[Property, Description("Активировать механизм аптечной корректировки цен (CreditNote, пересчет отсрочек в цены)"), Auditable]
		public virtual bool AllowDelayOfPayment { get; set; }

		[Property(NotNull = true)]
		public virtual string BasecostPassword { get; set; }

		[Property, Description("Принимать накладные от клиента"), Auditable]
		public virtual bool SendWaybillsFromClient { get; set; }

		[Property, Description("Показывать рекламу в AnalitF"), Auditable]
		public virtual bool ShowAdvertising { get; set; }

		[Property, Description("Передавать розничную цену (работа по договору комиссии)"), Auditable]
		public virtual bool SendRetailMarkup { get; set; }

		[Property, Description("Разбирать накладные"), Auditable]
		public virtual bool ParseWaybills { get; set; }

		[Property, Description("Показывать новую форму обработки дефектуры"), Auditable]
		public virtual bool ShowNewDefecture { get; set; }

		[Property, Description("Разрешить сопоставление вручную в AnalitOnline"), Auditable]
		public virtual bool ManualComparison { get; set; }

		[Property, Description("Проверять максимальный недельный заказ"), Auditable]
		public virtual bool CheckWeeklyOrdersSum { get; set; }

		[Property, Description("Максимальный недельный заказ"), Auditable]
		public virtual uint MaxWeeklyOrdersSum { get; set; }

		[Property, Description("Не подключать новые прайс-листы"), Auditable]
		public virtual bool IgnoreNewPrices { get; set; }

		[BelongsTo("BuyingMatrixPriceId"), Description("Ассортиментный прайс для матрицы закупок"), Auditable]
		public virtual Price BuyingMatrixPrice { get; set; }

		[Property, Description("Тип матрицы"), Auditable]
		public virtual BuyingMatrixType BuyingMatrixType { get; set; }

		[Property, Description("Действие матрицы"), Auditable]
		public virtual BuyingMatrixAction WarningOnBuyingMatrix { get; set; }

		[Property, Description("Обезличенный заказ"), Auditable]
		public virtual bool EnableImpersonalPrice { get; set; }

		[Property, Description("Автозаказ"), Auditable]
		public virtual bool EnableSmartOrder { get; set; }

		[Property, Description("Pассчитывать лидеров при получении заказов"), Auditable]
		public virtual bool CalculateLeader { get; set; }
	}
}
