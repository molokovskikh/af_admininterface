using System.ComponentModel;
using Castle.ActiveRecord;
using Castle.Components.Validator;

namespace AdminInterface.Models.Suppliers
{
	[ActiveRecord(Schema = "OrderSendRules")]
	public class SpecialHandler
	{
		public SpecialHandler()
		{}

		public SpecialHandler(Supplier supplier)
		{
			Supplier = supplier;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property, ValidateNonEmpty, Description("Название")]
		public virtual string Name { get; set; }

		[BelongsTo("SupplierId")]
		public virtual Supplier Supplier { get; set; }

		[BelongsTo("HandlerId"), Description("Обработчик"), ValidateNonEmpty]
		public virtual OrderHandler Handler { get; set;}
	}
}