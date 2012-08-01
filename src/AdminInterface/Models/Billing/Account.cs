using System;
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

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(DiscriminatorColumn = "Type", Schema = "Billing", Lazy = true), Auditable]
	public abstract class Account : ActiveRecordLinqBase<Account>, IAuditable
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
			get
			{
				return _payment;
			}
			set
			{
				if (_payment != value)
				{
					_payment = value;
					Payer.UpdatePaymentSum();
				}
			}
		}

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore)]
		public virtual bool BeAccounted
		{
			get
			{
				return _beAccounted;
			}
			set
			{
				if (_beAccounted != value)
				{
					_beAccounted = value;
					Payer.UpdatePaymentSum();
				}
			}
		}

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore)]
		public virtual bool ReadyForAccounting
		{
			get
			{
				return _readyForAccounting;
			}
			set
			{
				if (_readyForAccounting != value)
				{
					_readyForAccounting = value;
					Payer.UpdatePaymentSum();
				}
			}
		}

		[Property]
		public virtual int InvoiceGroup { get; set; }

		[
			Style,
			Property(Access = PropertyAccess.FieldCamelcaseUnderscore),
			Description("Обслуживается бесплатно"),
			Auditable("Обслуживается бесплатно"),
			RequiredPermission(PermissionType.CanRegisterClientWhoWorkForFree)
		]
		public virtual bool IsFree
		{
			get
			{
				return _isFree;
			}
			set
			{
				if (IsFree != value)
				{
					_isFree = value;
					Payer.UpdatePaymentSum();
				}
			}
		}

		[
			Property,
			Description("Дата окончания бесплатно периода"),
			DependOn("IsFree"),
			RequiredPermission(PermissionType.CanRegisterClientWhoWorkForFree)
		]
		public virtual DateTime? FreePeriodEnd { get; set; }

		public virtual uint PayerId
		{
			get
			{
				return Payer.Id;
			}
		}

		public abstract Payer Payer { get; }

		public abstract string Name { get;}

		public abstract LogObjectType ObjectType { get; }

		public abstract uint ObjectId { get; }

		public virtual string Type
		{
			get
			{
				return BindingHelper.GetDescription(ObjectType);
			}
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

		public virtual string DefaultDescription
		{
			get
			{
				if (Payer.Recipient != null)
					return Payer.Recipient.Description;
				return "";
			}
		}

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

		public static IEnumerable<Account> GetReadyForAccounting(Pager pager)
		{
			var freeEnd = DateTime.Today.AddDays(10);
			var readyForAccounting = Queryable.Where(a => a.ReadyForAccounting
				&& !a.BeAccounted
				&& !(a.IsFree && a.FreePeriodEnd != null && a.FreePeriodEnd > freeEnd)
			);

			pager.Total = readyForAccounting.Count();
			return pager.DoPage(readyForAccounting).ToList();
		}

		public virtual IAuditRecord GetAuditRecord()
		{
			return new PayerAuditRecord(Payer, this);
		}
	}
}
