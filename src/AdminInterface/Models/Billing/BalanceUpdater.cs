using System;
using System.Linq.Expressions;
using AdminInterface.NHibernateExtentions;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.MonoRailExtentions;

namespace AdminInterface.Models.Billing
{
	public interface IBalanceUpdater
	{
		/// <summary>
		/// сумма изменения баланся с учетом знака
		/// "-" - списание
		/// "+" - пополнение
		/// </summary>
		decimal BalanceAmount { get; }
	}

	public abstract class BalanceUpdater<T> : ActiveRecordLinqBase<T>, IBalanceUpdater
	{
		public abstract Payer Payer { get; set; }

		public abstract decimal BalanceAmount { get; protected set; }

		protected override void OnSave()
		{
			UpdateBalance();
		}

		protected override void OnUpdate()
		{
			if (!IsValid())
				return;

			UpdateBalance();
		}

		protected virtual bool IsValid()
		{
			if (ValidEventListner.ValidatorAccessor != null)
				return ValidEventListner.ValidatorAccessor.Validator.IsValid(this);

			return true;
		}

		protected override void OnDelete()
		{
			ResetBalance();
		}

		private void Reset(Payer payer, decimal sum)
		{
			if (payer == null)
				return;

			payer.Balance -= sum;
		}

		private void Apply(Payer payer, decimal sum)
		{
			if (payer == null)
				return;

			payer.Balance += sum;
		}

		private void ResetBalance()
		{
			Reset(Payer, BalanceAmount);
		}

		private void UpdateBalance()
		{
			var oldPayer = this.OldValue(p => p.Payer);
			var oldSum = this.OldValue(p => p.BalanceAmount);

			if (this.IsChanged(p => p.Payer) || this.IsChanged(p => p.BalanceAmount))
			{
				Reset(oldPayer, oldSum);
				Apply(Payer, BalanceAmount);

				if (oldPayer != null)
					oldPayer.Save();
				if (Payer != null)
					Payer.Save();
			}
		}
	}
}