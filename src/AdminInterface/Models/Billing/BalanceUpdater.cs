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
		public abstract Payer Payer { get; set; }
		public abstract decimal Sum { get; set; }

		private BalanceUpdaterType _type;

		protected BalanceUpdater(BalanceUpdaterType type)
		{
			_type = type;
		}

		protected override void OnSave()
		{
			UpdateBalance();
			base.OnSave();
		}

		protected override void OnUpdate()
		{
			UpdateBalance();
			base.OnUpdate();
		}

		protected override void OnDelete()
		{
			ResetBalance();
			base.OnDelete();
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
			Reset(Payer, Sum);
		}

		private void UpdateBalance()
		{
			var oldPayer = this.OldValue(p => p.Payer);
			var oldSum = this.OldValue(p => p.Sum);

			if (this.IsChanged(p => p.Payer) || this.IsChanged(p => p.Sum))
			{
				Reset(oldPayer, oldSum);
				Apply(Payer, Sum);

				if (oldPayer != null)
					oldPayer.Save();
				if (Payer != null)
					Payer.Save();
			}
		}
	}
}