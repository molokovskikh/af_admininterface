using System;
using System.Collections.Generic;
using System.ComponentModel;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.Components.Validator;
using Common.Web.Ui.MonoRailExtentions;

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
				if (_type == OperationType.Refund)
					BalanceAmount = Math.Abs(BalanceAmount) * -1;
				else
					BalanceAmount = Math.Abs(BalanceAmount);
			}
		}

		[BelongsTo(Cascade = CascadeEnum.SaveUpdate), ValidateNonEmpty, Description("Плательщик")]
		public override Payer Payer { get; set; }

		[Property, ValidateNonEmpty, Description("Основание")]
		public virtual string Description { get; set; }

		[Property, ValidateNonEmpty, Description("Дата")]
		public virtual DateTime Date { get; set; }

		[Property, ValidateGreaterThanZero, Description("Сумма")]
		public virtual decimal Sum
		{
			get
			{
				return Math.Abs(BalanceAmount);
			}
			set
			{
				BalanceAmount = Type == OperationType.Refund ? Decimal.Negate(value) : value;
			}
		}

		[Property]
		public override decimal BalanceAmount { get; protected set; }

		public virtual IEnumerable<ModelAction> Actions
		{
			get
			{
				return new [] {
					new ModelAction(this, "Delete", "Удалить")
				};
			}
		}
	}
}