using System;
using System.Linq.Expressions;
using AdminInterface.NHibernateExtentions;
using Castle.ActiveRecord.Framework;

namespace AdminInterface.Models.Billing
{
	public enum BalanceUpdaterType
	{
		Credit,
		Debit
	}

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
		protected BalanceUpdaterType BalanceType;

		protected BalanceUpdater()
		{}

		protected BalanceUpdater(BalanceUpdaterType type)
		{
			BalanceType = type;
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

			if (BalanceType == BalanceUpdaterType.Debit)
				payer.Balance += sum;
			else
				payer.Balance -= sum;
		}

		private void Apply(Payer payer, decimal sum)
		{
			if (payer == null)
				return;

			if (BalanceType == BalanceUpdaterType.Debit)
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

		public decimal BalanceAmount
		{
			get
			{
				if (BalanceType == BalanceUpdaterType.Debit)
					return Decimal.Negate(GetSum());
				else
					return GetSum();
			}
		}
	}
}