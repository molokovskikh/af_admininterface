using System.ComponentModel;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing"), Description("Номенклатура")]
	public class Nomenclature
	{
		public Nomenclature()
		{
		}

		public Nomenclature(string name)
		{
			Name = name;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[Property, ValidateNonEmpty, ValidateIsUnique("Такое значение уже существует")]
		public string Name { get; set; }
	}
}