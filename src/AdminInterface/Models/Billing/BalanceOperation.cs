using System;
using System.ComponentModel;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.Components.Validator;

namespace AdminInterface.Models.Billing
{
	public enum OperationType
	{
		[Description("Возврат")] Refund,
		[Description("Списание")] DebtRelief
	}

	[ActiveRecord(Schema = "Billing")]
	public class BalanceOperation : BalanceUpdater<BalanceOperation>
	{
		private OperationType _type;

		public BalanceOperation()
		{}

		public BalanceOperation(Payer payer)
		{
			Payer = payer;
			Date = DateTime.Now;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property, Description("Тип")]
		public virtual OperationType Type
		{
			get { return _type; }
			set
			{
				_type = value;
				BalanceType = _type == OperationType.Refund ? BalanceUpdaterType.Debit : BalanceUpdaterType.Credit;
			}
		}

		[BelongsTo(Cascade = CascadeEnum.SaveUpdate), ValidateNonEmpty, Description("Плательщик")]
		public override Payer Payer { get; set; }

		[Property, ValidateNonEmpty, Description("Основание")]
		public virtual string Description { get; set; }

		[Property, ValidateNonEmpty, Description("Дата")]
		public virtual DateTime Date { get; set; }

		[Property, ValidateGreaterThanZero, Description("Сумма")]
		public virtual decimal Sum { get; set; }

		protected override decimal GetSum()
		{
			return Sum;
		}

		protected override string GetSumProperty()
		{
			return "Sum";
		}
	}
}