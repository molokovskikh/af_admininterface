﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Models.Listeners;
using AdminInterface.Models.Suppliers;
using AdminInterface.NHibernateExtentions;
using Castle.ActiveRecord;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;
using Common.Web.Ui.NHibernateExtentions;

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

	public enum WaybillConvertFormat
	{
		[Description("SST")] SST = 0,
		[Description("Dbf для экспорта накладных Протек")] DBF = 1,
		[Description("SST для Здоровые Люди")] SSTLong = 2,
		[Description("Устаревшая версия универсального dbf")] LessUniversalDbf = 3,
		[Description("Универсальный dbf")] UniversalDbf = 4,
	}

	[ActiveRecord("RetClientsSet", Schema = "usersettings", Lazy = true), Auditable]
	public class DrugstoreSettings
	{
		private bool _noiseCosts;
		private Supplier _noiseCostExceptSupplier;

		public DrugstoreSettings()
		{
			ParseWaybills = true;
			ShowAdvertising = true;
			EnableSmartOrder = true;
		}

		public DrugstoreSettings(Client client)
			: this()
		{
			Client = client;
		}

		[PrimaryKey("ClientCode", Generator = PrimaryKeyType.Foreign)]
		public virtual uint Id { get; set; }

		[OneToOne]
		public virtual Client Client { get; set; }

		[Property, Description("Не доставлять заказы поставщикам"), Auditable]
		public virtual bool ServiceClient { get; set; }

		[Property, Description("Тип клиента в интефрейсе поставщика"), Auditable]
		public virtual DrugstoreType InvisibleOnFirm { get; set; }

		[Property(NotNull = true), Description("Зашумлять цены"), Auditable]
		public virtual bool NoiseCosts
		{
			get { return _noiseCosts; }
			set
			{
				if (_noiseCosts != value) {
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
		public virtual Supplier NoiseCostExceptSupplier
		{
			get { return _noiseCostExceptSupplier; }
			set
			{
				_noiseCostExceptSupplier = value;
				if (NoiseCosts) {
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

		[Property, Description("Принимать накладные от клиента"), Auditable]
		public virtual bool SendWaybillsFromClient { get; set; }

		[Property, Description("Показывать рекламу в AnalitF"), Auditable, ResetReclameDate]
		public virtual bool ShowAdvertising { get; set; }

		[Property, Description("Передавать розничную цену (работа по договору комиссии)"), Auditable]
		public virtual bool SendRetailMarkup { get; set; }

		[Property, Description("Разбирать накладные"), Auditable]
		public virtual bool ParseWaybills { get; set; }

		[Property, Description("Проверять максимальный недельный заказ"), Auditable]
		public virtual bool CheckWeeklyOrdersSum { get; set; }

		[Property, Description("Максимальный недельный заказ"), Auditable]
		public virtual uint MaxWeeklyOrdersSum { get; set; }

		[Property, Description("Не подключать новые прайсы \"Административно\""), Auditable]
		public virtual bool IgnoreNewPrices { get; set; }

		[Property, Description("Не подключать новые прайсы \"В работе\""), Auditable]
		public virtual bool IgnoreNewPriceForUser { get; set; }

		[Description("Скрыть клиента в интефрейсе поставщика, не доставлять заказы поставщикам")]
		public virtual bool IsHiddenFromSupplier
		{
			get { return InvisibleOnFirm == DrugstoreType.Hidden; }
			set
			{
				if (value)
					InvisibleOnFirm = DrugstoreType.Hidden;
				else
					InvisibleOnFirm = DrugstoreType.Standart;
			}
		}

		public virtual bool EnableBuyingMatrix
		{
			get { return BuyingMatrixPrice != null; }
			set
			{
				if (!value)
					BuyingMatrixPrice = null;
			}
		}

		[BelongsTo("BuyingMatrixPriceId"), Description("Ассортиментный прайс для матрицы закупок"), Auditable, SetForceReplication]
		public virtual Price BuyingMatrixPrice { get; set; }

		[Property, Description("Тип матрицы"), Auditable, SetForceReplication]
		public virtual BuyingMatrixType BuyingMatrixType { get; set; }

		[Property, Description("Действие матрицы"), Auditable, SetForceReplication]
		public virtual BuyingMatrixAction WarningOnBuyingMatrix { get; set; }

		[Property, Description("Конвертировать накладные"), Auditable]
		public virtual bool IsConvertFormat { get; set; }

		[Property, Description("Формат для конвертации накладных"), Auditable]
		public virtual WaybillConvertFormat WaybillConvertFormat { get; set; }

		[BelongsTo("AssortimentPriceId"), Description("Ассортиментный прайс для преобразования накладной в формат dbf"), Auditable]
		public virtual Price AssortimentPrice { get; set; }

		[Property, Description("Обезличенный заказ"), Auditable]
		public virtual bool EnableImpersonalPrice { get; set; }

		[Property, Description("Автозаказ"), Auditable]
		public virtual bool EnableSmartOrder { get; set; }

		[Property, Description("Pассчитывать лидеров при получении заказов"), Auditable]
		public virtual bool CalculateLeader { get; set; }

		[BelongsTo("SmartOrderRuleId", Cascade = CascadeEnum.SaveUpdate)]
		public virtual SmartOrderRules SmartOrderRules { get; set; }

		[BelongsTo("OfferMatrixPriceId"), Description("Матрица предложений"), Auditable]
		public virtual Price OfferMatrixPrice { get; set; }

		[Property, Description("Тип матрицы предложений"), Auditable]
		public virtual BuyingMatrixType OfferMatrixType { get; set; }

		[Property(NotNull = true)]
		public virtual string BasecostPassword { get; set; }

		public virtual bool EnableOfferMatrix
		{
			get { return OfferMatrixPrice != null; }
			set
			{
				if (!value)
					OfferMatrixPrice = null;
			}
		}

		[HasAndBelongsToMany(
			Table = "OfferMatrixSuppliers",
			Schema = "Usersettings",
			ColumnKey = "ClientId",
			ColumnRef = "SupplierId",
			Lazy = true)]
		public virtual IList<Supplier> OfferMatrixExcludes { get; set; }

		[Property, Description("Включить расписание обновлений"), Auditable]
		public virtual bool AllowAnalitFSchedule { get; set; }

		[Property, Description("Отображать сертификаты без привязки к поставщику"), Auditable]
		public virtual bool ShowCertificatesWithoutRefSupplier { get; set; }

		[Property, Description("Формат для сохранения накладных Протек"), Auditable]
		public virtual WaybillConvertFormat ProtekWaybillSavingType { get; set; }

		[Property, Description("Отправлять копию заказа на zakaz_copy@analit.net"), Auditable]
		public virtual bool DebugOrders { get; set; }

		public virtual void CheckDefaults()
		{
			if (Client == null)
				return;
			var payer = Client.Payers.FirstOrDefault();
			if (payer != null) {
				payer.ApplySettingsTemplate(this);
			}
		}

		public virtual void GenerateCryptPassword()
		{
			BasecostPassword = new String(Generator.Random(73)
				.Select(i => i + 49)
				.Where(i => !((i > 57 && i < 65) || (i > 90 && i < 97)))
				.Take(16)
				.Select(i => (char)i)
				.ToArray());
		}
	}
}
