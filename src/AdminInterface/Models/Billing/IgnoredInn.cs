using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "Billing")]
	public class IgnoredInn : ActiveRecordLinqBase<IgnoredInn>
	{
		public IgnoredInn()
		{
		}

		public IgnoredInn(string inn)
		{
			Inn = inn;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[Property, ValidateNonEmpty]
		public string Inn { get; set; }
	}
}