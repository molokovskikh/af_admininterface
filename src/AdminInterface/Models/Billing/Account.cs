﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(DiscriminatorColumn = "Type", Schema = "Billing", Lazy = true), Auditable]
	public abstract class Account : IAuditable
	{
		protected bool _beAccounted;
		protected bool _readyForAccounting;
		protected bool _isFree;
		protected decimal _payment;
		private string _description;

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime? WriteTime { get; set; }

		[Property]
		public virtual string Operator { get; set; }

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore), Auditable("Платеж")]
		public virtual decimal Payment
		{
			get { return _payment; }
			set
			{
				if (_payment != value) {
					_payment = value;
					Payer.UpdatePaymentSum();
				}
			}
		}

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore)]
		public virtual bool BeAccounted
		{
			get { return _beAccounted; }
			set
			{
				if (_beAccounted != value) {
					_beAccounted = value;
					Payer.UpdatePaymentSum();
				}
			}
		}

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore)]
		public virtual bool ReadyForAccounting
		{
			get { return _readyForAccounting; }
			set
			{
				if (_readyForAccounting != value) {
					_readyForAccounting = value;
					Payer.UpdatePaymentSum();
				}
			}
		}

		[Property]
		public virtual int InvoiceGroup { get; set; }

		[
			Property(Access = PropertyAccess.FieldCamelcaseUnderscore),
			Description("Обслуживается бесплатно"),
			Auditable("Бесплатно"),
			RequiredPermission(PermissionType.CanRegisterClientWhoWorkForFree)
		]
		public virtual bool IsFree
		{
			get { return _isFree; }
			set
			{
				if (IsFree != value) {
					_isFree = value;
					Payer.UpdatePaymentSum();
				}
			}
		}

		[Style]
		public virtual bool ConsolidateFree { get { return IsFree || Payment == 0; } }

		[
			Auditable,
			Property,
			Description("Дата окончания бесплатно периода"),
			DependOn("IsFree"),
			RequiredPermission(PermissionType.CanRegisterClientWhoWorkForFree)
		]
		public virtual DateTime? FreePeriodEnd { get; set; }

		public virtual uint PayerId
		{
			get { return Payer.Id; }
		}

		public abstract Payer Payer { get; }

		public abstract string Name { get; }

		public abstract LogObjectType ObjectType { get; }

		public abstract uint ObjectId { get; }

		public abstract bool Enabled { get; }

		public virtual string Comment { get; set; }

		public virtual string Type
		{
			get { return BindingHelper.GetDescription(ObjectType); }
		}

		public virtual Service Service
		{
			get { return null; }
		}

		public virtual bool Status
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore)]
		public virtual string Description
		{
			get
			{
				if (String.IsNullOrEmpty(_description))
					return DefaultDescription;
				return _description;
			}

			set
			{
				if (_description == DefaultDescription)
					_description = null;
				_description = value;
			}
		}

		public abstract string DefaultDescription { get; }

		public virtual string RegistrationMessage
		{
			get
			{
				if (!IsFree)
					return "";
				var message = "Регистрация бесплатная";
				if (FreePeriodEnd != null)
					message += " до " + FreePeriodEnd.Value.ToShortDateString();
				return message;
			}
		}

		public virtual void Accounted()
		{
			Operator = SecurityContext.Administrator.UserName;
			WriteTime = DateTime.Now;
			BeAccounted = true;
		}

		public virtual bool ShouldPay()
		{
			return (BeAccounted || ReadyForAccounting) && !IsFree && Payment > 0;
		}

		public static IEnumerable<Account> GetReadyForAccounting(Pager pager, ISession session)
		{
			var freeEnd = DateTime.Today.AddDays(10);
			var readyForAccounting = session.Query<Account>().Where(a => a.ReadyForAccounting
				&& !a.BeAccounted
				&& !(a.IsFree && a.FreePeriodEnd != null && a.FreePeriodEnd > freeEnd));
			var allResult = readyForAccounting.ToList().Where(a => a.Enabled);
			pager.Total = allResult.Count();
			return pager.DoPage(allResult).ToList();
		}

		public virtual IAuditRecord GetAuditRecord()
		{
			return new PayerAuditRecord(Payer, this, Comment);
		}
	}
}