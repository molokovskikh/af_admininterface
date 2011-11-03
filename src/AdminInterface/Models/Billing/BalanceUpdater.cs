using System;
using System.Linq.Expressions;
using AdminInterface.NHibernateExtentions;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Models.Billing
{
	public enum BalanceUpdaterType
	{
		Payment,
		ChargeOff
	}

	public abstract class BalanceUpdater<T> : ActiveRecordLinqBase<T>
	{
		private BalanceUpdaterType _type;

		protected BalanceUpdater(BalanceUpdaterType type)
		{
			_type = type;
		}

		public abstract Payer Payer { get; set; }

		protected abstract decimal GetSum();
		protected abstract string GetSumProperty();

		protected override void OnSave()
		{
			UpdateBalance();
		}

		protected override void OnUpdate()
		{
			UpdateBalance();
		}

		protected override void OnDelete()
		{
			ResetBalance();
		}

		private void Reset(Payer payer, decimal sum)
		{
			if (payer == null)
				return;

			if (_type == BalanceUpdaterType.ChargeOff)
				payer.Balance += sum;
			else
				payer.Balance -= sum;
		}

		private void Apply(Payer payer, decimal sum)
		{
			if (payer == null)
				return;

			if (_type == BalanceUpdaterType.ChargeOff)
				payer.Balance -= sum;
			else
				payer.Balance += sum;
		}

		private void ResetBalance()
		{
			Reset(Payer, GetSum());
		}

		private void UpdateBalance()
		{
			var oldPayer = this.OldValue(p => p.Payer);
			var oldSum = this.OldValue<decimal>(GetSumProperty());

			if (this.IsChanged(p => p.Payer) || this.IsChanged(GetSumProperty()))
			{
				Reset(oldPayer, oldSum);
				Apply(Payer, GetSum());

				if (oldPayer != null)
					oldPayer.Save();
				if (Payer != null)
					Payer.Save();
			}
		}
	}
}