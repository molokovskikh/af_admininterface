using System;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	public enum DrugstoreType
	{
		Standart,
		Hidden
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

		[Property]
		public bool ServiceClient { get; set; }

		[Property]
		public DrugstoreType InvisibleOnFirm { get; set; }

		[Property]
		public uint? FirmCodeOnly { get; set; }

		[Property]
		public ulong WorkRegionMask { get; set; }

		[Property]
		public ulong OrderRegionMask { get; set; }

		[Property(NotNull = true)]
		public string BasecostPassword { get; set; }

		public bool IsNoised
		{
			get { return FirmCodeOnly != null; }
		}
	}
}
