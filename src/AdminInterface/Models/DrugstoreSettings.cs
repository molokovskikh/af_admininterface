using System;
using System.ComponentModel;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	public enum DrugstoreType
	{
		Standart = 0,
		Hidden = 2
	}

	[ActiveRecord("usersettings.RetClientsSet")]
	public class DrugstoreSettings : ActiveRecordBase<DrugstoreSettings>
	{
		public DrugstoreSettings() {}

		public DrugstoreSettings(uint id)
		{
			Id = id;
		}

		[PrimaryKey("ClientCode", Generator = PrimaryKeyType.Assigned)]
		public uint Id { get; set; }

		[Property, Description("Сотрудник АК \"Инфорум\"")]
		public bool ServiceClient { get; set; }

		[Property, Description("Скрыть клиента в интефрейсе поставщика")]
		public DrugstoreType InvisibleOnFirm { get; set; }

		[Property]
		public uint? FirmCodeOnly { get; set; }

		[Property]
		public ulong WorkRegionMask { get; set; }

		[Property]
		public ulong OrderRegionMask { get; set; }

		[Property, Description("Активировать механизм аптечной корректировки цен")]
		public bool AllowDelayOfPayment { get; set; }

		[Property(NotNull = true)]
		public string BasecostPassword { get; set; }

		[Property, Description("Принимать накладные от клиента")]
		public bool SendWaybillsFromClient { get; set; }

		[Property, Description("Показывать рекламу в AnalitF")]
		public bool ShowAdvertising { get; set; }

		[Property, Description("Передавать розничную цену")]
		public bool SendRetailMarkup { get; set; }

		[Property, Description("Разбирать накладные")]
		public bool ParseWaybills { get; set; }

		[Property, Description("Показывать новую форму обработки дефектуры")]
		public bool ShowNewDefecture { get; set; }

		[Property, Description("Разрешить сопоставление вручную в AnalitOnline")]
		public bool ManualComparison { get; set; }

		[Property, Description("Проверять максимальный недельный заказ")]
		public bool CheckWeeklyOrdersSum { get; set; }

		[Property, Description("Максимальный недельный заказ")]
		public uint MaxWeeklyOrdersSum { get; set; }

		[Property, Description("Не подключать новые прайс-листы")]
		public bool IgnoreNewPrices { get; set; }

		public bool IsNoised
		{
			get { return FirmCodeOnly != null; }
		}
	}
}
