using System.ComponentModel;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing"), Description("Игнорируемые ИНН")]
	public class IgnoredInn : ActiveRecordLinqBase<IgnoredInn>
	{
		public IgnoredInn()
		{
		}

		public IgnoredInn(string name)
		{
			Name = name;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[Property("Inn"), ValidateNonEmpty, ValidateIsUnique("Такое значение уже существует")]
		public string Name { get; set; }
	}
}